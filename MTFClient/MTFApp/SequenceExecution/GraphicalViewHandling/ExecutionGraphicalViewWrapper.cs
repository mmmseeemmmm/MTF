using System;
using System.Collections.Generic;

namespace MTFApp.SequenceExecution.GraphicalViewHandling
{
    public class ExecutionGraphicalViewWrapper
    {
        private string viewName;
        private List<ExecutionGraphicalViewTestedItem> testItems;
        private byte[] imgData;
        private Guid id;

        public string ViewName
        {
            get { return viewName; }
            set { viewName = value; }
        }

        public byte[] ImgData
        {
            get { return imgData; }
            set { imgData = value; }
        }

        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        public List<ExecutionGraphicalViewTestedItem> TestItems
        {
            get { return testItems; }
            set { testItems = value; }
        }
    }
}