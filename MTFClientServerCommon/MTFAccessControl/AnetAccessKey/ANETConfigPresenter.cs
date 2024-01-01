using MTFClientServerCommon.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MTFClientServerCommon.MTFAccessControl.AnetAccessKey
{
    public class ANETConfigPresenter : INotifyPropertyChanged
    {
        private bool canDeletePerson = false;
        private Command deleteSelectedPerson;
        private Command assignRole;
        private AnetAccessKeyProvider provider;

        public ANETConfigPresenter(AnetAccessKeyProvider provider)
        {
            this.provider = provider;
            loadData();
            inicializationCommand();
            isModifyAllPersonIsFalse();
            provider.DataReceived += provider_DataReceived;
        }

        private void loadData()
        {
            var data = XmlOperations.LoadXmlData<DataTypePerson>(provider.DataFileName);
            if (data.Person == null)
            {
                Person = new ObservableCollection<Person>();
            }
            else
            {
                SecurityHelper.DecryptData(data.Person);
                Person = new ObservableCollection<Person>(data.Person);
            }

            if (data.AllRoles == null)
            {
                AllRoles = new ObservableCollection<Role>();
            }
            else
            {
                AllRoles = new ObservableCollection<Role>(data.AllRoles);
            }
        }

        private void inicializationCommand()
        {
            deleteSelectedPerson = new Command(deletePerson, () => canDeletePerson);
            assignRole = new Command(assignNewRole, () => CanAddRole);
        }

        void provider_DataReceived(string accessKey)
        {
            if (SelectedPerson != null)
            {
                var checkKey = Person.FirstOrDefault(x => x.ANETId == accessKey);
                if (checkKey != null)
                {
                    provider.ShowMessage("MTF - error", string.Format("This ANET Id is already assign {0} {1}!!!", checkKey.FirstName, checkKey.LastName), false);
                    return;
                }

                if (string.IsNullOrEmpty(SelectedPerson.ANETId))
                {
                    setANETId(accessKey);
                }
                else if (!SelectedPerson.ANETId.Equals(accessKey))
                {
                    if (provider.ShowMessage("MTF", string.Format("User {0} {1} have got already ANET Id. {2} Want you him assigned new ANET Id?", 
                        SelectedPerson.FirstName, SelectedPerson.LastName, Environment.NewLine), true))
                    {
                        setANETId(accessKey);
                    }
                }
            }
            else
            {
                provider.ShowMessage("MTF - error", "For assign ANET Id must selected user!!!", false);
            }
        }

        private void setANETId(string accessKey)
        {
            SelectedPerson.ANETId = accessKey;
            saveChanges();
        }
        
        private ObservableCollection<Person> person;
        public ObservableCollection<Person> Person
        {
            get => person;
            set 
            { 
                person = value;
                NotifyPropertyChanged();
            }
        }

        private Person selectedPerson;
        public Person SelectedPerson
        {
            get => selectedPerson;
            set 
            {
                if (selectedPerson == null)
                {
                    canDeletePerson = true;
                    canAddRole = true;
                    NotifyPropertyChanged("CanAddRole");
                    deleteSelectedPerson.RaiseCanExecuteChanged();
                    assignRole.RaiseCanExecuteChanged();
                }
                if (AllRoles.Count > 0)
                {
                    NewRole = AllRoles.First();
                }
                selectedPerson = value;
                NotifyPropertyChanged();
            }
        }

        private bool canAddRole = false;
        public bool CanAddRole
        {
            get => canAddRole;
            set 
            { 
                canAddRole = value;
                NotifyPropertyChanged();
            }
        }
  
        private Role newRole;
        public Role NewRole
        {
            get => newRole;
            set
            {
                newRole = value;
                NotifyPropertyChanged();
            }
        }

        private string selectedRole;
        public string SelectedRole
        {
            get => selectedRole;
            set 
            {
                selectedRole = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Role> allRoles;
        public ObservableCollection<Role> AllRoles
        {
            get => allRoles;
            set 
            { 
                allRoles = value;
                NotifyPropertyChanged();
            }
        }
   
        private void addNewPerson()
        {
            Person.Add(new Person() { Expiration = DateTime.Now.AddYears(1), Roles = new ObservableCollection<Role>() });
            SelectedPerson = Person.Last();
        }

        private void addNewRole()
        {
            AllRoles.Add(new Role() { IsModify = true }); 
        }

        private void assignNewRole()
        {
            if (selectedPerson != null && NewRole != null)
            {
                if (SelectedPerson.Roles.FirstOrDefault(x => x.Id == NewRole.Id) != null)
                {
                    provider.ShowMessage("MTF - error", "This role was already selected!!!", false);
                    return;
                }
                SelectedPerson.Roles.Add(NewRole);
                SelectedPerson.IsModify = true;
                NewRole = AllRoles.First();
            }
        }

        private void deletePerson()
        {
            if (SelectedPerson != null)
            {
                var deletePerson = Person.FirstOrDefault(x => x.Id == SelectedPerson.Id);
                if (deletePerson != null)
                {
                    Person.Remove(deletePerson);
                    canDeletePerson = false;
                    canAddRole = false;
                    NotifyPropertyChanged("CanAddRole");
                    deleteSelectedPerson.RaiseCanExecuteChanged();
                    assignRole.RaiseCanExecuteChanged();

                    if (Person.Count > 0)
                    {
                        Person.First().IsModify = true;
                    }
                }
            }
        }

        private void deleteRole(object param)
        {
            var role = param as Role;
            SelectedPerson.Roles.Remove(role);
            role.IsModify = true;
        }

        public void RenameRole(string roleName)
        {
            var role = AllRoles.FirstOrDefault(x => x.Name == roleName);
            if (role != null)
            {
                foreach (var person in Person)
                {
                    var renameRole = person.Roles.FirstOrDefault(x => x.Id == role.Id);
                    if (renameRole != null)
                    {
                        renameRole.Name = roleName;
                        person.IsModify = true;
                    }
                }
            }
        }

        private void saveChanges()
        {
            DataTypePerson actualData = new DataTypePerson();
            //actualData.Person = Person.ToList();
            actualData.Person = Person.AsEnumerable().Select(x => x.Clone() as Person).ToList();
            actualData.AllRoles = AllRoles.ToList();
            try
            {
                if (actualData.Person!=null)
                {
                    SecurityHelper.EncryptData(actualData.Person); 
                }
                XmlOperations.SaveXmlData(provider.DataFileName, actualData);
            }
            catch (Exception ex)
            {
                provider.ShowMessage("MTF - error", ex.Message, true);
            }
            isModifyAllPersonIsFalse();
        }

        private void deleteRoleFromAllRoles(object param)
        {
            var deleteRole = AllRoles.FirstOrDefault(x => x.Id == (param as Role).Id);
            if (deleteRole != null)
            {
                AllRoles.Remove(deleteRole);

                foreach (var person in Person)
                {
                    var role = person.Roles.FirstOrDefault(x => x.Id == deleteRole.Id);
                    if (role != null)
                    {
                        person.Roles.Remove(role);
                    }
                    person.IsModify = true;
                }
            }
        }

        private void isModifyAllPersonIsFalse()
        {
            foreach (var person in Person)
            {
                person.IsModify = false;
            }
        }

        public ICommand AddNewPersonCommand => new Command(addNewPerson);

        public ICommand AddNewRoleCommand => new Command(addNewRole);

        public ICommand DeletePersonCommand => deleteSelectedPerson;

        public ICommand DeleteRoleFromAllRolesCommand => new Command(deleteRoleFromAllRoles);

        public ICommand DeleteRoleCommand => new Command(deleteRole);

        public ICommand SaveCommand => new Command(saveChanges);

        public ICommand AssignRoleCommand => assignRole;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void OnClose()
        {
            provider.OpenConfigControl = false;
            provider.DataReceived -= provider_DataReceived;

            if (Person != null && AllRoles != null)
            {
                if (Person.FirstOrDefault(p => p.IsModify) != null || AllRoles.FirstOrDefault(r => r.IsModify) != null || Person.Count == 0)
                {
                    if (provider.ShowMessage("MTF", "Do you want to save all changes?", true))
                    {
                        saveChanges();
                    }
                }    
            }
        }
    }
}
