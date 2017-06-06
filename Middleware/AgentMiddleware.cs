using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NewRelicAgentMiddleware.Agent;
using NewRelicAgentMiddleware.Transactions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewRelicAgentMiddleware.Configuration;
using NewRelicAgentMiddleware.Extensions;
using NewRelicAgentMiddleware.LibWrappers;

namespace NewRelicAgentMiddleware.Middleware
{
    public class AgentMiddleware {
        private readonly RequestDelegate next;
        private readonly ILogger<AgentMiddleware> logger;
        private readonly IAgentSdk agentSdk;
        private readonly NewRelicOptions options;
        private bool isEnabled;
        // The following is completely arbitrary, presently the logger
        // requires an event id if you want to log an exception
        private const int errorEventId = 1000;
        
        public AgentMiddleware(RequestDelegate next, ILogger<AgentMiddleware> logger, IAgentSdk agentSdk, IOptions<NewRelicOptions> optionsAccessor, bool isEnabled = true)
        {
            this.next = next;
            this.logger = logger;
            this.agentSdk = agentSdk;
            this.isEnabled = isEnabled;
            this.options = optionsAccessor.Value;
            
            // New Relic agent is only compatible on Linux
            if (IsEnabledOnLinux())
            {
                InitializeAgent();
            }
        }

        private void InitializeAgent()
        {
            // If we fail to initialize the agent, disable the middleware
            var success = agentSdk.Init(options.LicenseKey, options.AppName, options.Language, options.LanguageVersion);
            if (!success)
            {
                isEnabled = false;
            }
        }

        /// <summary>
        /// To initialize the New Relic Core Agent for Linux, we must ensure
        /// that we are actually running on Linux, and that the middleware hasn't
        /// been disabled in the config file
        /// </summary>
        private bool IsEnabledOnLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && isEnabled;
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsEnabledOnLinux())
            {
                await InvokeWithTelemetry(context);
            }
            else
            {
                await next.Invoke(context);
            }
        }

        private async Task InvokeWithTelemetry(HttpContext context)
        {
            long transactionId;

            // If the New Relic SDK throws an exception,
            // we'll catch the exception but ensure processing continues
            // by immediately invoking the next middleware
            try
            {
                transactionId = agentSdk.BeginTransaction();
            }
            catch (Exception exc)
            {
                logger.LogError(errorEventId, exc, "Could not begin New Relic transaction");
                await next.Invoke(context);
                return;
            }

            try
            {
                await next.Invoke(context);

                const int minClientErrorStatusCode = 400;
                const int maxClientErrorStatusCode = 499;
                var statusCode = context.Response.StatusCode;

                if (statusCode >= minClientErrorStatusCode && statusCode <= maxClientErrorStatusCode)
                {
                    var error = $"HttpError {statusCode}";
                    var message = ((HttpStatusCode) statusCode).ToString();
                    var stackTrace = String.Empty;
                    agentSdk.NoticeError(transactionId, error, message, stackTrace);
                    agentSdk.SetTransactionName(transactionId, context.Request.Path);
                }
                else
                {
                    var name = GetTransactionName(context);
                    agentSdk.SetTransactionName(transactionId, name);
                }

                try
                {
                    agentSdk.EndTransaction(transactionId);
                }
                catch (Exception exc)
                {
                    logger.LogError(errorEventId, exc, "Could not end New Relic transaction");
                }
            }
            catch (Exception exc)
            {
                // These exceptions are our Internal Server Errors, we will 
                // publish the errors to New Relic
                var name = GetTransactionName(context);
                agentSdk.SetTransactionName(transactionId, name);

                var error = exc.GetType().ToString();
                var message = exc.Message;
                var stackTrace = exc.StackTrace;

                agentSdk.NoticeError(transactionId, error, message, stackTrace);
                agentSdk.EndTransaction(transactionId);
                throw;
            }
        }

        private string GetTransactionName(HttpContext context)
        {
            var transactionLabeller = TransactionLabeller<JsonMappingsRepository>.Instance;
            var controller = context.GetRouteValue("controller");
            var action = context.GetRouteValue("action");
            return transactionLabeller.GetTransactionLabel(controller.ToString(), action.ToString(), context.Request.Path);
        }
    }
}