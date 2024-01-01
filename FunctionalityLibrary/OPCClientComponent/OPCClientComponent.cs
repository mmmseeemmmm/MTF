using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPCClient;
using AutomotiveLighting.MTFCommon;
using OPCClient.Enums;
using OPCClientComponent.Models;
using OPCClient.Models;

namespace OPCClientComponent
{
    [MTFClass(Name = "OPC Client", Icon = MTFIcons.Communication)]
    [MTFClassCategory("Communication")]
    public class OPCClientComponent : IDisposable
    {
        private const string sessionName = "ALOPCSession";
        private bool disposed;
        private OPCClient.OPCClient opcClient;
        private OpcClientSettings opcClientSettings;

        [MTFConstructor(Description = "Anonymous identification")]        
        [MTFAllowedParameterValue("securityMode", "Invalid", "0")]
        [MTFAllowedParameterValue("securityMode", "None", "1")]
        [MTFAllowedParameterValue("securityMode", "Sign", "2")]
        [MTFAllowedParameterValue("securityMode", "SignAndEncrypt", "3")]
        [MTFAllowedParameterValue("securityPolicy", "None", "0")]
        [MTFAllowedParameterValue("securityPolicy", "Basic256", "1")]
        [MTFAllowedParameterValue("securityPolicy", "Basic128Rsa15", "2")]
        [MTFAllowedParameterValue("securityPolicy", "Basic256Sha256", "3")]
        [MTFAllowedParameterValue("messageEncoding", "Binary", "0")]
        [MTFAllowedParameterValue("messageEncoding", "Xml", "1")]
        public OPCClientComponent(string serverUrl, int securityMode, int securityPolicy, int messageEncoding)
        {
            this.saveServerSettings(serverUrl, (MessageSecurityMode)securityMode, (SecurityPolicy)securityPolicy, (MessageEncoding)messageEncoding, null);
            this.Connect();
            this.initComponent();
        }

        [MTFConstructor(Description = "User identification")]
        [MTFAllowedParameterValue("securityMode", "Invalid", "0")]
        [MTFAllowedParameterValue("securityMode", "None", "1")]
        [MTFAllowedParameterValue("securityMode", "Sign", "2")]
        [MTFAllowedParameterValue("securityMode", "SignAndEncrypt", "3")]
        [MTFAllowedParameterValue("securityPolicy", "None", "0")]
        [MTFAllowedParameterValue("securityPolicy", "Basic256", "1")]
        [MTFAllowedParameterValue("securityPolicy", "Basic128Rsa15", "2")]
        [MTFAllowedParameterValue("securityPolicy", "Basic256Sha256", "3")]
        [MTFAllowedParameterValue("messageEncoding", "Binary", "0")]
        [MTFAllowedParameterValue("messageEncoding", "Xml", "1")]
        public OPCClientComponent(string serverUrl, int securityMode, int securityPolicy, int messageEncoding, UserIdentification userIdentification)
        {
            this.saveServerSettings(serverUrl, (MessageSecurityMode)securityMode, (SecurityPolicy)securityPolicy, (MessageEncoding)messageEncoding, 
                new UserIdentity() { User = userIdentification.User, Password = userIdentification.Password });
            this.Connect();
            this.initComponent();
        }

        [MTFConstructor(Description = "Certificate identification")]
        [MTFAllowedParameterValue("securityMode", "Invalid", "0")]
        [MTFAllowedParameterValue("securityMode", "None", "1")]
        [MTFAllowedParameterValue("securityMode", "Sign", "2")]
        [MTFAllowedParameterValue("securityMode", "SignAndEncrypt", "3")]
        [MTFAllowedParameterValue("securityPolicy", "None", "0")]
        [MTFAllowedParameterValue("securityPolicy", "Basic256", "1")]
        [MTFAllowedParameterValue("securityPolicy", "Basic128Rsa15", "2")]
        [MTFAllowedParameterValue("securityPolicy", "Basic256Sha256", "3")]
        [MTFAllowedParameterValue("messageEncoding", "Binary", "0")]
        [MTFAllowedParameterValue("messageEncoding", "Xml", "1")]
        public OPCClientComponent(string serverUrl, int securityMode, int securityPolicy, int messageEncoding, CertificateIdentification certificateIdentification)
        {
            this.saveServerSettings(serverUrl, (MessageSecurityMode)securityMode, (SecurityPolicy)securityPolicy, (MessageEncoding)messageEncoding,
                new CertificateIdentity() { Path = certificateIdentification.Path, Password = certificateIdentification.Password });
            this.Connect();
            this.initComponent();
        }

