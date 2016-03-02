using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MonitoringFile
{
    public partial class MonitoringService : ServiceBase
    {
        private readonly static int timelag = 1000 * 60 * 60;//1小时
        private static readonly string timelagStr = ConfigurationManager.ConnectionStrings["timelag"].ConnectionString;
        public static string minSize = ConfigurationManager.ConnectionStrings["minSize"].ConnectionString;
        public static string maxSize = ConfigurationManager.ConnectionStrings["maxSize"].ConnectionString;
        public static string errorPath = ConfigurationManager.ConnectionStrings["errorPath"].ConnectionString;
        public static string imagePath = ConfigurationManager.ConnectionStrings["imagePath"].ConnectionString;
        private Timer time;
        public MonitoringService()
        {
            InitializeComponent();
            int timelast = 0;
            if (int.TryParse(timelagStr, out timelast))
            {
                if (timelast > 1)
                {
                    time = new Timer(timelast * 60 * 60);
                }
                else
                {
                    time = new Timer(timelag);
                    ErrorCollectHelper.ErrorLog("配置文件错误！timelag配置不符合条件，采取默认1小时内置配置");
                }
            }
            else
            {
                time = new Timer(timelag);
                ErrorCollectHelper.ErrorLog("配置文件错误！timelag配置不符合条件，采取默认1小时内置配置");
            }
        }

        protected override void OnStart(string[] args)
        {
            StartSomething();
        }

        private void StartSomething()
        {
            time.AutoReset = true;
            time.Enabled = false;
            time.Elapsed += Time_Elapsed;
            time.Start();
        }

        private void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //这里是监测操作
                new ImgsDetectionHelper().Monitor();
            }
            catch (Exception ex)
            {
                ErrorCollectHelper.ErrorLog(ex.ToString());
            }

        }

        protected override void OnStop()
        {

        }
    }
}
