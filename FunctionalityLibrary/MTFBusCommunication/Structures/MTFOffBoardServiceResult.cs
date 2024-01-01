using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardServiceResult: MTFOffBoardResponseSetting
    {
        public int Count
        {
            get { return values.Count; }
        }

        public string Values_Together
        {
            get
            {
                string result = string.Empty;
                if (values != null)
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        result = result + (i + 1).ToString() + ": " + values[i] + " - ";
                    }

                }
                return result;
            }
        }

        private List<string> values = new List<string>();

        public List<string> Values
        {
            get { return values; }
            set { values = value; }
        }
    }
}
