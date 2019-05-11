namespace FluentTerminal.Models.Enums
{
    public enum SshConnectionInfoValidationResult
    {
        UsernameEmpty,
        HostEmpty,
        SshPortZeroOrNegative,
        MoshPortZeroOrNegative,
        MoshPortRangeInvalid,
        Valid
    }
}
