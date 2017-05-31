using Newtonsoft.Json;

namespace NewRelicAgentMiddleware.Transactions
{
    /// <summary>
    /// Maps a regex pattern to be applied to a request path to
    /// return a transaction label
    /// </summary>
    internal class PathMapping
    {
        /// <summary>
        /// A regex pattern to match against request paths
        /// e.g. valuescontroller/get/[\\d]+$
        /// </summary>
        [JsonProperty("pattern")]
        public string Pattern { get; set; }
        /// <summary>
        /// The transaction label to roll the request under, to be displayed
        /// in the New Relic portal
        /// e.g. valuescontroller/get/:id
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
