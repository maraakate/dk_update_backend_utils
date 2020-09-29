using DK_Upd_Build_Client.ServiceReference1;
using System;

namespace DK_Upd_Build_Client
{
   class Program
   {
      public static Service1Client clientService;

      static void Main(string[] args)
      {
         int arch, type, beta;

         try
         {
            clientService = new Service1Client();
            clientService.InnerChannel.Open(new TimeSpan(0, 5, 0));

            Console.WriteLine(clientService.Ping());

selectArch:
            Console.Write("Select Arch: 0 - Win32, 1 - Win64\n");
            arch = int.Parse(Console.ReadKey().KeyChar.ToString());
            if (arch != 0 && arch != 1)
            {
               Console.Write("Invalid option: {0}.  Try again.\n", arch);
               goto selectArch;
            }
            Console.WriteLine();

selectType:
            Console.Write("Select Arch: 0 - Release, 1 - Debug\n");
            type = int.Parse(Console.ReadKey().KeyChar.ToString());
            if (type != 0 && type != 1)
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

            Console.Write("Going to build {0} {1} {2}\n", arch, type, beta);

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
