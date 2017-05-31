using System.Text;

namespace NewRelicAgentMiddleware.Agent
{
    public interface IAgentSdk {
        int RecordMetric(string name, double value);

        long BeginTransaction();

        long EndTransaction(long transactionId);

        long SetTransactionName(long transactionId, string name);

        long NoticeError(long transactionId, string exceptionType, string errorMessage, string stackTrace);
        bool Init(string licenseKey, string appName, string language, string languageVersion);
        int Shutdown(string message);
    }
}