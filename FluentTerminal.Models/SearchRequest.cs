namespace FluentTerminal.Models
{
    public class SearchRequest
    {
        public string Term { get; set; }
        public bool MatchCase { get; set; }
        public bool WholeWord { get; set; }
        public bool Regex { get; set; }
    }
}
