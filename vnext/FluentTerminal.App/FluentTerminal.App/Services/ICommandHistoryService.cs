using System;
using System.Collections.Generic;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface ICommandHistoryService
    {
        /// <summary>
        /// Returns the most recent <paramref name="top"/> commands from history.
        /// </summary>
        /// <param name="includeProfiles">Indicates if never used saved profiles should also be returned.</param>
        /// <param name="top">Maximal number of items to return.</param>
        /// <param name="profilesProvider">List of all saved profiles. If this argument isn't specified, the method will
        /// get the list from <see cref="ISettingsService"/>. The purpose of this argument is to avoid loading the
        /// profiles from settings again (since it includes file system access and JSON deserialization), if we
        /// already have them loaded.</param>
        List<ExecutedCommand> GetHistoryRecentFirst(bool includeProfiles = false, int top = int.MaxValue,
            Func<List<ShellProfile>> profilesProvider = null);

        /// <summary>
        /// Returns the most used <paramref name="top"/> commands from history.
        /// </summary>
        /// <param name="includeProfiles">Indicates if never used saved profiles should also be returned.</param>
        /// <param name="top">Maximal number of items to return.</param>
        /// <param name="profilesProvider">List of all saved profiles. If this argument isn't specified, the method will
        /// get the list from <see cref="ISettingsService"/>. The purpose of this argument is to avoid loading the
        /// profiles from settings again (since it includes file system access and JSON deserialization), if we
        /// already have them loaded.</param>
        List<ExecutedCommand> GetHistoryMostUsedFirst(bool includeProfiles = false, int top = int.MaxValue,
            Func<List<ShellProfile>> profilesProvider = null);

        void MarkUsed(ShellProfile profile);

        void Delete(ExecutedCommand executedCommand);
    }
}