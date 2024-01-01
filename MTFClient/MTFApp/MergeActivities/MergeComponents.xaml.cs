using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTFClientServerCommon;

namespace MTFApp.MergeActivities
{
    /// <summary>
    /// Interaction logic for MergeComponents.xaml
    /// </summary>
    public partial class MergeComponents : MergeActivitiesBase
    {
        private readonly IList<MTFSequenceClassInfo> classInfos;
        private readonly MergeSharedData mergedData;

        public MergeComponents(IList<MTFSequenceClassInfo> classInfos, MergeSharedData mergedData)
        {
            InitializeComponent();
            this.classInfos = classInfos;
            this.mergedData = mergedData;
            this.Root.DataContext = this;
            MergeComponentsAsync();
        }

        private async void MergeComponentsAsync()
        {
            var componentsMapping = new ObservableCollection<MergeActivitiesData<MTFSequenceClassInfo>>();
            await Task.Run(() =>
                           {
                               foreach (var component in mergedData.AllActivities.Where(a => a.ClassInfo != null).Select(a => a.ClassInfo).Distinct())
                               {
                                   var componentData = new MergeActivitiesData<MTFSequenceClassInfo>
                                   {
                                       OriginalData = component,
                                       AllowedData = classInfos.Where(c => c.MTFClass.AssemblyName == component.MTFClass.AssemblyName).ToList()
                                   };
                                   componentData.AllowedData.Add(new NewSequenceClassInfo());
                                   componentsMapping.Add(componentData);
                                   componentData.NewData = componentData.AllowedData.FirstOrDefault();
                               }
                           });
            mergedData.ComponentsMapping = componentsMapping;
            NotifyPropertyChanged("CurrentComponents");
        }


        public IEnumerable<MergeActivitiesData<MTFSequenceClassInfo>> CurrentComponents
        {
            get { return mergedData.ComponentsMapping; }
        }

        public override string Title
        {
            get { return "Merge components"; }
        }

        public override string Description
        {
            get { return "Select available components or create new."; }
        }

        public override bool IsFirstControl
        {
            get
            {
                return true;
            }
        }
    }

}
