using System;
namespace LightNlp.Tools.IO
{
    interface ILabeledTextDocumentReader
    {
        bool EndOfSource();
        LabeledTextDocument ReadDocument();
    }
}
