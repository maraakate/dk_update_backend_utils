using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DK_Upd_Build_WCF_Lib
{
   [ServiceContract]
   public interface IService1
   {
      [OperationContract]
      bool StartBuild(int arch, int type, int beta);

      [OperationContract]
      string Ping ();
   }
}
