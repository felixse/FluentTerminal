using System;
using FluentTerminal.Models;
using Windows.UI.StartScreen;
using System.Collections.Generic;

namespace FluentTerminal.App.Utilities
{
    public static class JumpListHelper
    {
        public static async void Update(IEnumerable<ShellProfile> profiles)
        {
            try
            {
                if (JumpList.IsSupported())
                {
                    var jumpList = await JumpList.LoadCurrentAsync();
                    jumpList.Items.Clear();
                    foreach (var profile in profiles)
                    {
                        var item = JumpListItem.CreateWithArguments("JumpList:" + profile.Id.ToString(), profile.Name);
                        item.Description = profile.Location;
                        item.Logo = new Uri("ms-appx:///Assets/AppIcons/StoreLogo.scale-100.png");
                        jumpList.Items.Add(item);
                    }
                    await jumpList.SaveAsync();
                }
            }catch (Exception){}
        }
    }
}
