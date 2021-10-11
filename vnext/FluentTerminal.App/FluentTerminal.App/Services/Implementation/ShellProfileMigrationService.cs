using FluentTerminal.Models;
using System;
using System.Collections.Generic;

namespace FluentTerminal.App.Services.Implementation
{
    public class ShellProfileMigrationService : IShellProfileMigrationService
    {
        private readonly Dictionary<int, Action<ShellProfile>> _migrationSteps = new Dictionary<int, Action<ShellProfile>>
        {
            [1] = new Action<ShellProfile>(profile =>
            {
                profile.EnvironmentVariables["TERM"] = "xterm-256color";
            })
        };


        public void Migrate(ShellProfile profile)
        {
            while(profile.MigrationVersion < ShellProfile.CurrentMigrationVersion)
            {
                ApplyMigrationStep(profile, profile.MigrationVersion + 1);
            }
        }

        private void ApplyMigrationStep(ShellProfile profile, int targetVersion)
        {
            _migrationSteps[targetVersion](profile);
            profile.MigrationVersion = targetVersion;
        }
    }
}
