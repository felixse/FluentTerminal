using System;
using FluentTerminal.Models;
using Windows.UI.StartScreen;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentTerminal.App.Services;

namespace FluentTerminal.App.Utilities
{
    public static class JumpListHelper
    {
        public const string ShellProfileFlag = "JumpList-ShellProfile-";

        public static async Task Update(IEnumerable<ShellProfile> profiles)
        {
            try
            {
                if (JumpList.IsSupported())
                {
                    var jumpList = await JumpList.LoadCurrentAsync();
                    jumpList.Items.Clear();
                    foreach (var profile in profiles)
                    {
                        var item = JumpListItem.CreateWithArguments(ShellProfileFlag + profile.Id.ToString(), profile.Name);
                        item.Description = profile.Location;
                        jumpList.Items.Add(item);
                    }
                    await jumpList.SaveAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e, "JumpList Update Exception");
            }
        }
    }
}
