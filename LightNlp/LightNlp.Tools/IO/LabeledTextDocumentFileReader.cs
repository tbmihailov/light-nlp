using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightNlp.Tools.IO
{
    public class LabeledTextDocumentFileReader : IDisposable, LightNlp.Tools.IO.ILabeledTextDocumentReader
    {
        private string _filePath = string.Empty;
        private StreamReader _reader;
        private char _fieldSeparator = '\t';
        Encoding _encoding = Encoding.UTF8;

        public LabeledTextDocumentFileReader(string tabSeparatedDocumentFilePath):this(tabSeparatedDocumentFilePath, '\t', Encoding.UTF8)
        {
        }

        public LabeledTextDocumentFileReader(string tabSeparatedDocumentFilePath, char fieldSeparator, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(tabSeparatedDocumentFilePath))
            {
                throw new ArgumentNullException("tabSeparatedDocumentFilePath");
            }

            _filePath = tabSeparatedDocumentFilePath;
            _fieldSeparator = fieldSeparator;
            _encoding = encoding;

            _reader = new StreamReader(_filePath, encoding);
            
        }

        public bool EndOfSource()
        {
            return _reader.EndOfStream;
        }

        public LabeledTextDocument ReadDocument()
        {
            string line = _reader.ReadLine();
            var fields = line.Split(new char[] { _fieldSeparator });

            //class label and doc contents
            string classLabel = fields[0];
            string docContent = fields[1];

            var document = new LabeledTextDocument(classLabel, docContent);
            return document;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_reader != null)
                {
                    _reader.Dispose();
                }
            }
        }

        ~LabeledTextDocumentFileReader()
        {
            Dispose(false);
        }
    }
}
