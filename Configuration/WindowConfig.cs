namespace LogMonitor.Configuration
{
    public class WindowConfig
    {
        public enum WindowPosition { Center, Top, Bottom }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Monitor { get; private set; }
        public WindowPosition Position { get; private set; }

        public WindowConfig(int width, int height, int monitor, WindowPosition position)
        {
            Width = width;
            Height = height;
            Monitor = monitor;
            Position = position;
        }
    }
}
