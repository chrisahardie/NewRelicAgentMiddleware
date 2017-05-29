using System;

namespace NewRelicAgentMiddleware
{
    public static class Guard
    {
        public static void NotNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void NotNullOrWhiteSpace(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException($"{argumentName} is null or whitespace");
            }
        }
        public static void NotZeroOrNegativeId(long argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentException($"{argumentName} is an invalid Id. Should be greater than zero.");
            }
        }
    }    
}