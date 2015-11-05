using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightNlp.Tools.IO
{
    public class LabeledTextDocument
    {
        private string classLabel;

        public string ClassLabel
        {
            get { return classLabel; }
            set { classLabel = value; }
        }
        private string docContent;

        public string DocContent
        {
            get { return docContent; }
            set { docContent = value; }
        }

        public LabeledTextDocument(string classLabel, string docContent)
        {
            this.ClassLabel = classLabel;
            this.DocContent = docContent;
        }
    }
}
