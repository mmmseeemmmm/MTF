using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFCore
{
    class MTFComponentConfigContext : IMTFComponentConfigContext
    {
        public string MessageBoxChoise(string header, string text, IList<string> items)
        {
            return MessageBoxChoiseMethod(header, text, items);
        }

        public Func<string, string, IList<string>, string> MessageBoxChoiseMethod
        {
            get;
            set;
        }

        public string MessageBoxMultiChoise(string header, string text, IList<string> items)
        {
            return MessageBoxMultiChoiseMethod(header, text, items);
        }

        public Func<string, string, IList<string>, string> MessageBoxMultiChoiseMethod
        {
            get;
            set;
        }

        public string MessageBoxListBox(string header, string text, IList<string> items)
        {
            return MessageBoxListBoxMethod(header, text, items);
        }

        public Func<string, string, IList<string>, string> MessageBoxListBoxMethod
        {
            get;
            set;
        }
    }
}
