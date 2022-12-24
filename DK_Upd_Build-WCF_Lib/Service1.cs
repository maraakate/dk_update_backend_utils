using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DK_Upd_Build_WCF_Lib
{
    public partial class Service1 : IService1
    {
        public const int TYPERELEASE = 0;
        public const int TYPEDEBUG = 1;

        public const int ARCHWIN32 = 0;
        public const int ARCHWIN64 = 1;
        public const int ARCHLINUX32 = 2;
        public const int ARCHLINUX64 = 3;
        public const int ARCHFREEBSD = 4;
        public const int ARCHOSX = 5;
        public const int ARCHDOS = 6;
        public readonly List<string> ListArch = new List<string> { "Win32", "Win64", "Linux", "Linux_x64", "FreeBSD", "OSX", "DOS" };
        public readonly List<string> ListWin32Batches = new List<string> {"buildrelease32.bat", "builddebug32.bat", "buildrelease32_full_no_newpak6.bat", "buildrelease32_full_with_newpak6.bat" };
        public readonly List<string> ListWin32BetaBatches = new List<string> { "buildrelease32_beta.bat", "builddebug32_beta.bat" };
        public readonly List<string> ListWin64Batches = new List<string> { "buildrelease64.bat", "builddebug64.bat", "buildrelease64_full_no_newpak6.bat", "buildrelease64_full_with_newpak6.bat" };
        public readonly List<string> ListWin64BetaBatches = new List<string> { "buildrelease64_beta.bat", "builddebug64_beta.bat" };
        public readonly List<string> ListDOSBatches = new List<string> { "buildreleaseDOS.bat", "builddebugDOS.bat" };
        public readonly List<string> ListDOSBetaBatches = new List<string> { "buildreleaseDOS_beta.bat", "builddebugDOS_beta.bat" };

        private bool GetBatch (int arch, int type, int beta, out string batchFile)
        {
            batchFile = string.Empty;

            switch (arch)
            {
                case ARCHWIN32:
                    if (beta > 0)
                    {
                        batchFile = ListWin32BetaBatches[type];
                    }
                    else
                    {
                        batchFile = ListWin32Batches[type];
                    }
                    return true;

                case ARCHWIN64:
                    if (beta > 0)
                    {
                        batchFile = ListWin64BetaBatches[type];
                    }
                    else
                    {
                        batchFile = ListWin64Batches[type];
                    }
                    return true;

                case ARCHDOS:
                    if (beta > 0)
                    {
                        batchFile = ListDOSBetaBatches[type];
                    }
                    else
                    {
                        batchFile = ListDOSBatches[type];
                    }
                    return true;

                default:
                    return false;
            }
        }

        public string StartBuild (int arch, int type, int beta)
        {
            try
            {
                string batchFile, cwd;
                clsConfigReader configReader;

                if (GetBatch(arch, type, beta, out batchFile) == false)
                {
                    return string.Format("Invalid options {0} {1} {2}\n", arch, type, beta);
                }

                using (Process batch = new Process())
                {
                    configReader = new clsConfigReader("dk.config");
                    cwd = configReader.GetSetting("WorkingDirectory");
                    if (String.IsNullOrWhiteSpace(cwd))
                    {
                        return string.Format("WorkingDirectory is null.\n");
                    }
                    string buildLog = Path.Combine(cwd, "buildlog.txt");
                    if (File.Exists(buildLog))
                    {
                        try
                        {
                            File.Delete(buildLog);
                        }
                        catch (Exception Ex)
                        {
                            return String.Format("Failed delete old build log.  Reason: {0}\n", Ex.Message);
                        }
                    }

                    batch.StartInfo.FileName = Path.Combine(cwd, batchFile);
                    batch.StartInfo.Arguments = string.Empty;
                    batch.StartInfo.WorkingDirectory = cwd;
                    batch.StartInfo.UseShellExecute = false;
                    batch.StartInfo.CreateNoWindow = true;
                    batch.StartInfo.RedirectStandardOutput = true;
                    batch.EnableRaisingEvents = true;
                    batch.Start();
                    Redirect(batch.StandardOutput, buildLog);

                    while (batch.HasExited == false)
                    {
                        Thread.Sleep(250);
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Format("Failed: {0}\n", ex.Message);
            }

            return "Success\n";
        }

        private void Redirect (StreamReader input, string buildLog)
        {
            char[] buffer = new char[1];

            new Thread(a =>
            {
                while (input.Read(buffer, 0, 1) > 0)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(buildLog, true))
                        {
                            sw.Write(buffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Failed to write to log.  {0}\n", ex.Message);
                    }

                    Console.Write(buffer);
                };
            }).Start();
        }

        public string Ping()
        {
            return "Pong";
        }
    }
}
