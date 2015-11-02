namespace LightNlp.Core.Modules
{
    public class Annotation
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
    }
}