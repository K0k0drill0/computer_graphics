using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Engine.Shaders;
using Engine.Camera;
using Engine.Lights;
using Engine.ToolBar;

using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Engine.SystemInteraction;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Engine.Objects;

namespace Engine
{
    internal class Player : Object
    {
        public static new string StaticName => "Player";
        public override string Name => "Player";
        public override int Count => 1;

        Window window;

        int VBO, VAO;
        Shader shader;
        Matrix4 model;

        protected Vector3 position,     rotate,     velocity, scale;
        private   Vector3 initPosition, initRotate;
        protected float mass;

        protected Vector3 color, material;

        protected bool physicsEnabled;
        private bool runnable;
        private bool active;

        private string UserClassPath = "Empty";

        public Player(ObjectManager objectManager, Window window) : base(objectManager)
        {
            runnable = false;

            ManagerUpdate(this, Name);
            this.window = window;

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, CommonData.CubeVertices.Length * sizeof(float), CommonData.CubeVertices, BufferUsageHint.DynamicDraw);

            shader = new Shader("../../../Shaders/User/common.vert", "../../../Shaders/common.frag");
            shader.Use();

            shader.BindAttribLocation("aPos",      3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            shader.BindAttribLocation("aNormal",   3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            shader.BindAttribLocation("aTexCoord", 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        }

        public override void Add(string name)
        {
            position = new(0f, 5f, 0f);
            rotate = new(0f);
            scale = new(1f);

            color = new(0.5f);
            material = new(0.45f, 0.8f, 0.5f);

            active = false;
            physicsEnabled = false;
            velocity = new(0f);
            mass = 1f;
        }

        public void Init(Vector3 pos, Vector3 rot, Vector3 scl, Vector3 clr, Vector3 mtrl, Vector3 vlct, float mass, bool physicsEnabled)
        {
            position = pos;
            rotate = rot;
            scale = scl;

            color = clr;
            material = mtrl;

            this.physicsEnabled = physicsEnabled;
            velocity = vlct;
            this.mass = mass;
        }

        public override void Draw(BaseCamera camera, Light light, bool isPhysicsWorld)
        {
            GL.BindVertexArray(VAO);
            shader.Use();

            if (light != null) Lights.ToolBar.AddLight(shader, light, camera);

            model = Common.CalculateModel(position, scale, rotate);

            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            shader.SetVector3("material", material);
            shader.SetVector3("color", color);

            shader.SetVector3("viewPos", camera.Position);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        public override Vector3 GetColor(int idx = 0) => color;

        public override Vector3 GetPosition(int idx = 0) => position;
        public override Vector3 GetRotate(int idx = 0)   => rotate;
        public override Vector3 GetScale(int idx = 0)    => scale;

        public Vector3 GetVelocity() => velocity;

        public override bool GetActive(int idx = 0) => active;
        public override bool GetPhysicsEnabled(int idx = 0)    => physicsEnabled;
        public override void ChangePhysicsEnabled(int idx = 0) => physicsEnabled = !physicsEnabled;
        public override Vector3 GetVertex(int objectIdx, int vertexIdx)
            => AABB.GetCubeCorner(position, scale, CommonData.CubeCornerns[vertexIdx]);

        public override void SetModel(Model model, int idx, Vector3 value)
        {
            switch (model)
            {
                case Model.Position:
                    position = value;
                    break;

                case Model.Rotate:
                    rotate = value;
                    break;

                case Model.Scale:
                    scale = value;
                    break;

                case Model.Color:
                    color = value;
                    break;

                case Model.Material:
                    material = value;
                    break;

                case Model.Velocity:
                    velocity = value;
                    break;

                default:
                    throw new Exception("Check model type of SetModel call");
            }
        }

        public override sealed void ShowNativeDataDialog(ref int idx) 
        {
            var sysVecPosition = Transform.ToSystemNumerics(position);
            var sysVecRotate   = Transform.ToSystemNumerics(rotate);
            var sysVecScale    = Transform.ToSystemNumerics(scale);
            var sysVecColor    = Transform.ToSystemNumerics(color);
            var sysVecMaterial = Transform.ToSystemNumerics(material);

            GUI_Helper.Vec3(ref position, "uPosition", ref sysVecPosition, 0.01f);
            GUI_Helper.Vec3(ref rotate,   "uRotate",   ref sysVecRotate,   0.01f);
            GUI_Helper.Vec3(ref scale,    "uScale",    ref sysVecScale,    0.01f, 0.01f, 100f, "%.3f");
            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            ImGui.DragFloat("uMass", ref mass, 0.01f, 0.01f, 1_000_000f, "%.3f");
            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.ColorEdit3("uColor", ref sysVecColor)) SetModel(Model.Color, idx, Transform.ToOpenTK(sysVecColor));
            GUI_Helper.Vec3(ref material, "uMaterial", ref sysVecMaterial, 0.005f, 0.05f, 1f, "%.3f");
            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            ImGui.Checkbox("uPhysicsEnabled", ref physicsEnabled);
            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.Button("Choose Own Class"))
            {
                string newPath = NativeFileDialog.OpenFileDialog("D:\\LABS\\5sem\\comp_graphics\\Engine\\Engine\\Engine\\UserClass", "CSharp Files (*.cs)|*.cs");
                if (newPath is not null)
                {
                    UserClassPath = newPath;
                    int classNameIndex = UserClassPath.IndexOf("UserClass");

                    if (classNameIndex != -1)
                    {
                        string className = UserClassPath.Substring(classNameIndex, UserClassPath.Length - classNameIndex).Replace('\\', '.');
                        string text = File.ReadAllText(UserClassPath);

                        Player? userPlayer = LoadPlayerFromScript(text, string.Concat("Engine.", className.AsSpan(0, className.Length - 3)));
                        if (userPlayer != null)
                        {
                            userPlayer.Init(position, rotate, scale, color, material, velocity, mass, physicsEnabled);
                            window.player = userPlayer;
                        }
                    }
                }
            }

            ImGui.SameLine();

            string briefTexturePath = UserClassPath.Length > 20 ? string.Concat("...", UserClassPath.AsSpan(UserClassPath.Length - 20, 20)) : UserClassPath;
            ImGui.Text(briefTexturePath);
        }

        public virtual void KeyboardInteractionControl(ref KeyboardState input, float deltaTime)
        {
            if (input.IsKeyDown(KeyboardManager.GetButton("User Front")))
                position += Vector3.UnitZ * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Back")))
                position -= Vector3.UnitZ * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Left")))
                position -= Vector3.UnitX * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Right")))
                position += Vector3.UnitX * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Up")))
                position += Vector3.UnitY * deltaTime;

        }

        public override void Update(int idx, float deltaTime)
        {
            if (!runnable)
            {
                initPosition = position;
                initRotate = rotate;
                runnable = true;
            }

            position += velocity * deltaTime;
        }

        public sealed override void Undo()
        {
            if (!physicsEnabled) return;

            position = initPosition;
            rotate   = initRotate;
            velocity = Vector3.Zero;
            runnable = false;
        }
        public override void Delete(int idx = 0) { }

        public override void Save(StreamWriter writer)
        {
            writer.Write(Name);
            ToolBar.Stream.WriteVector(writer, position);
            ToolBar.Stream.WriteVector(writer, scale);
            ToolBar.Stream.WriteVector(writer, rotate);
            ToolBar.Stream.WriteVector(writer, color);
            ToolBar.Stream.WriteVector(writer, velocity);
            writer.Write(" " + mass);
            ToolBar.Stream.WriteVector(writer, material);
            writer.Write(" " + physicsEnabled);

            writer.WriteLine();
        }

        public static void Load(Player player, ref string[] settings)
        {
            int start = 1;

            ToolBar.Stream.ReadVector(out Vector3 position, ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 scale,    ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 rotate,   ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 color,    ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 velocity, ref start, ref settings);
            float mass = float.Parse(settings[start++]);

            ToolBar.Stream.ReadVector(out Vector3 material, ref start, ref settings);

            bool physicsEnabled = bool.Parse(settings[start++]);

            player.Init(position, rotate, scale, color, material, velocity, mass, physicsEnabled);

            string text = File.ReadAllText("D:\\LABS\\5sem\\comp_graphics\\Engine\\Engine\\Engine\\UserClass\\UPlayer.cs");
            Player? userPlayer = player.LoadPlayerFromScript(text, "Engine.UserClass.UPlayer");
            if (userPlayer != null)
            {
                userPlayer.Init(position, rotate, scale, color, material, velocity, mass, physicsEnabled);
                player.window.player = userPlayer;
            }
        }
        
        public Player? LoadPlayerFromScript(string script, string className)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(script);
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(KeyboardState).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Keys).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Vector3).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Player).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(AABB).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ToolBar.Stream).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ObjectsInstance).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Window).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ObjectManager).Assembly.Location),
                MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\8.0.11\\System.Runtime.dll"),
                MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\8.0.11\\System.Linq.dll"),
                MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\8.0.11\\System.Collections.dll"),
            };

            var compilation = CSharpCompilation.Create("UPlayer")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTree);

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                        Console.WriteLine($"Error: {diagnostic.GetMessage()}");
                    return null;
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());
                var type = assembly.GetType(className);

                if (type != null && typeof(Player).IsAssignableFrom(type))
                {
                    objectManager.Objects.Remove(Name);
                    return (Player)Activator.CreateInstance(type, objectManager, window);
                }
            }

            return null;
        }
    }
}
