using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;
using MTFClientServerCommon;

namespace MTFApp.Managers.Components
{
    class ComponentsManager : ManagerBase
    {
        private ObservableCollection<MTFClassCategory> mtfClassCategories = new ObservableCollection<MTFClassCategory>();
        public override void Init()
        {
            base.Init();

            mtfClassCategories.Clear();

            ServiceClientsContainer.Get<ComponentsClient>().MonsterClassLoaded += UpdateMtfClassCategories;
        }

        public ObservableCollection<MTFClassCategory> MTFClassCategories
        {
            get
            {
                Task.Run(() => UpdateMtfClassCategories());
                return mtfClassCategories;
            }
        }

        private void UpdateMtfClassCategories()
        {
            IEnumerable<MTFClassCategory> categories = ServiceClientsContainer.Get<ComponentsClient>().MTFClassCategories;

            if (mtfClassCategories.Count == 0)
            {
                foreach (var category in categories)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => mtfClassCategories.Add(category));
                }

                return;
            }

            foreach (var category in categories)
            {
                var actualCategory = mtfClassCategories.FirstOrDefault(c => c.Id == category.Id);
                if (actualCategory == null)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => mtfClassCategories.Add(category));
                }
                else
                {
                    UpdateMtfCategory(actualCategory, category);
                }
            }
        }

        private void UpdateMtfCategory(MTFClassCategory actualCategory, MTFClassCategory newCategory)
        {
            foreach (var mtfClass in newCategory.Classes)
            {
                if (actualCategory.Classes.All(c => c.Id != mtfClass.Id))
                {
                    Application.Current.Dispatcher.InvokeAsync(() => actualCategory.Classes.Add(mtfClass));
                }
            }
        }
    }
}
