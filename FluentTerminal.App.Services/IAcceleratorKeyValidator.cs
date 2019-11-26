using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.Services
{
    // This validator is needed because accelerator key has to be member of VirtualKey enum.
    public interface IAcceleratorKeyValidator
    {
        bool Valid(int key);

        bool Valid(ExtendedVirtualKey key);
    }
}