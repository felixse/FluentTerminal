namespace FluentTerminal.Models.Enums
{
    // Mirrors Windows.ApplicationModel.StartupTaskState
    public enum StartupTaskStatus
    {
        Disabled = 0,
        DisabledByUser = 1,
        Enabled = 2,
        DisabledByPolicy = 3,
        EnabledByPolicy = 4
    }
}
