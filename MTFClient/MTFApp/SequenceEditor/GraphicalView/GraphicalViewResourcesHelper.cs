using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public class GraphicalViewResourcesHelper
    {
        private static GraphicalViewResourcesHelper instance;
        private readonly MTFClient mtfClient = MTFClient.GetMTFClient();
        private ObservableCollection<GraphicalViewImg> imageCollection;
        private readonly object lockObject = new object();

        public static GraphicalViewResourcesHelper Instance
        {
            get { return instance ?? (instance = new GraphicalViewResourcesHelper()); }
        }

        private GraphicalViewResourcesHelper()
        {
        }

        public Task<ObservableCollection<GraphicalViewImg>> GetResources()
        {
            return imageCollection == null ? Task.Run(() => LoadImagesAsync()) : Task.FromResult(imageCollection);
        }

        private ObservableCollection<GraphicalViewImg> LoadImagesAsync()
        {
            lock (lockObject)
            {
                if (imageCollection != null)
                {
                    return imageCollection;
                }

                var tmpCollection = new ObservableCollection<GraphicalViewImg>();
                var data = UIHelper.HandleWcfCallWithErrorMsg(() => mtfClient.LoadAllGraphicalViewImages(),
                    LanguageHelper.GetString("Editor_GraphicalView_ResourceManager_LoadError"));

                if (data != null)
                {
                    foreach (var img in data.OrderBy(x => x.Name))
                    {
                        tmpCollection.Add(img);
                    }
                }
                imageCollection = tmpCollection;
                return imageCollection;
            }
        }

        public void Reload()
        {
            imageCollection = null;
        }
    }
}