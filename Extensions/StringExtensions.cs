using System;
using System.Collections.Generic;
using System.Text;

namespace NewRelicAgentMiddleware.Extensions
{
    public static class StringExtensions
    {
        public static StringBuilder ToStringBuilder(this string text)
        {
            return new StringBuilder(text);
        }
    }
}
