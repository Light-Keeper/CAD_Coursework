using System;

namespace WPF_GUI.Helpers
{
    public static class Defines
    {
        public const UInt32 KernelStateEmpty = 0x00000000;
        public const UInt32 KernelStatePlace = 0x00000001;
        public const UInt32 KernelStateTrace = 0x00000002;
        public const UInt32 KernelStatePlacing = 0x00000003;
        public const UInt32 KernelStateTracing = 0x00000004;

        public const string ProgramName = "Program name";

        public const int ProgramStateGood = 0;
        public const int ProgramStateBusy = 1;
        public const int ProgramStateError = 2;

        public const string ConsoleButtonNameWhenOpened = "Скрыть консоль";
        public const string ConsoleButtonNameWhenClosed = "Показать консоль";
    }
}