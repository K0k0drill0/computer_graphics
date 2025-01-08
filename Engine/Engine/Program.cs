using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;

namespace Engine
{
    internal class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                WindowState = WindowState.Normal,
                ClientSize = new Vector2i(1920, 1080),
                Title = "Engine",
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run();
            }
        }
    }
}