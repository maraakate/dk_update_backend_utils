/* FS: Re-adapted from https://stackoverflow.com/questions/177856/how-do-i-trap-ctrl-c-sigint-in-a-c-sharp-console-app/22996552#22996552 */
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DK_Upd_Build_SVC
{
   static partial class Program
   {
      enum CtrlType
      {
         CTRL_C_EVENT = 0,
         CTRL_BREAK_EVENT = 1,
         CTRL_CLOSE_EVENT = 2,
         CTRL_LOGOFF_EVENT = 5,
         CTRL_SHUTDOWN_EVENT = 6
      }

      [DllImport("Kernel32")] private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
      private delegate bool EventHandler(CtrlType sig);
      static EventHandler _handler;
      public static volatile bool bExitSystem = false;

      private static bool Handler(CtrlType sig)
      {
         if (sig != CtrlType.CTRL_C_EVENT)
         {
            return false;
         }

         Console.Write("CTRL+C triggered\n");

         //do your cleanup here
         /* FS: OnStop() will trigger when we bust out of while loop. */
         Thread.Sleep(1000); //simulate some cleanup delay

         //allow main to run off
         bExitSystem = true;

         //shutdown right away so there are no lingering threads
         Environment.Exit(-1);

         return true;
      }
   }
}
