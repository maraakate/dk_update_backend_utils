using DK_Upd_Build_SVC.ServiceReference1;
using System;
using System.ServiceProcess;
using System.Threading;

namespace DK_Upd_Build_SVC
{
   public partial class Service1 : ServiceBase
   {
      public static Service1Client clientService = null;

      public Service1()
      {
         InitializeComponent();
      }

      protected override void OnStart(string[] args)
      {
         InitSVCConnection();

         Console.Write("{0}: {1}\n", DateTime.Now, clientService.Ping());
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
         TimeSpan timeout = new TimeSpan(0, 0, 20); /* FS: FIXME: This doesn't actually work... */

      retry:
         try
         {
            if (clientService != null)
            {
               clientService.Abort();
            }

            clientService = new Service1Client();
            clientService.InnerChannel.Open(timeout);
            Program.WriteMessage("Connected to service.\n");
         }
         catch (Exception Ex)
         {
            Program.WriteMessage("Unable to connect to the Daikatana Build Service.  Reason: {0}", Ex.Message);
            goto retry;
         }
      }

      private static void CloseSVCConnection()
      {
         if (clientService == null)
            return;

         clientService.Close();
         clientService = null;
      }
   }
}
