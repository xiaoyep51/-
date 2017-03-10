using CommonModel.Enum;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CommonHelper
{
    public class LogHelper
    {
        private static string LogPath;
        private static object ErrorLockingTarget = new object();
        private static string IsImportWriteLog = ConfigurationManager.AppSettings["IsImportWriteLog"];
        public delegate void DetialLog(string log);
        public static DetialLog SelfDetialLog;

        static LogHelper()
        {
            SelfDetialLog = null;
            if (System.Web.HttpContext.Current != null)
            {
                LogPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Log");
            }
            else
            {
                LogPath = AppDomain.CurrentDomain.BaseDirectory + "/Log";
            }
        }

        public static void APILog(string fileNameExtend, string format, params object[] args)
        {
            APILog(string.Format(format, args), fileNameExtend);
        }

        private static void APILog(string log, string fileNameExtend)
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                string filePath = string.Format("{0}/API_{1}_{2}.txt", LogPath, DateTime.Now.ToString("yyyy-MM-dd"), fileNameExtend);

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(string.Format("[{0}]{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log));
                }
            }
            catch
            {
                return;
            }
        }

        public static void DetailLog(string format, params object[] args)
        {
            if (SelfDetialLog != null)
            {
                SelfDetialLog(string.Format(format, args));
            }
            else
            {
                DetailLogInFile(string.Format(format, args));
            }
        }

        private static void DetailLogInFile(string log)
        {
            if (IsImportWriteLog.Equals("0"))
            {
                return;
            }

            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                string filePath = string.Format("{0}/{1}.txt", LogPath, DateTime.Now.ToString("yyyy-MM-dd"));

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(string.Format("[{0}]{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log));
                }
            }
            catch
            {
                return;
            }
        }

        public static void UpdateLastImportDate(ImportTypeEnum importType, DateTime lastImportDate)
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                string filePath = string.Format("{0}/LastImportDate.xml", LogPath);

                XDocument xDoc = new XDocument();
                var enumName = typeof(ImportTypeEnum).GetEnumName(importType);

                #region 初始化xml
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }
                try
                {
                    xDoc = XDocument.Load(filePath);
                }
                catch
                {
                    xDoc.Add(new XElement("Root"));
                }
                #endregion

                #region 确保对应节点存在
                if (!xDoc.Root.Elements().Select(i => i.Name.ToString()).Contains(enumName))
                {
                    xDoc.Root.Add(new XElement(enumName));
                    var xml = xDoc.Root.Elements().OrderBy(s => s.Name.ToString());
                    xDoc.Root.ReplaceAll(xml);
                }
                #endregion

                var node = xDoc.Root.Elements().First(i => i.Name.ToString() == enumName);
                node.SetAttributeValue("LastUpdateDate", lastImportDate.ToString("yyyy-MM-dd HH:mm:ss"));
                xDoc.Save(filePath);
            }
            catch
            {
                return;
            }
        }

        public static DateTime? GetLastImportDate(ImportTypeEnum importType)
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                string filePath = string.Format("{0}/LastImportDate.xml", LogPath);

                XDocument xDoc = new XDocument();
                var enumName = typeof(ImportTypeEnum).GetEnumName(importType);

                if (!File.Exists(filePath))
                {
                    return null;
                }
                try
                {
                    xDoc = XDocument.Load(filePath);
                }
                catch
                {
                    return null;
                }

                if (!xDoc.Root.Elements().Select(i => i.Name.ToString()).Contains(enumName))
                {
                    return null;
                }

                var node = xDoc.Root.Elements().First(i => i.Name.ToString() == enumName);
                var dateStr = node.Attribute("LastUpdateDate").Value;
                DateTime date;
                if (DateTime.TryParse(dateStr, out date))
                {
                    return date;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void WriteError(string log)
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                string filePath = string.Format("{0}/Error_{1}.txt", LogPath, DateTime.Now.ToString("yyyy-MM-dd"));

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                lock (ErrorLockingTarget)
                {
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        sw.WriteLine(string.Format("[{0}]{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log));
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}