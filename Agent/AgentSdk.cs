using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewRelicAgentMiddleware.Configuration;
using NewRelicAgentMiddleware.Extensions;
using NewRelicAgentMiddleware.LibWrappers;

namespace NewRelicAgentMiddleware.Agent
{
    internal class AgentSdk : IAgentSdk
    {
        private ILogger<AgentSdk> logger;
        private readonly NewRelicOptions options;
        // The following is completely arbitrary, presently the logger
        // requires an event id if you want to log an exception
        private const int errorEventId = 1001;
        public AgentSdk(ILogger<AgentSdk> logger, IOptions<NewRelicOptions> optionsAccessor)
        {
            this.logger = logger;
            this.options = optionsAccessor.Value;
        }

        public int RecordMetric(string name, double value) {
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            var nameSb = name.ToStringBuilder();
            return AgentSdkWrapper.newrelic_record_metric(nameSb, value);
        }

        public long BeginTransaction() {
            return AgentSdkWrapper.newrelic_transaction_begin();
        }

        public long EndTransaction(long transactionId) {
            Guard.NotZeroOrNegativeId(transactionId, nameof(transactionId));

            return AgentSdkWrapper.newrelic_transaction_end(transactionId);
        }

        public long SetTransactionName(long transactionId, string name) {
            Guard.NotZeroOrNegativeId(transactionId, nameof(transactionId));
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            var nameSb = name.ToStringBuilder();
            return AgentSdkWrapper.newrelic_transaction_set_name(transactionId, nameSb);
        }

        public int Shutdown(string message = "")
        {
            var messageSB = message.ToStringBuilder();
            return AgentSdkWrapper.newrelic_request_shutdown(messageSB);
        }

        public long NoticeError(long transactionId, string exceptionType, string errorMessage, string stackTrace) {
            Guard.NotZeroOrNegativeId(transactionId, nameof(transactionId));
            Guard.NotNullOrWhiteSpace(exceptionType, nameof(exceptionType));
            Guard.NotNullOrWhiteSpace(errorMessage, nameof(errorMessage));

            var exceptionTypeSB = exceptionType.ToStringBuilder();
            var errorMessageSB = errorMessage.ToStringBuilder();
            var stackTraceSB = stackTrace.ToStringBuilder();
            var delimiterSB = Environment.NewLine.ToStringBuilder();

            return AgentSdkWrapper.newrelic_transaction_notice_error(transactionId, exceptionTypeSB, errorMessageSB, stackTraceSB, delimiterSB);
        }

        /// <summary>
        /// Registers a method handler for the New Relic agent
        /// </summary>
        /// <returns>True if the operation succeeds, false otherwise</returns>
        public bool RegisterMessageHandler()
        {
            const int rtldNow = 2; // for dlopen's flags 
            const string unmanagedMethodName = "newrelic_message_handler";
            var unmanagedLibPath = $"{options.LibraryPath}/libnewrelic-collector-client.so";

            var methodHandle = IntPtr.Zero;
            var libraryHandle = LibDlWrapper.dlopen(unmanagedLibPath, rtldNow);

            if (libraryHandle == IntPtr.Zero)
            {
                logger.LogError(errorEventId, $"Unable to load unmanaged New Relic module, expected to find it at {unmanagedLibPath}");
            }
            else
            {
                methodHandle = LibDlWrapper.dlsym(libraryHandle, unmanagedMethodName);
                if (methodHandle == IntPtr.Zero)
                {
                    logger.LogError(errorEventId, $"Unable to load unmanaged New Relic method {unmanagedMethodName}");
                }
            }

            // If we fail to get handles for library and method, we have been
            // unsuccessful
            if (libraryHandle == IntPtr.Zero || methodHandle == IntPtr.Zero)
            {
                return false;
            }

            AgentSdkWrapper.newrelic_register_message_handler(methodHandle);
            return true;
        }

        public bool Init(string licenseKey, string appName, string language, string languageVersion)
        {
            // Attempt to register the New Relic message handler
            var success = RegisterMessageHandler();
            if (!success)
            {
                return false;
            }
            
            var licenseKeySB = licenseKey.ToStringBuilder();
            var appNameSB = appName.ToStringBuilder();
            var languageSB = language.ToStringBuilder();
            var languageVersionSB = languageVersion.ToStringBuilder();
            try
            {
                AgentSdkWrapper.newrelic_init(licenseKeySB, appNameSB, languageSB, languageVersionSB);
            }
            catch (Exception exc)
            {
                logger.LogError(errorEventId, exc, "Could not initialize New Relic agent.");
                return false;
            }
            return true;
        }
    }
}