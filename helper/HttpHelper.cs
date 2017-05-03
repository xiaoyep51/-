using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RCApp_Win.Logic.Utility
{
    public class HttpHelper
    {
        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        public static HttpWebRequest CreateGetHttpRequest(string url, CookieCollection cookies = null, int? timeout = null, string userAgent = "")
        {
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "GET";
            request.Proxy = null;
            request.KeepAlive = false;

            //设置代理UserAgent和超时
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request;
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        public static HttpWebRequest CreatePostHttpRequest(string url, IDictionary<string, string> parameters, CookieCollection cookies = null, int? timeout = null, string userAgent = "")
        {
            HttpWebRequest request = null;
            //如果是发送HTTPS请求
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer(); //记录cookie
            //request.Proxy = null;
            request.KeepAlive = false;

            //设置代理UserAgent和超时
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //发送POST数据  
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        i++;
                    }
                }
                byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            else
            {
                request.ContentLength = 0;
            }
            return request;
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        public static HttpWebRequest CreatePostHttpRequest(string url, string jsonStr, CookieCollection cookies = null, int? timeout = null, string userAgent = "")
        {
            HttpWebRequest request = null;
            //如果是发送HTTPS请求
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/json";
            request.CookieContainer = new CookieContainer(); //记录cookie
            //request.Proxy = null;
            request.KeepAlive = false;

            //设置代理UserAgent和超时
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //发送POST数据
            if (!string.IsNullOrWhiteSpace(jsonStr))
            {
                byte[] data = Encoding.UTF8.GetBytes(jsonStr);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            else
            {
                request.ContentLength = 0;
            }
            return request;
        }

        /// <summary>
        /// 验证证书
        /// </summary>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }

        public static HttpWebResponse GetHttpResponse(HttpWebRequest request)
        {
            var response = request.GetResponse() as HttpWebResponse;
            return response;
        }

        /// <summary>
        /// 获取请求的数据
        /// </summary>
        public static string GetResponseString(HttpWebResponse webresponse)
        {
            using (webresponse)
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        public static string CreateUploadFileRequest(string url, List<string> filePathList, CookieCollection cookies = null)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer.Add(cookies);
            using (var httpClient = new HttpClient(handler))
            using (var content = new MultipartFormDataContent())
            {
                foreach (var filePath in filePathList)
                {
                    string fileName = System.IO.Path.GetFileName(filePath);

                    Stream imageStream = new FileStream(filePath, FileMode.Open);
                    var streamContent = new StreamContent(imageStream);
                    streamContent.Headers.Add("Content-Type", "application/octet-stream");
                    fileName = System.Web.HttpUtility.UrlEncode(fileName, Encoding.UTF8);
                    streamContent.Headers.Add("Content-Disposition", "form-data; name=\"Filedata\"; filename=\"" + fileName + "\"");
                    content.Add(streamContent);
                }

                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode) //OK
                {
                    String jsonString = response.Content.ReadAsStringAsync().Result;
                    return jsonString;
                }
            }
            return "";
        }

        public static string UploadFileByWebClient(string url, string filePath, CookieCollection cookies)
        {
            WebClient wc = new WebClient();
            wc.Credentials = CredentialCache.DefaultCredentials;
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            wc.Headers.Add("Cookie", "Signing=" + cookies["Signing"].Value);
            //wc.QueryString["fname"] = openFileDialog1.SafeFileName;

            //byte[] fileb = wc.UploadFile(new Uri(url), "POST", filePath);
            byte[] fileb = wc.UploadFile(url, "POST", filePath);
            //string res = Encoding.GetEncoding("gb2312").GetString(fileb);
            string res = Encoding.GetEncoding("UTF-8").GetString(fileb);
            return res;
        }

        public static bool DownloadFile(string url, string localPath)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] recorde = response.Content.ReadAsByteArrayAsync().Result;
                        //将文件流写入文件
                        using (FileStream write = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, System.IO.FileShare.Read))
                        {
                            write.Write(recorde, 0, recorde.Count());
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
