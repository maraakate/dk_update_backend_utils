using System.ServiceModel;

namespace DK_Upd_Build_WCF_Lib
{
   [ServiceContract]
   public interface IService1
   {
      [OperationContract]
      string StartBuild(int arch, int type, int beta);

      [OperationContract]
      string Ping ();
   }
}
