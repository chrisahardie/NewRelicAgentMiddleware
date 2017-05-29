using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NewRelicAgentMiddleware.LibWrappers
{
    public static class AgentSdkWrapper {

        [DllImport("newrelic-collector-client")]
        public static extern unsafe int newrelic_init(StringBuilder license, StringBuilder app_name, StringBuilder language, StringBuilder language_version);

        [DllImport("newrelic-collector-client")]
        public static extern unsafe int newrelic_request_shutdown(StringBuilder reason);
        
        [DllImport("newrelic-transaction")]
        public static extern unsafe void newrelic_register_message_handler(IntPtr handler);
        [DllImport("newrelic-transaction")]
        public static extern unsafe int newrelic_record_metric(StringBuilder name, double value);

        [DllImport("newrelic-transaction")]
        public static extern unsafe long newrelic_transaction_begin();

        [DllImport("newrelic-transaction")]
        public static extern unsafe long newrelic_transaction_end(long transactionId);

        [DllImport("newrelic-transaction")]
        public static extern unsafe long newrelic_transaction_set_name(long transactionId, StringBuilder transactionName);
        
        [DllImport("newrelic-transaction")]
        public static extern unsafe long newrelic_transaction_notice_error(long transaction_id, StringBuilder exception_type, StringBuilder error_message, StringBuilder stack_trace, StringBuilder stack_frame_delimiter);
    }
}