using OPCClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPCClientComponent.Models
{
    public class OpcClientSettings
    {
        private string serverUrl;
        private MessageSecurityMode securityMode;
        private SecurityPolicy securityPolicy;
        private MessageEncoding messageEncoding;
        private object identification;

        public string ServerUrl
        {
            get
            {
                return this.serverUrl;
            }
            set
            {
                this.serverUrl = value;
            }
        }

        public MessageSecurityMode SecurityMode
        {
            get
            {
                return this.securityMode;
            }
            set
            {
                this.securityMode = value;
            }
        }

        public SecurityPolicy SecurityPolicy
        {
            get
            {
                return this.securityPolicy;
            }
            set
            {
                this.securityPolicy = value;
            }
        }

        public MessageEncoding MessageEncoding
        {
            get
            {
                return this.messageEncoding;
            }
            set
            {
                this.messageEncoding = value;
            }
        }

        public object Identification
        {
            get
            {
                return this.identification;
            }
            set
            {
                this.identification = value;
            }
        }
    }
}
