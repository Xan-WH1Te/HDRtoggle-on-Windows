using System;
using System.Runtime.InteropServices;

class Program {
    [DllImport("user32.dll")]
    public static extern int GetDisplayConfigBufferSizes(uint flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

    [DllImport("user32.dll")]
    public static extern int QueryDisplayConfig(uint flags, ref uint numPathArrayElements, IntPtr pathInfoArray, ref uint numModeInfoArrayElements, IntPtr modeInfoArray, IntPtr currentTopologyId);

    [DllImport("user32.dll")]
    public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

    [DllImport("user32.dll")]
    public static extern int DisplayConfigSetDeviceInfo(ref DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE requestPacket);

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_DEVICE_INFO_HEADER {
        public uint type;
        public uint size;
        public LUID adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;
        public uint colorEncoding;
        public uint bitsPerColorChannel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint enableAdvancedColor; // 控制开关：1为开，0为关
    }

    static void Main() {
        uint QDC_ONLY_ACTIVE_PATHS = 2;
        uint pathCount;
        uint modeCount;
        
        GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);

        IntPtr paths = Marshal.AllocHGlobal((int)pathCount * 72);
        IntPtr modes = Marshal.AllocHGlobal((int)modeCount * 64);

        if (QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero) == 0) {
            for (int i = 0; i < pathCount; i++) {
                int offset = i * 72;
                LUID adapterId = new LUID();
                adapterId.LowPart = (uint)Marshal.ReadInt32(paths, offset + 20);
                adapterId.HighPart = Marshal.ReadInt32(paths, offset + 24);
                uint targetId = (uint)Marshal.ReadInt32(paths, offset + 28);

                DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO getInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                getInfo.header.type = 9; 
                getInfo.header.size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO));
                getInfo.header.adapterId = adapterId;
                getInfo.header.id = targetId;

                if (DisplayConfigGetDeviceInfo(ref getInfo) == 0) {
                    
                    bool isTrueHdrOn = (getInfo.value == 3);

                    DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE setInfo = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE();
                    setInfo.header.type = 10; 
                    setInfo.header.size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE));
                    setInfo.header.adapterId = adapterId;
                    setInfo.header.id = targetId;
                    
                    // 逻辑反转：如果是开(3)，发 0 关掉；如果是关(7 或 1)，发 1 打开
                    setInfo.enableAdvancedColor = isTrueHdrOn ? 0u : 1u;

                    DisplayConfigSetDeviceInfo(ref setInfo);
                }
            }
        }

        Marshal.FreeHGlobal(paths);
        Marshal.FreeHGlobal(modes);
    }
}
