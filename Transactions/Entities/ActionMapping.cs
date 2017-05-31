using Newtonsoft.Json;
using System.Collections.Generic;

namespace NewRelicAgentMiddleware.Transactions
{
    /// <summary>
    /// Top level JSON document entity
    /// </summary>
    internal class ActionMapping
    {
        /// <summary>
        /// The basic "route" to an action, should look like "controller/action"
        /// This object maps an action to one or more path mappings.
        /// </summary>
        [JsonProperty("actionRoute")]
        public string ActionRoute { get; set; }
        /// <summary>
        /// Each action can possibly be overloaded, making it impossible
        /// to distinguish between overloads in New Relic
        /// e.g.
        /// Get()
        /// Get(int id) 
        /// This collection represents all the different mappings of request paths
        /// to overloaded actions described in the JSON document.
        /// </summary>
        [JsonProperty("pathMappings")]
        public List<PathMapping> PathMappings { get; set; }
    }
}