        private void initComponent()
        {
            this.disposed = false;
        }

        private void saveServerSettings(string serverUrl, MessageSecurityMode securityMode, SecurityPolicy securityPolicy, MessageEncoding messageEncoding, object identification)
        {
            this.opcClientSettings = new OpcClientSettings()
            {
                ServerUrl = serverUrl,
                SecurityMode = securityMode,
                SecurityPolicy = securityPolicy,
                MessageEncoding = messageEncoding,
                Identification = identification
            };
        }

        //[MTFMethod]
        public void Connect()
        {
            this.Disconnect();
            
            if (this.opcClientSettings.Identification?.GetType() == typeof(CertificateIdentity))
            {
                this.opcClient = new OPCClient.OPCClient(
                    sessionName, this.opcClientSettings.ServerUrl,
                    this.opcClientSettings.SecurityMode,
                    this.opcClientSettings.SecurityPolicy,
                    this.opcClientSettings.MessageEncoding,
                    (UserIdentity)this.opcClientSettings.Identification
                    );
            }
            else if (this.opcClientSettings.Identification?.GetType() == typeof(CertificateIdentity))
            {
                this.opcClient = new OPCClient.OPCClient(
                    sessionName, this.opcClientSettings.ServerUrl,
                    this.opcClientSettings.SecurityMode,
                    this.opcClientSettings.SecurityPolicy,
                    this.opcClientSettings.MessageEncoding,
                    (CertificateIdentity)this.opcClientSettings.Identification
                    );
            }
            else
            {
                this.opcClient = new OPCClient.OPCClient(
                    sessionName, this.opcClientSettings.ServerUrl,
                    this.opcClientSettings.SecurityMode,
                    this.opcClientSettings.SecurityPolicy,
                    this.opcClientSettings.MessageEncoding
                    );
            }
        }

        //[MTFMethod]
        public void Disconnect()
        {
            this.opcClient?.Dispose();
            this.opcClient = null;
        }
        
        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public bool ReadBoolVariable(string objectName, string variableName)
        {
            return Convert.ToBoolean(this.opcClient.ReadValue(objectName, variableName));
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public void WriteBoolVariable(string objectName, string variableName, bool value)
        {
            this.opcClient.WriteVariable(objectName, variableName, value);
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public int ReadIntVariable(string objectName, string variableName)
        {
            return Convert.ToInt32(this.opcClient.ReadValue(objectName, variableName));
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public void WriteIntVariable(string objectName, string variableName, int value)
        {
            this.opcClient.WriteVariable(objectName, variableName, value);
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public double ReadDoubleVariable(string objectName, string variableName)
        {
            return Convert.ToDouble(this.opcClient.ReadValue(objectName, variableName));
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public void WriteDoubleVariable(string objectName, string variableName, double value)
        {
            this.opcClient.WriteVariable(objectName, variableName, value);
        }        

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public string ReadStringVariable(string objectName, string variableName)
        {
            return Convert.ToString(this.opcClient.ReadValue(objectName, variableName));
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "objectName", ValueListName = "Objects")]
        [MTFAdditionalParameterInfo(ParameterName = "variableName", ValueListName = "Variables")]
        public void WriteStringVariable(string objectName, string variableName, string value)
        {
            this.opcClient.WriteVariable(objectName, variableName, value);
        }

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> ServerDescription()
        {
            var descriptionList = new List<Tuple<string, object>>();

            foreach (var opcObject in this.opcClient.GetServerDescription())
            {
                descriptionList.Add(new Tuple<string, object>(opcObject.Name, null));

                foreach (var variable in opcObject.Variables)
                {
                    descriptionList.Add(new Tuple<string, object>(" - " + variable.Name + " - " + variable.AccessLevel.ToString() + " - " + variable.NodeId, null));
                }
            }

            return descriptionList;
        }

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> Objects()
        {
            var objects = new List<Tuple<string, object>>();

            foreach (var opcObject in this.opcClient.GetServerDescription())
            {
                objects.Add(new Tuple<string, object>(opcObject.Name, opcObject.Name));
            }

            return objects;
        }

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> Variables()
        {
            var variables = new List<Tuple<string, object>>();

            foreach (var opcObject in this.opcClient.GetServerDescription())
            {
                foreach (var variable in opcObject.Variables)
                {
                    variables.Add(new Tuple<string, object>(variable.Name, variable.Name));
                }                
            }

            return variables;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OPCClientComponent()
        {
            this.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                this.opcClient?.Dispose();                
            }

            disposed = true;
        }
    }
}
