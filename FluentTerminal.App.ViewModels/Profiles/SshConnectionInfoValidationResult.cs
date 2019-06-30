using System;

namespace FluentTerminal.App.ViewModels.Profiles
{
    [Flags]
    public enum SshConnectionInfoValidationResult
    {
        Valid = 0, 
        UsernameEmpty = 1,
        HostEmpty = 2,
        SshPortZeroOrNegative = 4,
        MoshPortZeroOrNegative = 8,
        MoshPortRangeInvalid = 16, 
        IdentityFileDoesNotExist = 32
    }
}
