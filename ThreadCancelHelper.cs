using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CommonHelper
{
    public class ThreadCancelHelper
    {
        private static CancellationTokenSource cts;
        private static Object _objectLock = new Object();

        static ThreadCancelHelper()
        {
        }

        public static void BeginImport()
        {
            lock (_objectLock)
            {
                cts = new CancellationTokenSource();
            }
        }

        public static void EndImport()
        {
            lock (_objectLock)
            {
                cts.Dispose();
                cts = null;
            }
        }

        public static bool IsCancel()
        {
            return cts.IsCancellationRequested;
        }

        public static void Cancel()
        {
            if (cts == null)
            {
                return;
            }

            lock (_objectLock)
            {
                cts.Cancel();
            }
        }
    }
}