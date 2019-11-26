using System;
using Windows.System;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Services
{
    public class AcceleratorKeyValidator : IAcceleratorKeyValidator
    {
        public bool Valid(int key) => Enum.IsDefined(typeof(VirtualKey), key);

        public bool Valid(ExtendedVirtualKey key) => Valid((int) key);
    }
}