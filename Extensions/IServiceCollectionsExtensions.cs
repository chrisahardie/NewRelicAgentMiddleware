using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NewRelicAgentMiddleware.Agent;
using NewRelicAgentMiddleware.Configuration;

namespace NewRelicAgentMiddleware.Extensions
{
    public static class ServiceCollectionsExtensions
    {
        public static void AddNewRelicServices(this IServiceCollection services, IConfigurationSection config)
        {
            services.Configure<NewRelicOptions>(config);
            services.AddTransient<IAgentSdk, AgentSdk>();
        }

        public static void AddNewRelicServices(this IServiceCollection services, Action<NewRelicOptions> options)
        {
            services.Configure(options);
            services.AddTransient<IAgentSdk, AgentSdk>();
        }
    }
}
