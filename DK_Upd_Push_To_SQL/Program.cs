using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
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
        private static string dkPath = string.Empty;
        private static readonly string cfgFile = "DK_Upd_Push_To_SQL.exe.config";
        public static string filenamewithpath, pdbfilenamewithpath, arch;
        public static int beta;
        public static DateTime date = DateTime.Now;
        public static string md5Hash = string.Empty;
        public static clsConfigReader cfgReader;

        /* FS: Email args */
        private static bool useEmail = false;
        private static MailAddress fromAddress;
        private static string toAddress, emailHost, emailUser, emailPass;

        /* FS: FTP args */
        private static bool useFtp = false;
        private static string ftpExe, ftpAddress, ftpPort, ftpUser, ftpPass, ftpDirectory;

        public static readonly List<string> ListArch = new List<string> { "Win32", "Win64", "Linux", "Linux_x64", "FreeBSD", "OSX" };
        public static readonly List<string> ListWin32Files = new List<string> { "dk_win32.txt", "dk_win32.md5" };
        public static readonly List<string> ListWin32BetaFiles = new List<string> { "dk_win32_beta.txt", "dk_win32_beta.md5" };
        public static readonly List<string> ListWin64Files = new List<string> { "dk_win64.txt", "dk_win64.md5" };
        public static readonly List<string> ListWin64BetaFiles = new List<string> { "dk_win64_beta.txt", "dk_win64_beta.md5" };
        public static readonly List<string> ListDOSFiles = new List<string> { "dk_dos.txt", "dk_dos.md5" };
        public static readonly List<string> ListDOSBetaFiles = new List<string> { "dk_dos_beta.txt", "dk_dos_beta.md5" };

        static void PushToSQL()
        {
            StringBuilder Query;
            string binFileName, pdbFileName, msg;
            Collection<SqlParameter> Parameters;
            Guid guid;

            guid = Guid.NewGuid();
            msg = string.Empty;

            if (String.IsNullOrWhiteSpace(filenamewithpath))
            {
                Log("{0}: filenamewithpath is null.\r\n", MethodBase.GetCurrentMethod().Name);
                System.Environment.Exit((int)ErrorCodes.NullFileNamePath);
                return;
            }

            if (String.IsNullOrWhiteSpace(pdbfilenamewithpath))
            {
                Log("{0}: pdbfilenamewithpath is null.\r\n", MethodBase.GetCurrentMethod().Name);
                System.Environment.Exit((int)ErrorCodes.NullPDBFilePath);
                return;
            }

            if (String.IsNullOrWhiteSpace(arch))
            {
                Log("{0}: arch is null.\r\n", MethodBase.GetCurrentMethod().Name);
                System.Environment.Exit((int)ErrorCodes.NullArch);
                return;
            }

            using (clsSQL dbSQL = new clsSQL(SQLConnStr))
            {
                Query = new StringBuilder(4096);
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
                        msg = String.Format("{0}: Failed Query: {1}\r\n", MethodBase.GetCurrentMethod().Name, dbSQL.LastErrorMessage);
                        Log(msg);
                        SendEmail(String.Format("Daikatana Update - ERROR {0}", (int)ErrorCodes.ErrorSQLQuery), msg);
                        System.Environment.Exit((int)ErrorCodes.ErrorSQLQuery);
                        return;
                    }

                    msg = String.Format("Success!  ID: {0}\r\n", guid.ToString());
                    Log(msg);
                    SendEmail("Daikatana Update - SUCCESS", msg);
                    PushToFTP();
                }
                catch (Exception ex)
                {
                    msg = String.Format("{0}: Failed: {1}\r\n", MethodBase.GetCurrentMethod().Name, ex.Message);
                    Log(msg);
                    SendEmail(String.Format("Daikatana Update - FAILED {0}", (int)ErrorCodes.ErrorSQLQuery), msg);
                    System.Environment.Exit((int)ErrorCodes.ErrorSQLQuery);
                }
            }
        }

        static void ParseArgs(string[] args)
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
                            string msg = String.Format("{0}: Date string '{1}' is invalid length {2}.  Format is YYYY-MM-DD\r\n", MethodBase.GetCurrentMethod().Name, dateStr, dateStr.Length);
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

        static void GetMD5()
        {
            if (File.Exists(dkExePath) == false)
            {
                string msg = String.Format("{0}: Can't locate file {1}.  Aborting.\r\n", MethodBase.GetCurrentMethod().Name, dkExePath);
                Log(msg);
                SendEmail(String.Format("Daikatana Update - ERROR {0}", (int)ErrorCodes.DKExeMissing), msg);
                System.Environment.Exit((int)ErrorCodes.DKExeMissing);
            }

            md5Hash = md5.CreateMD5(File.ReadAllBytes(dkExePath));
            if (String.IsNullOrWhiteSpace(md5Hash))
            {
                string msg = String.Format("{0}: MD5 Hash is null.  Aborting.\r\n", MethodBase.GetCurrentMethod().Name);
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
                    Log("{0}: SQLConnStr is null.  Aborting.\r\n", MethodBase.GetCurrentMethod().Name);
                    System.Environment.Exit((int)ErrorCodes.NullSQLConnStr);
                }

                dkExePath = cfgReader.GetSetting("dkExePath");
                if (String.IsNullOrWhiteSpace(dkExePath))
                {
                    Log("{0}: dkExePath is null.  Aborting.\r\n", MethodBase.GetCurrentMethod().Name);
                    System.Environment.Exit((int)ErrorCodes.NullDKExePathStr);
                }

                dkPath = cfgReader.GetSetting("dkPath");
                if (String.IsNullOrWhiteSpace(dkPath))
                {
                    Log("{0}: dkPath is null.  Aborting.\r\n", MethodBase.GetCurrentMethod().Name);
                    System.Environment.Exit((int)ErrorCodes.NulldkPathStr);
                }

                string useEmailStr = cfgReader.GetSetting("useEmail");
                if ((String.IsNullOrWhiteSpace(useEmailStr) == false)
                   && (useEmailStr.Equals("0", StringComparison.OrdinalIgnoreCase) == false)
                   && (useEmailStr.Equals("false", StringComparison.OrdinalIgnoreCase) == false))
                {
                    fromAddress = new MailAddress(cfgReader.GetSetting("fromAddress"));
                    if (String.IsNullOrWhiteSpace(fromAddress.Address))
                    {
                        Log("{0}: fromAdress is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.EmailFromAddrNull);
                    }

                    toAddress = cfgReader.GetSetting("toAddress");
                    if (String.IsNullOrWhiteSpace(toAddress))
                    {
                        Log("{0}: toAddress is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.EmailToAddrNull);
                    }

                    emailHost = cfgReader.GetSetting("emailHost");
                    if (String.IsNullOrWhiteSpace(emailHost))
                    {
                        Log("{0}: emailHost is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.EmailHostNull);
                    }

                    emailUser = cfgReader.GetSetting("emailUser");
                    if (String.IsNullOrWhiteSpace(emailUser))
                    {
                        Log("{0}: emailUser is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.EmailUserNull);
                    }

                    emailPass = cfgReader.GetSetting("emailPass");
                    if (String.IsNullOrWhiteSpace(emailPass))
                    {
                        Log("{0}: emailPass is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.EmailPassNull);
                    }

                    useEmail = true;
                }

                string useFtpStr = cfgReader.GetSetting("useFtp");
                if ((String.IsNullOrWhiteSpace(useFtpStr) == false)
                   && (useFtpStr.Equals("0", StringComparison.OrdinalIgnoreCase) == false)
                   && (useFtpStr.Equals("false", StringComparison.OrdinalIgnoreCase) == false))
                {
                    ftpExe = cfgReader.GetSetting("ftpExe");
                    if (String.IsNullOrWhiteSpace(ftpExe))
                    {
                        Log("{0}: ftpExe is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.FtpExeNull);
                    }

                    ftpAddress = cfgReader.GetSetting("ftpAddress");
                    if (String.IsNullOrWhiteSpace(ftpAddress))
                    {
                        Log("{0}: ftpAddress is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.FtpAddressNull);
                    }

                    ftpPort = cfgReader.GetSetting("ftpPort");
                    if (String.IsNullOrWhiteSpace(ftpPort))
                    {
                        Log("{0}: ftpPort is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.FtpPortNull);
                    }

                    ftpUser = cfgReader.GetSetting("ftpUser");
                    if (String.IsNullOrWhiteSpace(ftpUser))
                    {
                        Log("{0}: ftpUser is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.FtpUserNull);
                    }

                    ftpPass = cfgReader.GetSetting("ftpPass");
                    if (String.IsNullOrWhiteSpace(ftpPass))
                    {
                        Log("{0}: ftpPass is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.FtpPassNull);
                    }

                    ftpDirectory = cfgReader.GetSetting("ftpDirectory");
                    if (String.IsNullOrWhiteSpace(ftpDirectory))
                    {
                        Log("{0}: ftpDirectory is null.  Aborting\r\n", MethodBase.GetCurrentMethod().Name);
                        System.Environment.Exit((int)ErrorCodes.FtpDirectoryNull);
                    }

                    useFtp = true;
                }
            }
            catch (Exception ex)
            {
                Log("{0}: Failed to parse config.  Reason: {1}\r\n", MethodBase.GetCurrentMethod().Name, ex.Message);
                System.Environment.Exit((int)ErrorCodes.CfgReaderException);
            }
        }

        static void SendEmail(string subject, string body)
        {
            if (useEmail == false)
                return;

            StringBuilder sb = new StringBuilder(4096);
            string[] attachments = { };

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
                clsEmail.Email(fromAddress, toAddress, sb.ToString(), subject, attachments, emailHost, emailUser, emailPass);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        static void EmailTest()
        {
            if (useEmail == false)
                return;

            try
            {
                string[] attachments = { };

                clsEmail.Email(fromAddress, toAddress, "DK Updater test\n", "DK Updater Test", attachments, emailHost, emailUser, emailPass);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

        }

        static void PushToFTP()
        {
            Process ftpProcess;
            List<string> files = new List<string> { };

            if (useFtp == false)
                return;

            if (arch.Equals("win32", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(ListWin32BetaFiles[0]);
                files.Add(ListWin32BetaFiles[1]);

                if (beta <= 0)
                {
                    files.Add(ListWin32Files[0]);
                    files.Add(ListWin32Files[1]);
                }
            }
            else if (arch.Equals("win64", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(ListWin64BetaFiles[0]);
                files.Add(ListWin64BetaFiles[1]);

                if (beta <= 0)
                {
                    files.Add(ListWin64Files[0]);
                    files.Add(ListWin64Files[1]);
                }
            }
            else if (arch.Equals("dos", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(ListDOSBetaFiles[0]);
                files.Add(ListDOSBetaFiles[1]);

                if (beta <= 0)
                {
                    files.Add(ListDOSFiles[0]);
                    files.Add(ListDOSFiles[1]);
                }
            }
            else
            {
                return;
            }

            files.Add(filenamewithpath);
            files.Add(pdbfilenamewithpath);

            using (StreamWriter sw = new StreamWriter(Path.Combine(dkPath, files[0]), false))
            {
                string fileNoPath = Path.GetFileName(filenamewithpath);
                sw.Write(fileNoPath);
                sw.Write('\n');
                sw.Flush();
            }

            using (StreamWriter sw = new StreamWriter(Path.Combine(dkPath, files[1]), false))
            {
                sw.Write(md5Hash);
                sw.Write('\n');
                sw.Flush();
            }

            if (beta <= 0)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(dkPath, files[2]), false))
                {
                    string fileNoPath = Path.GetFileName(filenamewithpath);
                    sw.Write(fileNoPath);
                    sw.Write('\n');
                    sw.Flush();
                }

                using (StreamWriter sw = new StreamWriter(Path.Combine(dkPath, files[3]), false))
                {
                    sw.Write(md5Hash);
                    sw.Write('\n');
                    sw.Flush();
                }
            }

            foreach (string file in files)
            {
                ftpProcess = new Process();
                ftpProcess.StartInfo.FileName = ftpExe;
                ftpProcess.StartInfo.Arguments = String.Format("-u {0} -p {1} -P {2} {3} {4} {5}", ftpUser, ftpPass, ftpPort, ftpAddress, ftpDirectory, Path.Combine(dkPath, file));
                ftpProcess.Start();
                ftpProcess.WaitForExit();
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
