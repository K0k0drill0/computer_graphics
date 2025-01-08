using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

using Engine.Objects.Objects3D;
using Engine.Objects.Objects2D;
using Engine.Camera;
using Engine.Lights;
using Engine.ToolBar;
using ImGuiNET;

namespace Engine
{
    internal class Window : GameWindow
    {
        public static string FilePath => "../../../Data/CommonData.txt";

        Font font;
        HUD hud;
        Light light;
        public Player player;

        Cube cube;
        Pyramid pyramid;

        public bool needToSave = true;
        public bool needToLoad = true;
        public bool isRunning = false;

        GUI gui;
        Shadow shadow;
        BaseCamera camera;
        GameCamera gameCamera;
        EngineCamera engineCamera;
        bool firstMove = true;

        ObjectManager objectManager;
        bool isFocused = true;

        int width, height;

        internal Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            width = nativeWindowSettings.ClientSize.X;
            height = nativeWindowSettings.ClientSize.Y;

            hud = new HUD(nativeWindowSettings, this);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0.25f, 0.25f, 1f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            objectManager = new ObjectManager(height, width);
            objectManager.Registrate(typeof(Font),           Font.StaticName);
            objectManager.Registrate(typeof(Player),         Player.StaticName);
            objectManager.Registrate(typeof(DirectionLight), DirectionLight.StaticName);
            objectManager.Registrate(typeof(PointLight),     PointLight.StaticName);
            objectManager.Registrate(typeof(SpotLight),      SpotLight.StaticName);
            objectManager.Registrate(typeof(Cube),           Cube.StaticName);
            objectManager.Registrate(typeof(Pyramid),        Pyramid.StaticName);

            light  = new Light(objectManager);
            player = new Player(objectManager, this);

            engineCamera = new EngineCamera(Vector3.UnitZ * 2, Size.X, Size.Y);
            camera = engineCamera;
            CursorState = CursorState.Grabbed;

            gui = new GUI();
            gui.Create();
            shadow = new Shadow();
            gui.Set(shadow, 0);
            //gui.Set("../../../Textures/GUI/banana.png", 0);
            

            if (needToLoad)
            {
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

                using StreamReader reader = new(FilePath);
                string line;

                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null) break;

                    string[] settings = line.Split(' ');

                    switch (settings[0])
                    {
                        case "Cube":
                            if (!objectManager.Objects.ContainsKey("Cube"))
                            {
                                cube = new Cube(objectManager);
                            }
                            else if (objectManager.Objects["Cube"].Count <= int.Parse(settings[1]))
                                cube = new Cube(objectManager);

                            Cube.Load(objectManager.Objects["Cube"], int.Parse(settings[1]), ref settings);
                            break;

                        case "Pyramid":
                            if (!objectManager.Objects.ContainsKey("Pyramid"))
                            {
                                pyramid = new Pyramid(objectManager);
                            }
                            else if (objectManager.Objects["Pyramid"].Count <= int.Parse(settings[1]))
                                pyramid = new Pyramid(objectManager);

                            Cube.Load(objectManager.Objects["Pyramid"], int.Parse(settings[1]), ref settings);
                            break;

                        case "PointLight":
                            Light.Load(light, LightType.PointLight, ref settings);
                            break;

                        case "SpotLight":
                            Light.Load(light, LightType.SpotLight, ref settings);
                            break;

                        case "DirectionLight":
                            Light.Load(light, LightType.DirectionLight, ref settings);
                            break;

                        case "Player":
                            Player.Load(player, ref settings);
                            break;

                        case "Custom3D":
                            CustomObject customObject = CustomObject.Load(settings[2], objectManager, hud);
                            CustomObject.Load(customObject, ref settings);
                            break;

                        case "Font":
                            if (!objectManager.Objects.ContainsKey("Font"))
                            {
                                font = new Font(objectManager);
                                font.LoadFiles(Font.Settings, Font.Image);
                                font.Resize(width, height);
                            }

                            Font.Load(font, ref settings);
                            break;
                    }
                }
            }

            if (light.DirectionLight == null)
                light.CreateDirectionLight();

            gameCamera = new GameCamera(player, Vector3.UnitZ * 2, Size.X, Size.Y);

            KeyboardManager.Load();
            hud.Load(objectManager);
            hud.Load(gameCamera);
            hud.Load(engineCamera);
            hud.Load(light);
            hud.Load(player);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            light.GetShadows(LightType.DirectionLight, 0).Load();
            GL.Clear(ClearBufferMask.DepthBufferBit);
            objectManager.Draw(new DirectionLightCamera(camera, light.GetDirection(LightType.DirectionLight, 0)), light, isRunning);
            light.GetShadows(LightType.DirectionLight, 0).Unload();

            shadow.Load();
            GL.Clear(ClearBufferMask.DepthBufferBit);
            objectManager.Draw(new DirectionLightCamera(camera, light.GetDirection(LightType.DirectionLight, 0)), light, isRunning);
            shadow.Unload();

            objectManager.Draw(camera, light, isRunning);
            gui.Draw();

            hud.Render(ref needToSave, ref needToLoad, ref isFocused, ref isRunning, ref firstMove);

            SwapBuffers();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!isFocused && e.Button == MouseButton.Left && MousePosition.X > 400)
            {
                Vector2 mousePosition = new(MousePosition.X, MousePosition.Y);

                AABB.HandleMouseClick(mousePosition, camera, objectManager, hud, width, height);
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (KeyboardManager.ActiveName != null)
            {
                KeyboardManager.SetButton(KeyboardManager.ActiveName, e.Key);
                KeyboardManager.ActiveName = null;
            }

            if (objectManager.keyIsExpected)
            {
                objectManager.key = e.Key;
            }
            else
            {
                objectManager.key = Keys.F25;
            }

            if (e.Key == Keys.Escape)
            {
                isFocused = !isFocused;

                if (isFocused)
                {
                    CursorState = CursorState.Grabbed;
                    firstMove = true;
                }
                else
                {
                    if (isRunning) objectManager.Undo();
                    
                    isRunning = false;
                    camera = engineCamera;
                    gameCamera.firstFrame = true;
                    CursorState = CursorState.Normal;
                }
            }
            else if (e.Key == Keys.Delete) hud.Delete();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            hud.Update(this, (float)args.Time);

            if (!isRunning && isFocused)
            {
                var input = KeyboardState;
                var mouse = MouseState;
                camera.KeyboardInteractionControl(ref input, (float)args.Time);
                camera.MouseInteractionControl(ref mouse, ref firstMove);
            } 
            else if (isRunning)
            {

                isFocused = true;
                CursorState = CursorState.Grabbed;
                var input = KeyboardState;
                var mouse = MouseState;

                player.KeyboardInteractionControl(ref input, (float)args.Time);
                objectManager.Update((float)args.Time);

                camera = gameCamera;
                camera.KeyboardInteractionControl(ref input, (float)args.Time);
                camera.MouseInteractionControl(ref mouse, ref firstMove);
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

            height = e.Height;
            width = e.Width;

            objectManager.Resize(width, height);

            camera.Width = Size.X;
            camera.Height = Size.Y;

            hud.OnResize(e.Width, e.Height);
        }

        protected override void OnFileDrop(FileDropEventArgs e)
        {
            base.OnFileDrop(e);

            foreach (string path in e.FileNames)
            {
                if (path.Contains(".obj")) CustomObject.Load(path, objectManager, hud);
            }
        }

        protected override void OnUnload()
        {
            objectManager.Save();
            base.OnUnload();
        }

    }
}
