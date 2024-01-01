using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

namespace MTFClientServerCommon.MTFAccessControl.AnetAccessKey
{
    [Serializable]
    public class Person : INotifyPropertyChanged, ICloneable
    {
        public Person()
        {
            Id = Guid.NewGuid();
        }

        private Guid id;
        public Guid Id
        {
            get => id;
            set => id = value;
        }

        private string lastName;
        public string LastName
        {
            get => lastName;
            set 
            { 
                lastName = value;
                IsModify = true;
                NotifyPropertyChanged();
            }
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set 
            { 
                firstName = value;
                IsModify = true;
                NotifyPropertyChanged();
            }
        }

        private DateTime expiration;
        public DateTime Expiration
        {
            get => expiration;
            set 
            { 
                expiration = value;
                IsModify = true;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Role> roles;
        public ObservableCollection<Role> Roles
        {
            get => roles;
            set 
            { 
                roles = value;
                IsModify = true;
                NotifyPropertyChanged();
                roles.CollectionChanged += (s, e) => NotifyPropertyChanged("StringRoles");
            }
        }

        private string anetId;
        public string ANETId
        {
            get => anetId;
            set 
            { 
                anetId = value;
                IsModify = true;
                NotifyPropertyChanged();
            }
        }

        private bool isModify = false;
        [XmlIgnore]
        public bool IsModify
        {
            get => isModify;
            set => isModify = value;
        }

        private string stringRoles = string.Empty;
        [XmlIgnore]
        public string StringRoles
        {
            get 
            {
                stringRoles = string.Empty;
                if (Roles != null && Roles.Count > 0)
                {
                    var roles = new StringBuilder();
                    foreach (var role in Roles)
                    {
                        roles.Append(role.Name);
                        roles.Append(", ");
                    }
                    roles.Length -= 2;
                    stringRoles = roles.ToString();
                }
                return stringRoles;
            }
            set
            {
                stringRoles = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
