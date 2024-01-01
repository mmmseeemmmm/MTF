using MTFClientServerCommon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MTFClientServerCommon.MTFAccessControl.AnetAccessKey
{
    public delegate void DataReceivedANETId(string data);

    public class AnetAccessKeyProvider : AccessKeyProvider
    {
        private const string IpAddressParameter = "IP address";
        private const string PortParameter = "Port";
        private const string DataFileParameter = "Data file";

        private TcpClient client = null;
        private NetworkStream stream = null;
        private bool listenClient = false;

        private string startString = string.Empty;
        private AccessKey lastLogPerson = null;
        public event DataReceivedANETId DataReceived;
        private readonly Dictionary<ulong, Person> personList = new Dictionary<ulong, Person>();

        public AnetAccessKeyProvider()
        {
            Parameters = new List<AccessKeyProviderParameter>
            {
                new AccessKeyProviderParameter{Name = IpAddressParameter},
                new AccessKeyProviderParameter{Name = PortParameter, Value = "10001"},
                new AccessKeyProviderParameter{Name = DataFileParameter},
            };
        }

        public override void Init()
        {
            Connect();
            var xmlData = XmlOperations.LoadXmlData<DataTypePerson>(DataFileName);
            if (xmlData!=null)
            {
                SecurityHelper.DecryptData(xmlData.Person);
                foreach (var person in xmlData.Person)
                {
                    if (person!=null)
                    {
                        try
                        {
                            var longId = Convert.ToUInt64(person.ANETId, 16);
                            personList[longId] = person; 
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                        }
                        
                    }
                }
            }
        }

        public override bool CanReconnect
        {
            get { return true; }
        }

        public override bool HasConfigControl
        {
            get { return true; }
        }

        public override UserControl ConfigControl
        {
            get { return new ANETConfigControl(this); }
        }

        public override void Destroy()
        {
            listenClient = false;
            if (stream != null)
            {
                stream.Close();
            }
            if (client != null)
            {
                client.Close();
            }
        }

        protected void OnDataReceived(string data)
        {
            var lengthKey = data.IndexOf(Environment.NewLine);
            var indexStartLastLine = data.LastIndexOf(Environment.NewLine);
            if (indexStartLastLine == -1)
            {
                startString = data;
                return;
            }
            else if (!string.IsNullOrEmpty(startString))
            {
                data = string.Concat(startString, data);
                indexStartLastLine += startString.Length;
                lengthKey += startString.Length;
                startString = string.Empty;
            }

            string accessKey = data.Substring(indexStartLastLine - lengthKey, lengthKey);

            if (OpenConfigControl)
            {
                if (DataReceived != null)
                {
                    DataReceived(accessKey);
                }
            }
            else
            {
                ulong longAccesKey;
                try
                {
                    longAccesKey = Convert.ToUInt64(accessKey, 16);
                }
                catch (Exception ex)
                {
                    RaiseError(ex.Message);
                    return;
                }
                if (personList.ContainsKey(longAccesKey))
                {
                    var selectedUser = personList[longAccesKey];
                    if (lastLogPerson != null && lastLogPerson.KeyCreatorName.Equals(selectedUser.ANETId))
                    {
                        lastLogPerson = null;
                    }
                    else
                    {
                        var selectedRoles = new List<AccessRole>();
                        foreach (var role in selectedUser.Roles)
                        {
                            selectedRoles.Add(new AccessRole(role.Name));
                        }

                        var listAccessMachine = new List<AccessMachine>();
                        listAccessMachine.Add(new AccessMachine(Environment.MachineName, selectedRoles, null));

                        lastLogPerson = new AccessKey(selectedUser.FirstName, selectedUser.LastName, accessKey, selectedUser.Expiration, listAccessMachine, "ANET", string.Empty, string.Empty, string.Empty);
                    }
                    SetAccessKey(lastLogPerson);
                }
                else
                {
                    RaiseError("For this ANET Id isn't assigned any users!!!");
                }
                
            }
        }


        private void Connect()
        {
            int maxAttempts = 5;
            int i = 0;
            bool readyConnection = false;
            Exception exception = null;

            do
            {
                try
                {
                    client = new TcpClient(IpAddress, Port);
                    readyConnection = true;
                }
                catch (Exception ex)
                {
                    i++;
                    exception = ex;
                    Thread.Sleep(500);
                }

                if (readyConnection)
                {
                    stream = client.GetStream();
                    listenClient = true;
                    Task.Run(() =>
                    {
                        Byte[] data = new Byte[256];
                        while (listenClient)
                        {
                            if (listenClient && stream.DataAvailable)
                            {
                                Int32 bytes = stream.Read(data, 0, data.Length);
                                OnDataReceived(System.Text.Encoding.ASCII.GetString(data, 0, bytes));
                            }
                            Thread.Sleep(100);
                        }
                    });
                }

            } while (!readyConnection && i < maxAttempts);

            if (!readyConnection)
            {
                RaiseError(exception.Message);
            }
        }

        private string IpAddress
        {
            get { return Parameters.First(p => p.Name == IpAddressParameter).Value; }
        }

        private int Port
        {
            get { return int.Parse(Parameters.First(p => p.Name == PortParameter).Value); }
        }

        public string DataFileName
        {
            get { return Parameters.First(p => p.Name == DataFileParameter).Value; }
        }
    }
}
