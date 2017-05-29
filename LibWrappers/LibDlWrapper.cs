using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NewRelicAgentMiddleware.LibWrappers
{
    internal static class LibDlWrapper
    {
        [DllImport("dl")]
        public static extern IntPtr dlopen(string filename, int flags);

        [DllImport("dl")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
    }
}
