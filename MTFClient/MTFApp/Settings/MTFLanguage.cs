using AutomotiveLighting.MTFCommon;

namespace MTFApp.Settings
{
    public class MTFLanguage
    {
        private string key;
        private string name;
        private MTFIcons icon;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public MTFIcons Icon
        {
            get { return icon; }
            set { icon = value; }
        }
    }
}
