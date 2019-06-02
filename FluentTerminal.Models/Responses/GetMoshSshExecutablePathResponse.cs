namespace FluentTerminal.Models.Responses
{
    public class GetMoshSshExecutablePathResponse : CommonResponse
    {
        public bool IsMosh { get; set; }

        public string Path { get; set; }
    }
}