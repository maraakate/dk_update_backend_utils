using DK_Upd_Build_WCF_Lib;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DK_Upd_Build_SVC
{
   public partial class Program
   {
      private static EventLog eventLog = null;
      private static StreamWriter swLog = null;
      private static string logFileName = null;
      internal static readonly string AppName = "DK Build";

      public static void InitFileLog()
      {
         string path, cwd;
         clsConfigReader configReader;

         configReader = new clsConfigReader("dk.config");
         cwd = configReader.GetSetting("WorkingDirectory");

         path = Path.Combine(cwd, "Logs");
         Directory.CreateDirectory(path);

         logFileName = Path.Combine(path, String.Format("DK_Build_SVC_{0}_{1}_{2}.txt", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));

         try
         {
            swLog = new StreamWriter(logFileName, true);
         }
         catch (Exception ex)
         {
            Console.Write("{0}:  Failed to open logfile {1} for writing.  Reason:  {2}\n", MethodBase.GetCurrentMethod().Name, logFileName, ex.Message);
         }
      }

      public static void InitEventLog()
      {
         if (eventLog != null)
         {
            eventLog.Close();
            eventLog.Dispose();
         }

         eventLog = new EventLog();
         if (!EventLog.SourceExists("Application"))
         {
            EventLog.CreateEventSource(AppName, "Application");
         }
         eventLog.Source = AppName;
         eventLog.Log = "Application";
      }

      public static void WriteMessage(string msg)
      {
         try
         {
            Console.Write(msg);

            if (swLog != null)
            {
               swLog.Write(msg);
               swLog.Flush();
            }

            if (eventLog != null)
            {
               eventLog.WriteEntry(msg);
            }
         }
         catch
         {
            /* FS: Don't care. */
         }
      }

      public static void WriteMessage(string msg, params object[] args)
      {
         string tmp;

         tmp = String.Format(msg, args);

         try
         {
            Console.Write(tmp);

            if (swLog != null)
            {
               swLog.Write(tmp);
               swLog.Flush();
            }

            if (eventLog != null)
            {
               eventLog.WriteEntry(tmp);
            }
         }
         catch
         {
            /* FS: Don't care. */
         }
      }
   }
}
