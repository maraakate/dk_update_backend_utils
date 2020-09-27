using System;
using System.IO;

namespace DK_Upd_Push_To_SQL
{
   public partial class Program
   {
      public static string LogFile = String.Format("Logs\\DK_Build_Log_{0}_{1}_{2}.txt", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

      static void Log (string msg)
      {
         string timeStampMsg;

         Console.Write(msg);

         timeStampMsg = String.Format("{0}: {1}", DateTime.Now, msg);

         try
         {
            Directory.CreateDirectory("Logs");

            using (StreamWriter sw = new StreamWriter(LogFile, true))
            {
               sw.Write(timeStampMsg);
               sw.Flush();
            }
         }
         catch(Exception ex)
         {
            Console.Write("Failed to write to log {0}.  Reason:  {1}\n", LogFile, ex.Message);
         }
      }

      static void Log (string msg, params object[] args)
      {
         string temp;

         temp = string.Format(msg, args);
         Log(temp);
      }
   }
}
