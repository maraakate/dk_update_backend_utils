using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace DK_Upd_Push_To_SQL
{
   public partial class Program
   {
      private static string SQLConnStr = string.Empty;
      private static string dkExePath = string.Empty;
      private static readonly string cfgFile = "DK_Upd_Push_To_SQL.exe.config";
      public static string filenamewithpath, pdbfilenamewithpath, arch;
      public static int beta;
      public static DateTime date = DateTime.Now;
      public static string md5Hash = string.Empty;
      public static clsConfigReader cfgReader;

      /* FS: Email args */
      private static MailAddress fromAddress;
      private static string toAddress, emailHost, emailUser, emailPass;

      static void PushToSQL ()
      {
         StringBuilder Query;
         string binFileName, pdbFileName, msg;
         clsSQL dbSQL;
         Collection<SqlParameter> Parameters;
         Guid guid;

         guid = Guid.NewGuid();
         msg = string.Empty;

         if (String.IsNullOrWhiteSpace(filenamewithpath))
         {
            Log("{0}: filenamewithpath is null.\n", MethodBase.GetCurrentMethod().Name);
            System.Environment.Exit((int)ErrorCodes.NullFileNamePath);
            return;
         }

         if (String.IsNullOrWhiteSpace(pdbfilenamewithpath))
         {
            Log("{0}: pdbfilenamewithpath is null.\n", MethodBase.GetCurrentMethod().Name);
            System.Environment.Exit((int)ErrorCodes.NullPDBFilePath);
            return;
         }

         if (String.IsNullOrWhiteSpace(arch))
         {
            Log("{0}: arch is null.\n", MethodBase.GetCurrentMethod().Name);
            System.Environment.Exit((int)ErrorCodes.NullArch);
            return;
         }

         Query = new StringBuilder(4096);
         dbSQL = new clsSQL(SQLConnStr);
         Parameters = new Collection<SqlParameter>();

         binFileName = Path.GetFileName(filenamewithpath);
         pdbFileName = Path.GetFileName(pdbfilenamewithpath);

         Query.AppendLine("INSERT INTO tblBuilds (id, date, arch, filename, changes)");
         Query.AppendLine("VALUES (@guid, @date, @arch, @binFileName, '')");

         Query.AppendLine("INSERT INTO tblDBSymbols (id, filename)");
         Query.AppendLine("VALUES (@guid, @pdbFileName)");

         Query.AppendLine("INSERT INTO tblBuildsBinary(id, data, md5)");
         Query.AppendLine(String.Format("VALUES (@guid, (SELECT * FROM OPENROWSET(BULK N'{0}', SINGLE_BLOB) AS Executable), @md5)", filenamewithpath));

         Query.AppendLine("insert into tblDBSymbolsBinary(id, data)");
         Query.AppendLine(String.Format("VALUES (@guid, (SELECT * FROM OPENROWSET(BULK N'{0}', SINGLE_BLOB) AS Executable))", pdbfilenamewithpath));

         Query.AppendLine("UPDATE tblLatest");
         Query.AppendLine("SET id  = @guid");
         Query.AppendLine("WHERE arch = @arch AND beta = @beta");

         Parameters.Add(clsSQL.BuildSqlParameter("@arch", System.Data.SqlDbType.NVarChar, arch));
         Parameters.Add(clsSQL.BuildSqlParameter("@date", System.Data.SqlDbType.Date, date));
         Parameters.Add(clsSQL.BuildSqlParameter("@beta", System.Data.SqlDbType.Bit, beta));
         Parameters.Add(clsSQL.BuildSqlParameter("@pdbFileName", System.Data.SqlDbType.NVarChar, pdbFileName));
         Parameters.Add(clsSQL.BuildSqlParameter("@binFileName", System.Data.SqlDbType.NVarChar, binFileName));
         Parameters.Add(clsSQL.BuildSqlParameter("@guid", System.Data.SqlDbType.UniqueIdentifier, guid));
         Parameters.Add(clsSQL.BuildSqlParameter("@md5", System.Data.SqlDbType.NVarChar, md5Hash));

         try
         {
            if (!dbSQL.Query(Query.ToString(), Parameters.ToArray()))
            {
               msg = String.Format("{0}: Failed Query: {1}\n", MethodBase.GetCurrentMethod().Name, dbSQL.LastErrorMessage);
               Log(msg);
               SendEmail(String.Format("Daikatana Update - ERROR {0}", (int)ErrorCodes.ErrorSQLQuery), msg);
               System.Environment.Exit((int)ErrorCodes.ErrorSQLQuery);
               return;
            }

            msg = String.Format("Success!  ID: {0}\n", guid.ToString());
            Log(msg);
            SendEmail("Daikatana Update - SUCCESS", msg);
         }
         catch (Exception ex)
         {
            msg = String.Format("{0}: Failed: {1}\n", MethodBase.GetCurrentMethod().Name, ex.Message);
            Log(msg);
            SendEmail(String.Format("Daikatana Update - FAILED {0}", (int)ErrorCodes.ErrorSQLQuery), msg);
            System.Environment.Exit((int)ErrorCodes.ErrorSQLQuery);
         }
      }

      static void ParseArgs (string[] args)
      {
         for (int i = 0; i < args.Length; i++)
         {
            if (args[i].Equals("-filename", StringComparison.OrdinalIgnoreCase))
            {
               if (i < args.Length - 1 && args[i + 1].Length > 0)
               {
                  filenamewithpath = args[i + 1];
                  Console.Write("Filename: {0}\n", filenamewithpath);
               }
            }
            else if (args[i].Equals("-pdbfilename", StringComparison.OrdinalIgnoreCase))
            {
               if (i < args.Length - 1 && args[i + 1].Length > 0)
               {
                  pdbfilenamewithpath = args[i + 1];
                  Console.Write("PDB Filename: {0}\n", pdbfilenamewithpath);
               }
            }
            else if (args[i].Equals("-arch", StringComparison.OrdinalIgnoreCase))
            {
               if (i < args.Length - 1 && args[i + 1].Length > 0)
               {
                  arch = args[i + 1];
                  Console.Write("Arch: {0}\n", arch);
               }
            }
            else if (args[i].Equals("-date", StringComparison.OrdinalIgnoreCase))
            {
               if (i < args.Length - 1 && args[i + 1].Length > 0)
               {
                  string dateStr, yearStr, monthStr, dayStr;
                  int year = 1900, month = 01, day = 01;

                  dateStr = args[i + 1];
                  if (dateStr.Length != 10)
                  {
                     string msg = String.Format("{0}: Date string '{1}' is invalid length {2}.  Format is YYYY-MM-DD\n", MethodBase.GetCurrentMethod().Name, dateStr, dateStr.Length);
                     Log(msg);
                     SendEmail(String.Format("Daikatana Update - ERROR {0}", (int)ErrorCodes.DateTimeInvalidLen), msg);
                     System.Environment.Exit((int)ErrorCodes.DateTimeInvalidLen);
                  }
                  yearStr = dateStr.Substring(0, 4);
                  monthStr = dateStr.Substring(5, 2);
                  dayStr = dateStr.Substring(8, 2);

                  int.TryParse(yearStr, out year);
                  int.TryParse(monthStr, out month);
                  int.TryParse(dayStr, out day);

                  date = new DateTime(year, month, day);
                  Console.Write("Date: {0}\n", date.ToShortDateString());
               }
            }
            else if (args[i].Equals("-beta", StringComparison.OrdinalIgnoreCase))
            {
               if (i < args.Length - 1 && args[i + 1].Length > 0)
               {
                  string betaStr;
                  betaStr = args[i + 1];
                  int.TryParse(betaStr, out beta);
                  Console.Write("Beta flag: {0}\n", beta);
               }
            }
         }
      }

      static void GetMD5 ()
      {
         if (File.Exists(dkExePath) == false)
         {
            string msg = String.Format("{0}: Can't locate file {1}.  Aborting.\n", MethodBase.GetCurrentMethod().Name, dkExePath);
            Log(msg);
            SendEmail(String.Format("Daikatana Update - ERROR {0}", (int)ErrorCodes.DKExeMissing), msg);
            System.Environment.Exit((int)ErrorCodes.DKExeMissing);
         }

         md5Hash = md5.CreateMD5(File.ReadAllBytes(dkExePath));
         if (String.IsNullOrWhiteSpace(md5Hash))
         {
            string msg = String.Format("{0}: MD5 Hash is null.  Aborting.\n", MethodBase.GetCurrentMethod().Name);
            Log(msg);
            SendEmail(String.Format("Daikatana Update - ERROR {0}", (int)ErrorCodes.NullMD5Hash), msg);
            System.Environment.Exit((int)ErrorCodes.NullMD5Hash);
         }

         Console.Write("{0}\n", md5Hash);
      }

      static void GetConfigSettings()
      {
         try
         {
            SQLConnStr = cfgReader.GetSetting("SQLConnStr");
            if (String.IsNullOrWhiteSpace(SQLConnStr))
            {
               Log("{0}: SQLConnStr is null.  Aborting.\n", MethodBase.GetCurrentMethod().Name);
               System.Environment.Exit((int)ErrorCodes.NullSQLConnStr);
            }

            dkExePath = cfgReader.GetSetting("dkExePath");
            if (String.IsNullOrWhiteSpace(dkExePath))
            {
               Log("{0}: dkExePath is null.  Aborting.\n", MethodBase.GetCurrentMethod().Name);
               System.Environment.Exit((int)ErrorCodes.NullDKExePathStr);
            }

            fromAddress = new MailAddress(cfgReader.GetSetting("fromAddress"));
            if (String.IsNullOrWhiteSpace(fromAddress.Address))
            {
               Log("{0}: fromAdress is null.  Aborting\n", MethodBase.GetCurrentMethod().Name);
               System.Environment.Exit((int)ErrorCodes.EmailFromAddrNull);
            }

            toAddress = cfgReader.GetSetting("toAddress");
            if (String.IsNullOrWhiteSpace(toAddress))
            {
               Log("{0}: toAddress is null.  Aborting\n", MethodBase.GetCurrentMethod().Name);
               System.Environment.Exit((int)ErrorCodes.EmailToAddrNull);
            }

            emailHost = cfgReader.GetSetting("emailHost");
            if (String.IsNullOrWhiteSpace(emailHost))
            {
               Log("{0}: emailHost is null.  Aborting\n", MethodBase.GetCurrentMethod().Name);
               System.Environment.Exit((int)ErrorCodes.EmailHostNull);
            }

            emailUser = cfgReader.GetSetting("emailUser");
            if (String.IsNullOrWhiteSpace(emailHost))
            {
               Log("{0}: emailUser is null.  Aborting\n", MethodBase.GetCurrentMethod().Name);
               System.Environment.Exit((int)ErrorCodes.EmailUserNull);
            }

            emailPass = cfgReader.GetSetting("emailPass");
            if (String.IsNullOrWhiteSpace(emailHost))
            {
               Log("{0}: emailPass is null.  Aborting\n", MethodBase.GetCurrentMethod().Name);
               System.Environment.Exit((int)ErrorCodes.EmailPassNull);
            }
         }
         catch (Exception ex)
         {
            Log("{0}: Failed to parse config.  Reason: {1}\n", MethodBase.GetCurrentMethod().Name, ex.Message);
            System.Environment.Exit((int)ErrorCodes.CfgReaderException);
         }
      }

      static void SendEmail (string subject, string body)
      {
         StringBuilder sb = new StringBuilder(4096);

         sb.AppendLine("Daikatana Build Parameters");
         sb.AppendLine("--------------------------");
         sb.AppendLine(String.Format("dkExePath: {0}", dkExePath));
         sb.AppendLine(String.Format("filenamewithpath: {0}", filenamewithpath));
         sb.AppendLine(String.Format("pdbfilenamewithpath: {0}", pdbfilenamewithpath));
         sb.AppendLine(String.Format("arch: {0}", arch));
         sb.AppendLine(String.Format("beta: {0}", beta));
         sb.AppendLine(String.Format("date: {0}", date.ToShortDateString()));
         sb.AppendLine(String.Format("md5Hash: {0}", md5Hash));
         sb.AppendLine("--------------------------");
         sb.AppendLine(String.Format("\n{0}", body));

         try
         {
            clsEmail.Email(fromAddress, toAddress, sb.ToString(), subject, emailHost, emailUser, emailPass);
         }
         catch (Exception ex)
         {
            Console.Write(ex.Message);
         }
      }

      static void Main(string[] args)
      {
         cfgReader = new clsConfigReader(cfgFile);

         GetConfigSettings();

         ParseArgs(args);
         GetMD5();
         PushToSQL();

         System.Environment.Exit((int)ErrorCodes.OK);
      }
   }
}
