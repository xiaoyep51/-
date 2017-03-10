using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CommonHelper
{
    public class AccessHelper
    {
        private static int DataImportCount;
        private static object LockingTarget;
        private static Object _objectLock = new Object();

        static AccessHelper()
        {
            DataImportCount = 0;
            LockingTarget = new object();
        }

        public static bool Access()
        {
            if (DataImportCount > 0)
            {
                return false;
            }
            else
            {
                ThreadCancelHelper.BeginImport();
                lock (_objectLock)
                {
                    DataImportCount++;
                }
                return true;
            }
        }

        public static bool CanAccess()
        {
            if (DataImportCount > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void Leave()
        {
            ThreadCancelHelper.EndImport();
            lock (_objectLock)
            {
                DataImportCount = 0;
            }
        }
    }
}