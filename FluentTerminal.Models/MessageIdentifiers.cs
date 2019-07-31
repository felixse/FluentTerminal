namespace FluentTerminal.Models
{
    public enum MessageIdentifiers : byte
    {
        CreateTerminalRequest = 1,
        GetAvailablePortRequest = 2,
        GetUserNameRequest = 3,
        ResizeTerminalRequest = 4,
        SaveTextFileRequest = 5,
        SetToggleWindowKeyBindingsRequest = 6,
        TerminalExitedRequest = 7,
        CommonResponse = 8,
        CreateTerminalResponse = 9,
        GetAvailablePortResponse = 10,
        StringValueResponse = 11,
        CheckFileExistsRequest = 12,
        GetSshConfigFolderRequest = 13,
        GetSshConfigFolderResponse = 14,
        MuteTerminalRequest = 15,
        UpdateSettingsRequest = 16,
        GetCommandPathRequest = 17,
    }
}