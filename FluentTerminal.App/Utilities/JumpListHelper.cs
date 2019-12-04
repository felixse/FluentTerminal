using System;
using Windows.UI.StartScreen;
using System.Threading.Tasks;
using FluentTerminal.App.Services;

namespace FluentTerminal.App.Utilities
{
    public static class JumpListHelper
    {
        public const string ShellProfileFlag = "JumpList-ShellProfile-";

        public static async Task UpdateAsync(ISettingsService settingsService)
        {
            if (!JumpList.IsSupported())
            {
                return;
            }

            try
            {
                var jumpList = await JumpList.LoadCurrentAsync();
                jumpList.Items.Clear();
                foreach (var profile in settingsService.GetAllProfiles())
                {
                    var item = JumpListItem.CreateWithArguments(ShellProfileFlag + profile.Id, profile.Name);
                    item.Description = profile.Location;
                    jumpList.Items.Add(item);
                }
                await jumpList.SaveAsync();
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e, "JumpList Update Exception");
            }
        }
    }
}
