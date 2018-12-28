using KSPTrace.queue;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KSPTrace
{
    public class TraceHttpModule : IHttpModule
    {
        log4net.ILog log = log4net.LogManager.GetLogger("KSPTrace.Logging");
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += Context_EndRequest;
        }
        private void Context_EndRequest(object sender, EventArgs e)
        {
            //System.Collections.Concurrent.ConcurrentQueue<>
           
            try
            {
                string KSPTrace_Enabled = ConfigurationManager.AppSettings["KSPTrace_Enabled"];
                if (!string.IsNullOrEmpty(KSPTrace_Enabled) && KSPTrace_Enabled.ToUpper().Equals("TRUE"))
                {
                    KSPTrace((HttpApplication)sender);
                }
            
            }catch(Exception ex)
            {
                log.Error(ex);
            }

        }

        private bool IsIgnorUrl(string url)
        {
            bool blRet = false;
            if (string.IsNullOrEmpty(url)) return true;
            
            string IgnoreUrlSuffix = ConfigurationManager.AppSettings["KSPTrace_IgnoreUrlSuffix"];
            string IgnoreUrlPrefix = ConfigurationManager.AppSettings["KSPTrace_IgnoreUrlPrefix"];
            

            if (!string.IsNullOrEmpty(IgnoreUrlSuffix))
            {
                string[] suffixWords=IgnoreUrlSuffix.Split(',');

                foreach(string word in suffixWords)
                {
                    if(url.ToLower().EndsWith(word))
                    {
                        blRet = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(IgnoreUrlPrefix))
            {
                string[] prefixWords = IgnoreUrlPrefix.Split(',');
                
                foreach (string word in prefixWords)
                {
                    if (url.ToLower().StartsWith(word))
                    {
                        blRet = true;
                        break;
                    }
                }
            }


            return blRet;
        }
        private   void KSPTrace(HttpApplication ha)
        {
            string KSPTrace_AppId = ConfigurationManager.AppSettings["KSPTrace_AppId"];
            if (string.IsNullOrEmpty(KSPTrace_AppId)) return;

            HttpRequest request = ha.Request;
            string url = request.Url.ToString();
            if (IsIgnorUrl(url)) return;

          
            //HttpApplication ha = (HttpApplication)sender;
       
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            string username = "guest";
            if (windowsIdentity != null && windowsIdentity.Name != null)
            {
                username = windowsIdentity.Name;
            }
            string contentType = request.ContentType;
            string agent = request.UserAgent;
             string httpMethod = request.HttpMethod;
            //  string queryUrl = request.QueryString.ToString();

            string fileNames = "";
            string formData = "";
            if (contentType != null && contentType.ToLower().StartsWith("multipart/form-data"))
            {
                formData = GetFormData(request.Form);
                fileNames = GetFileNames(request);
                //上传文件 "multipart/form-data; boundary=----WebKitFormBoundarykswooCU7y89ccnOv"

            }
            else
            {
                formData = GetFormData(request.Form);
                if (string.IsNullOrEmpty(formData) && request.TotalBytes > 0)
                {
                    // application/x-www-form-urlencoded;charset=utf-8
                    byte[] data = request.BinaryRead(request.TotalBytes);
                    formData = System.Text.Encoding.UTF8.GetString(data);
                }
            }

            TraceData td = new TraceData {ContentType=contentType,UserAgent=agent,Url=url, RequestData=formData , UserName = username,AppId= KSPTrace_AppId,HttpMethod=httpMethod , FileNames= fileNames };
            QueueManager.getInstance().pushQueue(td);
        }

        private   string GetFileNames(HttpRequest request)
        {
             string fileNames = "";
            try
            {
                if (request.Files != null)
                {
                    StringBuilder buffer = new StringBuilder();

                    foreach (string key in request.Files.AllKeys)
                    {
                        buffer.AppendFormat("{0},", request.Files[key].FileName);
                    }

                    fileNames = buffer.ToString();
                }
            }catch(Exception ex)
            {
                log.Error(ex);
            }

            return fileNames;
        }

        private string GetFormData(NameValueCollection forms)
        {
            if (forms == null) return "";

            StringBuilder buffer = new StringBuilder();
            foreach (string key in forms.Keys)
            {
                string str = string.IsNullOrEmpty(buffer.ToString()) ? "" : ",";
                buffer.AppendFormat("{0}{1}:\"{2}\"",str, key, forms[key]);
            }

            return string.IsNullOrEmpty(buffer.ToString()) ? "" : "{" + buffer.ToString() + "}";

        }
    }
}
