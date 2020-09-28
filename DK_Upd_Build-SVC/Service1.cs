using System;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using DK_Upd_Build_WCF_Lib;

namespace DK_Upd_Build_SVC
{
   public partial class Service1 : ServiceBase
   {
      internal static ServiceHost myServiceHost = null;

      public Service1()
      {
         InitializeComponent();
      }

      protected override void OnStart(string[] args)
      {
         InitSVCConnection();
      }

      protected override void OnStop()
      {
         CloseSVCConnection();
      }

      public void RunAsConsole(string[] args)
      {
         OnStart(args);

         while (!Program.bExitSystem)
         {
            Thread.Sleep(250); /* FS: This is OK.  Timer will still fire and do what it needs to do.  Need this for CTRL+C handling. */
         }

         OnStop();
      }


      private static void InitSVCConnection()
      {
         if (myServiceHost != null)
            myServiceHost.Close();

         myServiceHost = new ServiceHost(typeof(DK_Upd_Build_WCF_Lib.Service1));
         myServiceHost.Open();
      }

      private static void CloseSVCConnection()
      {
         if (myServiceHost == null)
            return;

         myServiceHost.Close();
         myServiceHost = null;
      }
   }
}
