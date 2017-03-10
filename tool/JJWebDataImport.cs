using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using System.Net.Http;
using System.Web.Script.Serialization;
using WindowsService.Model;
using System.Xml.Linq;
using WindowsService.Tools;

namespace WindowsService
{
    public partial class JJWebDataImport : ServiceBase
    {
        IScheduler scheduler;

        public JJWebDataImport()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogHelper.DetailLog("Windows服务开启");

            ISchedulerFactory factory = new StdSchedulerFactory();
            scheduler = factory.GetScheduler();
            var cronExpression = "";

            //导入所有数据
            cronExpression = System.Configuration.ConfigurationManager.AppSettings["AllImportCronExpression"];
            if (cronExpression != null)
            {
                IJobDetail dataImportJob = JobBuilder.Create<AllImport>().WithIdentity("AllImportJob", "MainGroup").Build();
                ITrigger dataImportTrigger = TriggerBuilder.Create()
                                    .WithIdentity("AllImportTrigger", "MainGroup")
                                    .WithCronSchedule(cronExpression)
                                    .StartNow().Build();
                scheduler.ScheduleJob(dataImportJob, dataImportTrigger);
            }

            //根据楼盘导入数据
            cronExpression = System.Configuration.ConfigurationManager.AppSettings["BuildingImportCronExpression"];
            if (cronExpression != null)
            {
                IJobDetail keepAliveJob = JobBuilder.Create<PartImport>().WithIdentity("BuildingImportJob", "MainGroup").Build();
                ITrigger keepAliveTrigger = TriggerBuilder.Create()
                                    .WithIdentity("BuildingImportTrigger", "MainGroup")
                                    .WithCronSchedule(cronExpression)
                                    .StartNow().Build();
                scheduler.ScheduleJob(keepAliveJob, keepAliveTrigger);
            }

            scheduler.Start();
        }

        protected override void OnStop()
        {
            LogHelper.DetailLog("Windows服务关闭");
            scheduler.Shutdown();
        }
    }

    public class AllImport : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            LogHelper.DetailLog("导入程序执行");
            string apiUrl = System.Configuration.ConfigurationManager.AppSettings["AllImportUrl"];

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                LogHelper.DetailLog("导入程序执行失败，API接口为空");
                return;
            }

            using (var httpClient = new HttpClient())
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                HttpResponseMessage response = httpClient.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var apiResponse = serializer.Deserialize<Response>(result);

                if (apiResponse.StatusCode != 0)
                {
                    LogHelper.WriteError("请求失败！");
                    return;
                }
            }
        }
    }

    public class PartImport : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            LogHelper.DetailLog("楼盘导入执行");
            string apiUrl = System.Configuration.ConfigurationManager.AppSettings["BuildingImportUrl"];

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                LogHelper.DetailLog("楼盘导入执行失败，API接口为空");
                return;
            }

            using (var httpClient = new HttpClient())
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                HttpResponseMessage response = httpClient.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var apiResponse = serializer.Deserialize<Response>(result);

                if (apiResponse.StatusCode != 0)
                {
                    LogHelper.WriteError("请求失败！");
                    return;
                }
            }
        }
    }
}
