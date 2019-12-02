namespace FluentTerminal.Models
{
    public class TerminalSize
    {
        public int Columns { get; set; }

        public int Rows { get; set; }

        public bool EquivalentTo(TerminalSize other) => ReferenceEquals(this, other) ||
                                                        other != null && Columns.Equals(other.Columns) &&
                                                        Rows.Equals(other.Rows);
    }
}