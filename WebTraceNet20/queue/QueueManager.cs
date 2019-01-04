using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace KSPTraceNet20.queue
{
    public class QueueManager
    {
        private static QueueManager instance = null;
        log4net.ILog log = log4net.LogManager.GetLogger("KSPTrace.Logging");
        Queue<TraceData> taskQueue = null;
        public static QueueManager getInstance()
        {
           
            if (instance == null) instance = new QueueManager();
            return instance;
        }
        private QueueManager()
        {
            /*
            try
            {
                init();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            */
        }
        public void emptyQueue()
        {
            taskQueue = new Queue<TraceData>();
        }
        private void init()
        {
            taskQueue = new Queue<TraceData>();

            Thread t = new Thread(() =>
              {
                  while (true)
                  {
                      if(Monitor.TryEnter (taskQueue))
                      {
                          if (taskQueue.Count > 0)
                          {
                              TraceData td = taskQueue.Dequeue();
                              Monitor.Exit(taskQueue);
                              sendTraceData(td);
                          }
                          
                          
                      }

                  }
              });

            t.Start();
        }

        private   void sendTraceData(TraceData td)
        {
            if (td == null) return;

            try
            {
                string webServerUrl = ConfigurationManager.AppSettings["KSPTrace_WebServerUrl"];
                if (string.IsNullOrEmpty(webServerUrl)) return;

                 
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "ksptracenet20");
               // client.Headers.Add("Content-Type", "application/json;charset=UTF-8");
                client.Headers.Add("Content-Type", "application/json");
                
                 string json= JsonConvert.SerializeObject(td);

                  byte[] data=System.Text.Encoding.GetEncoding("utf-8").GetBytes(json);
                 
                //client.Headers.Add("Content-Length", json.Length.ToString());

                client.UploadDataCompleted += Client_UploadDataCompleted;

                client.UploadDataAsync(new Uri(webServerUrl), "post", data);
              
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }



        }

        private void Client_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            if(e!=null && e.Error != null)
            {
                log.Error("发送数据到服务器出错1");
                log.Error(e.Error);
            }
        }
 
     
        public void pushQueue(TraceData td)
        {
            if (td == null) return;
            new Thread(() =>
            {
                sendTraceData(td);
            }).Start();

            
            /*
           if( Monitor.TryEnter(taskQueue))
            {
                
                taskQueue.Enqueue(td);
                Monitor.Exit(taskQueue);
                
            }*/

        }
    }
}
