using System;
using System.Collections.Generic;
using System.Text;

namespace KSPTraceNet20.queue
{
    public class TraceData
    {

        public string UserNo { get; set; }
        public string UserCompany { get; set; }
        public string UserDeptName { get; set; }
        public string UserName { get; set; }
        public string UserAgent { get; set; }
        public string ContentType { get; set; }
        public string RequestUrl { get; set; }

        public string RequestData { get; set; }

        public string HttpMethod { get; set; }
        public string AppId { get; set; }

        public string FileNames { get; set; }
        public DateTime RequestDate { get; set; }
        public override string ToString()
        {
            return string.Format("AppId:{0},UserName:{1},Url:{2},ContentType:{3},Url:{4},FileNames:{5},RequestData:{6},UserAgent:{7},HttpMethod:{8}", AppId, UserName, RequestUrl, ContentType, RequestUrl, FileNames, RequestData, UserAgent, HttpMethod);
        }
    }
}
