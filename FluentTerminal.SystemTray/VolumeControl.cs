using System;
using System.Runtime.InteropServices;

namespace FluentTerminal.SystemTray
{
    public static class VolumeControl
    {
        public static bool? GetAudioSessionMute(int processId)
        {
            ISimpleAudioVolume volume = GetAudioVolume(processId);
            if (volume == null)
                return null;

            volume.GetMute(out var mute);
            Marshal.ReleaseComObject(volume);
            return mute;
        }

        public static void SetAudioSessionMute(int processId, bool mute)
        {
            ISimpleAudioVolume volume = GetAudioVolume(processId);
            if (volume == null)
                return;

            Guid guid = Guid.Empty;
            volume.SetMute(mute, ref guid);
            Marshal.ReleaseComObject(volume);
        }

        public static bool? GetDefaultAudioEndpointMute()
        {
            IMMDevice speakers = GetDefaultDevice();

            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
            speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out var result);
            IAudioEndpointVolume audioEndpoint = (IAudioEndpointVolume)result;
            if (audioEndpoint == null)
                return null;

            audioEndpoint.GetMute(out var mute);
            Marshal.ReleaseComObject(audioEndpoint);
            return mute;
        }

        public static void SetDefaultAudioEndpointMute(bool mute)
        {
            IMMDevice speakers = GetDefaultDevice();

            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
            speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out var result);
            IAudioEndpointVolume audioEndpoint = (IAudioEndpointVolume)result;
            if (audioEndpoint == null)
                return;

            Guid guid = Guid.Empty;
            audioEndpoint.SetMute(mute, ref guid);
            Marshal.ReleaseComObject(audioEndpoint);
        }

        private static IMMDevice GetDefaultDevice()
        {
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
            Marshal.ReleaseComObject(deviceEnumerator);
            return device;
        }

        private static ISimpleAudioVolume GetAudioVolume(int pid)
        {
            IMMDevice device = GetDefaultDevice();
            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            device.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out var result);
            IAudioSessionManager2 sessionManager = (IAudioSessionManager2)result;

            sessionManager.GetSessionEnumerator(out var sessionEnumerator);
            sessionEnumerator.GetCount(out var count);

            ISimpleAudioVolume volumeControl = null;
            for (int i = 0; i < count; i++)
            {
                sessionEnumerator.GetSession(i, out var sessionControl2);
                sessionControl2.GetProcessId(out var processId);

                if (processId == pid)
                {
                    volumeControl = sessionControl2 as ISimpleAudioVolume;
                    break;
                }
                Marshal.ReleaseComObject(sessionControl2);
            }
            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(sessionManager);
            Marshal.ReleaseComObject(device);
            return volumeControl;
        }
    }

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator
    {
    }

    internal enum EDataFlow
    {
        eRender,
        eCapture,
        eAll,
        EDataFlow_enum_count
    }

    internal enum ERole
    {
        eConsole,
        eMultimedia,
        eCommunications,
        ERole_enum_count
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        int NoImp1();
        int NoImp2();
        int NoImp3();
        int NoImp4();
        int NoImp5();
        int NoImp6();
        int NoImp7();
        int NoImp8();
        int NoImp9();
        int NoImp10();
        int NoImp11();

        [PreserveSig]
        int SetMute([MarshalAs(UnmanagedType.Bool)] Boolean bMute, ref Guid pguidEventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        int NoImp1();

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int clsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        int NoImp1();
        int NoImp2();

        [PreserveSig]
        int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig]
        int GetCount(out int SessionCount);

        [PreserveSig]
        int GetSession(int SessionCount, out IAudioSessionControl2 Session);
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fLevel, ref Guid EventContext);

        [PreserveSig]
        int GetMasterVolume(out float pfLevel);

        [PreserveSig]
        int SetMute(bool bMute, ref Guid EventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);
    }

    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl2
    {
        // IAudioSessionControl
        int NoImp0();
        int NoImp1();
        int NoImp2();
        int NoImp3();
        int NoImp4();
        int NoImp5();
        int NoImp6();
        int NoImp7();
        int NoImp8();

        // IAudioSessionControl2
        [PreserveSig]
        int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

        [PreserveSig]
        int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

        [PreserveSig]
        int GetProcessId(out int pRetVal);

        [PreserveSig]
        int IsSystemSoundsSession();

        [PreserveSig]
        int SetDuckingPreference(bool optOut);
    }
}
