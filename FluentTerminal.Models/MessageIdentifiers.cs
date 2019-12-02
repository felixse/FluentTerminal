namespace FluentTerminal.Models
{
    public enum MessageIdentifiers : byte
    {
        WriteDataMessage = 0,
        CreateTerminalRequest = 1,
        GetUserNameRequest = 2,
        ResizeTerminalRequest = 3,
        SaveTextFileRequest = 4,
        SetToggleWindowKeyBindingsRequest = 5,
        TerminalExitedRequest = 6,
        CommonResponse = 7,
        CreateTerminalResponse = 8,
        StringValueResponse = 9,
        CheckFileExistsRequest = 10,
        GetSshConfigFolderRequest = 11,
        GetSshConfigFolderResponse = 12,
        MuteTerminalRequest = 13,
        UpdateSettingsRequest = 14,
        GetCommandPathRequest = 15,
        PauseTerminalOutputRequest = 16,
        PauseTerminalOutputResponse = 17,
        QuitApplicationRequest = 18
    }
}