using DK_Upd_Build_SVC.ServiceReference1;
using System;
using System.Reflection;
using System.ServiceProcess;

namespace DK_Upd_Build_SVC
{
   public partial class Program
   {
      public static void Main(string[] args)
      {
         Service1 service = new Service1();

         // In interactive and debug mode ?
         if (Environment.UserInteractive /*&& System.Diagnostics.Debugger.IsAttached*/)
         {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            // Simulate the services execution
            service.RunAsConsole(args);
         }
         else
         {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { service };
            ServiceBase.Run(ServicesToRun);
         }
      }
   }
}
