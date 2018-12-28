using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSPTrace.queue
{
    public class TraceData
    {
        public string UserName { get; set; }
        public string UserAgent { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }

        public string RequestData { get; set; }

        public string HttpMethod { get; set; }
        public string AppId { get; set; }

        public string FileNames { get; set; }
        
        public override string ToString()
        {
            return string.Format("AppId:{0},UserName:{1},Url:{2},ContentType:{3},Url:{4},FileNames:{5},RequestData:{6},UserAgent:{7},HttpMethod:{8}", AppId, UserName, Url, ContentType, Url, FileNames, RequestData,UserAgent,HttpMethod);
        }
    }
}
