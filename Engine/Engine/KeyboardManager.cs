using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;

namespace Engine
{
    internal static class KeyboardManager
    {
        public static Dictionary<string, Keys> Buttons { get; private set; }
        public static string? ActiveName { get; set; }

        public static void AddButton(string name, Keys key)
        {
            Buttons.Add(name, key);
            ActiveName = null;
        }

        public static void Load()
        {
            Buttons = [];
            AddButton("Engine camera Up", Keys.Space);
            AddButton("Engine camera Down", Keys.LeftShift);
            AddButton("Engine camera Left", Keys.A);
            AddButton("Engine camera Right", Keys.D);
            AddButton("Engine camera Front", Keys.W);
            AddButton("Engine camera Back", Keys.S);

            AddButton("Game camera Up", Keys.PageUp);
            AddButton("Game camera Down", Keys.PageDown);
            AddButton("Game camera Left", Keys.Left);
            AddButton("Game camera Right", Keys.Right);
            AddButton("Game camera Front", Keys.Up);
            AddButton("Game camera Back", Keys.Down);

            AddButton("User Up", Keys.Space);
            AddButton("User Left", Keys.A);
            AddButton("User Right", Keys.D);
            AddButton("User Front", Keys.W);
            AddButton("User Back", Keys.S);
        }

        public static Keys GetButton(string name)
        {
            if (!Buttons.TryGetValue(name, out Keys value))
            {
                Console.WriteLine("KeyboardManager: Check Button");
                return Keys.Z;
            }

            return value;
        }
        public static void SetButton(string name, Keys key) => Buttons[name] = key;
    }
}
