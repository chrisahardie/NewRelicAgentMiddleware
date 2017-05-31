using Microsoft.AspNetCore.Builder;
using NewRelicAgentMiddleware.Middleware;

namespace NewRelicAgentMiddleware.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseNewRelicAgent(this IApplicationBuilder builder, bool IsEnabled = true)
        {
            return builder.UseMiddleware<AgentMiddleware>(IsEnabled);
        }
    } 
}