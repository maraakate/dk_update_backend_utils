using DK_Upd_Build_Client.ServiceReference1;
using System;
using System.Collections.Generic;

namespace DK_Upd_Build_Client
{
    class Program
    {
        public static Service1Client clientService;
        private static readonly List<string> archList = new List<string> { "Win32", "Win64", "DOS" };
        private static readonly List<string> typeList = new List<string> { "Release", "Debug", "Release FULL NO New Pak6", "Release FULL WITH New Pak6" };

        static void Main(string[] args)
        {
            int arch, type, beta;

            try
            {
                clientService = new Service1Client();
                clientService.InnerChannel.Open(new TimeSpan(0, 5, 0));

                Console.WriteLine(clientService.Ping());

            selectArch:
                Console.Write("Select Arch: ");
                for (int x = 0; x < archList.Count; x++)
                {
                    Console.Write("{0} - {1}{2}", x, archList[x], x != archList.Count - 1 ? ", " : "");
                }
                Console.Write("\n");

                arch = int.Parse(Console.ReadKey().KeyChar.ToString());
                if (arch != 0 && arch != 1 && arch != 2)
                {
                    Console.Write("Invalid option: {0}.  Try again.\n", arch);
                    goto selectArch;
                }
                Console.WriteLine();

            selectType:
                Console.Write("Select Type: ");
                for (int x = 0; x < typeList.Count; x++)
                {
                    Console.Write("{0} - {1}{2}", x, typeList[x], x != typeList.Count - 1 ? ", " : "");
                }
                Console.Write("\n");

                type = int.Parse(Console.ReadKey().KeyChar.ToString());
                if (type != 0 && type != 1 && type != 2 && type != 3)
                {
                    Console.Write("Invalid option: {0}.  Try again.\n", type);
                    goto selectType;
                }
                Console.WriteLine();

            selectBeta:
                Console.Write("Select Beta: 0 - No, 1 - Yes\n");
                beta = int.Parse(Console.ReadKey().KeyChar.ToString());
                if (beta != 0 && beta != 1)
                {
                    Console.Write("Invalid option: {0}.  Try again.\n", beta);
                    goto selectBeta;
                }
                Console.WriteLine();

                Console.Write("Going to build {0} {1} {2}\n", archList[arch], typeList[type], beta > 0 ? "Beta" : "Non-Beta");

                /* FS: Crud hack. */
                if (arch == 2)
                    arch = 6;

                Console.Write(clientService.StartBuild(arch, type, beta));
                clientService.Close();
            }
            catch (Exception ex)
            {
                Console.Write("Failed {0}\n", ex.Message);
            }

            Console.Write("Done\n");
            Console.ReadKey();
        }
    }
}
