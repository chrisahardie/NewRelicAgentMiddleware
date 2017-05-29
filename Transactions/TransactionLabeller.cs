using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NewRelicAgentMiddleware.Transactions
{
    /// <summary>
    /// When we publish data to New Relic, we want to roll up transactions that are consuming the same endpoint. This cannot be done
    /// automatically given dynamic parameters in URL segments - e.g.
    /// 
    /// /controller/action/1
    /// /controller/action/2
    /// 
    /// Instead of the two above requests having different labels, we want them to be rolled up under a single label like:
    /// 
    /// /controller/action/:id
    /// 
    /// However, we can't simply use the controller and action to generate the label as we are not assured that the combination will be unique.
    /// An action can be overloaded like:
    /// 
    /// Get()
    /// Get(int id)
    /// 
    /// In middleware, we do not have access to the controllercontext or actioncontext, so we can't retrieve enough detail to create a proper label 
    /// ref: (https://github.com/aspnet/Mvc/issues/3826)
    /// 
    /// As such, we will need to maintain a mapping JSON document where, if an action is overloaded, we will match the request path against
    /// a list of regex patterns to determine the correct label to apply.
    /// 
    /// This JSON document should be placed at /Mappings/mappings.json in the consuming application.
    /// </summary>
    internal class TransactionLabeller<T> where T : IMappingsRepository, new()
    {
        private static readonly Lazy<TransactionLabeller<T>> instance = new Lazy<TransactionLabeller<T>>(() => new TransactionLabeller<T>(new T()));
        private IEnumerable<ActionMapping> mappings;
        private IMappingsRepository mappingsRepo;
        
        public static TransactionLabeller<T> Instance => instance.Value;

        internal TransactionLabeller(T mappingsRepo)
        {
            this.mappingsRepo = mappingsRepo;
            mappings = this.mappingsRepo.Get();
            this.mappingsRepo.RegisterCallbackOnUpdate(mappings => this.mappings = mappings);
        }
        
        public string GetTransactionLabel(string controller, string action, string requestPath)
        {
            // We will default the transaction label to the request path. If 
            // an appropriate mapping in the consuming application's mapping.json
            // file is found, the label found therein will take precedence
            var label = requestPath;
            
            var actionRouteName = $"{controller}/{action}";

            var actionRoute = mappings.FirstOrDefault(mapping => mapping.ActionRoute.Equals(actionRouteName, StringComparison.OrdinalIgnoreCase));

            if (actionRoute == null)
            {
                return label;
            }

            foreach (var pathMapping in actionRoute.PathMappings)
            {
                if (Regex.IsMatch(requestPath, pathMapping.Pattern, RegexOptions.IgnoreCase))
                {
                    label = pathMapping.Label;
                    break;
                }
            }

            return label;
        }
    }
}
