﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DK_Upd_Build_Client.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IService1")]
    public interface IService1 {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/StartBuild", ReplyAction="http://tempuri.org/IService1/StartBuildResponse")]
        string StartBuild(int arch, int type, int beta);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/StartBuild", ReplyAction="http://tempuri.org/IService1/StartBuildResponse")]
        System.Threading.Tasks.Task<string> StartBuildAsync(int arch, int type, int beta);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/Ping", ReplyAction="http://tempuri.org/IService1/PingResponse")]
        string Ping();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/Ping", ReplyAction="http://tempuri.org/IService1/PingResponse")]
        System.Threading.Tasks.Task<string> PingAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IService1Channel : DK_Upd_Build_Client.ServiceReference1.IService1, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class Service1Client : System.ServiceModel.ClientBase<DK_Upd_Build_Client.ServiceReference1.IService1>, DK_Upd_Build_Client.ServiceReference1.IService1 {
        
        public Service1Client() {
        }
        
        public Service1Client(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public Service1Client(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string StartBuild(int arch, int type, int beta) {
            return base.Channel.StartBuild(arch, type, beta);
        }
        
        public System.Threading.Tasks.Task<string> StartBuildAsync(int arch, int type, int beta) {
            return base.Channel.StartBuildAsync(arch, type, beta);
        }
        
        public string Ping() {
            return base.Channel.Ping();
        }
        
        public System.Threading.Tasks.Task<string> PingAsync() {
            return base.Channel.PingAsync();
        }
    }
}
