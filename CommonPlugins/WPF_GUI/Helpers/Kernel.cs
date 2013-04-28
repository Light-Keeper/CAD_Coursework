using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WPF_GUI.Helpers
{
    public static class Kernel
    {
        public const UInt32 StateEmpty = 0x00000000;
        public const UInt32 StatePlace = 0x00000001;
        public const UInt32 StateTrace = 0x00000002;
        public const UInt32 StatePlacing = 0x00000003;
        public const UInt32 StateTracing = 0x00000004;

        [DllImport("GUI_CLR_loader.dll", EntryPoint = "GetKernelState")]
        public static extern UInt32 GetState();

        [DllImport("GUI_CLR_loader.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadFile(StringBuilder path);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern bool StartPlaceModule(string moduleName, bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern bool StartTraceModule(string moduleName, bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern UInt32 NextStep(bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern int GetModuleList(int bufferSize, StringBuilder charBuffer);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern void RenderPicture(IntPtr hWnd, UInt32 x, UInt32 y, double scale,
            bool renderNewPicture, bool renderLayer, UInt32 layer);
    }
}
