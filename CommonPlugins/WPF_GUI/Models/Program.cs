namespace WPF_GUI.Helpers
{
    public static class Program
    {
        public enum State : byte
        {
            Good = 0x00,
            Busy = 0x01,
            Error = 0x02
        }
    }
}