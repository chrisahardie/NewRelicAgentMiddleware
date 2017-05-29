using System;
using System.Collections.Generic;

namespace NewRelicAgentMiddleware.Transactions
{
    internal interface IMappingsRepository
    {
        IEnumerable<ActionMapping> Get();
        void RegisterCallbackOnUpdate(Action<IEnumerable<ActionMapping>> callback);
    }
}