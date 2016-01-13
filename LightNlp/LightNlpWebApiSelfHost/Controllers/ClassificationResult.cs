namespace LightNlpWebApiSelfHost.Controllers
{
    public class ClassificationResult
    {
        public double Confidence { get; internal set; }
        public double Label { get; internal set; }
        public string LabelName { get; internal set; }
        public string Text { get; internal set; }
    }
}