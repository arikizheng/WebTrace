using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KSPTrace.queue
{
    public class QueueManager
    {
        private static QueueManager instance = null;
        log4net.ILog log = log4net.LogManager.GetLogger("KSPTrace.Logging");
        private ConcurrentQueue<TraceData> taskQueue = null;

        SemaphoreSlim semaphore = null;
        int taskCount = 5;
        public static QueueManager getInstance()
        {
            if (instance == null) instance = new QueueManager();
            return instance;
        }
        private QueueManager()
        {
            try
            {
                init();
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }
        private void init()
        {
            semaphore=new SemaphoreSlim(0, int.MaxValue);
            taskQueue=new ConcurrentQueue<TraceData>();
          
                string tc = ConfigurationManager.AppSettings["KSPTrace_TaskCount"];
                taskCount = string.IsNullOrEmpty(tc) ? taskCount : int.Parse(tc);
            
           // Task[] tasks = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                  Task.Run(() => {

                      while (true)
                      {
                          log.DebugFormat("Task {0} waiting", Task.CurrentId);

                          semaphore.Wait();
                          TraceData td;
                          if (taskQueue.TryDequeue(out td))
                          {
                              if (td != null)
                              {
                                  log.DebugFormat("Task {0} enter", Task.CurrentId);
                                  Console.WriteLine(td.AppId);
                                  sendTraceData(td);
                                  //log.DebugFormat("td.AppId {0} ", td.ToString());
                                  semaphore.Release();
                              }
                          }
                      }
                     
                });
            }
        }
        private async void sendTraceData(TraceData td)
        {
            if (td == null) return;
             
            try
            {
                string webServerUrl = ConfigurationManager.AppSettings["KSPTrace_WebServerUrl"];
                if (string.IsNullOrEmpty(webServerUrl)) return;

                HttpClient hc = new HttpClient();
               
                HttpContent jsonContent = new StringContent(JsonConvert.SerializeObject(td));

                jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage retMessage = await hc.PostAsync(webServerUrl, jsonContent);

                log.Debug(retMessage);


            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            


        }
        public  void pushQueue(TraceData td)
        {
            if (td == null) return;
            taskQueue.Enqueue(td);
            semaphore.Release(1);
        }


    }
}
