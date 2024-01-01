using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardErrorMemoryResult
    {
        public int CountAll
        {
            get
            {
                return errorCodesAll.Count;
            }

        }

        public string ErrorCodesAll
        {
            get
            {
                return getValuesTogether(errorCodesAll);
            }
        }

        public string ErrorTextsAll
        {
            get
            {
                return getValuesTogether(errorTextsAll);
            }
        }

        public int CountUnignored
        {
            get
            {
                return errorCodesUnignored.Count;
            }

        }

        public string ErrorCodesUnignored
        {
            get
            {
                return getValuesTogether(errorCodesUnignored);
            }
        }

        public string ErrorTextsUnignored
        {
            get
            {
                return getValuesTogether(errorTextsUnignored);
            }
        }

        public int CountIgnored
        {
            get
            {
                return errorCodesAll.Count - errorCodesUnignored.Count;
            }

        }

        public string ErrorCodesIgnored
        {
            get
            {
                return getValuesTogether(errorCodesAll.Except(errorCodesUnignored).ToList());
            }
        }

        public string ErrorTextsIgnored
        {
            get
            {
                return getValuesTogether(errorTextsAll.Except(errorTextsUnignored).ToList());
            }
        }

        public void SetErrorCodesAll(List<string> values)
        {
            errorCodesAll = values;
        }

        public void AddErrorCodesUnignored(string value)
        {
            errorCodesUnignored.Add(value);
        }

        public void SetErrorTextsAll(List<string> values)
        {
            errorTextsAll = values;
        }

        public void AddErrorTextsUnignored(string value)
        {
            errorTextsUnignored.Add(value);
        }

        private List<string> errorCodesAll = new List<string>();
        private List<string> errorCodesUnignored = new List<string>();
        private List<string> errorCodesIgnored = new List<string>();
        private List<string> errorTextsAll = new List<string>();
        private List<string> errorTextsUnignored = new List<string>();
        private List<string> errorTextsIgnored = new List<string>();

        private string getValuesTogether(List<string> values)
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
}
