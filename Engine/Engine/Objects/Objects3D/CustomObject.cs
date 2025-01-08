using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Engine.Lights;
using Engine.Shaders;
using Engine.Camera;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Engine.SystemInteraction;
using Engine.ToolBar;
using ImGuiNET;
using System;

namespace Engine.Objects.Objects3D
{
    internal class CustomObject(ObjectManager objectManager) : Object(objectManager)
    {
        private List<float> common;
        private Shader shader;
        private int vao, vbo;

        public int Index;
        public string Path;
        public static new string StaticName => "Custom3D";
        public override string Name => "Custom3D";
        public override int Count => 1;

        private float mass;
        private Vector3 position = new(0f), 
                        rotate = new(0f), 
                        scale = new(1f), 
                        color = new(0.5f), 
                        material = new(0.5f), 
                        velocity = new(0f);

        private Vector3 initPosition, initRotate;

        private bool physicsEnabled = false, 
                     running = false,
                     active = false;

        public static CustomObject Load(string filePath, ObjectManager objectManager, HUD hud)
        {
            List<float> tempCommon;

            //Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            tempCommon = [];

            var vertices = new List<float>();
            var normals = new List<float>();

            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(' ');

                if (parts.Length > 0)
                {
                    if (parts[0] == "v") vertices.AddRange(parts.Skip(1).Select(float.Parse));
                    else if (parts[0] == "vn") normals.AddRange(parts.Skip(1).Select(float.Parse));
                    else if (parts[0] == "f")
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            var vertexData = parts[i].Split('/');

                            int vertexIndex = int.Parse(vertexData[0]) - 1;
                            int normalIndex = int.Parse(vertexData[2]) - 1;

                            tempCommon.AddRange(vertices.GetRange(vertexIndex * 3, 3));
                            tempCommon.AddRange(normals.GetRange(normalIndex * 3, 3));
                        }
                    }
                }
            }

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CurrentCulture;

            if (tempCommon.Count > 0)
            {
                CustomObject customObject = new(objectManager)
                {
                    common = tempCommon,
                    Path = filePath,
                };
                customObject.Add(customObject.Name);

                customObject.Index = objectManager.Objects[customObject.Name].Count - 1;

                hud.FocusObj(customObject, 0);

                return customObject;
            }

            return null;
        }

        private void Init(Vector3 position, Vector3 rotate, Vector3 scale, Vector3 color, Vector3 velocity, float mass, Vector3 material, bool physicsEnabled)
        {
            this.position = position;
            this.rotate = rotate;
            this.scale = scale;
            
            this.color = color;
            this.material = material;

            this.velocity = velocity;
            this.mass = mass;
            this.physicsEnabled = physicsEnabled;
        }

        public override void Add(string name)
        {
            ManagerUpdate(this, Name);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            shader = new Shader("../../../Shaders/Custom/custom.vert", "../../../Shaders/common.frag");
            shader.Use();

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, common.Count * sizeof(float), common.ToArray(), BufferUsageHint.StaticDraw);

            var shaderPosition = shader.GetAttribLocation("aPos");
            GL.VertexAttribPointer(shaderPosition, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(shaderPosition);

            var shaderNormal = shader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(shaderNormal, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(shaderNormal);
        }

        public override void Undo()
        {
        }

        public override void Update(int idx, float deltaTime)
        {
        }

        public override void Delete(int idx)
            => objectManager.Objects[Name].RemoveAt(idx);

        public override void Draw(BaseCamera camera, Light light, bool isPhysicsWorld)
        {
            GL.BindVertexArray(vao);
            shader.Use();

            if (light != null) Lights.ToolBar.AddLight(shader, light, camera);

            shader.SetVector3("color", color);
            shader.SetVector3("material", material);

            shader.SetVector3("viewPos", camera.Position);
            shader.SetMatrix4("model", Matrix4.Identity * Matrix4.CreateTranslation(position));
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, common.Count / 6);
        }

        public override Vector3 GetPosition(int idx) => position;
        public override Vector3 GetRotate(int idx) => rotate;
        public override Vector3 GetScale(int idx) => scale;

        public override Vector3 GetColor(int idx) => color;
        public Vector3 GetMaterial() => material;

        public override bool GetActive(int idx) => active;
        public override bool GetPhysicsEnabled(int idx) => physicsEnabled;
        public override void ChangePhysicsEnabled(int idx) => physicsEnabled = !physicsEnabled;

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
                    throw new Exception("Check model of SetModel call");
            }
        }

        public override void ShowNativeDataDialog(ref int idx)
        {
            System.Numerics.Vector3 curColor = Transform.ToSystemNumerics(GetColor(idx));
            System.Numerics.Vector3 curTranslate = Transform.ToSystemNumerics(GetPosition(idx));
            System.Numerics.Vector3 curRotate = Transform.ToSystemNumerics(GetRotate(idx));
            System.Numerics.Vector3 curScale = Transform.ToSystemNumerics(GetScale(idx));
            System.Numerics.Vector3 curMaterial = Transform.ToSystemNumerics(GetMaterial());
            bool curPE = GetPhysicsEnabled(idx);

            ImGui.Text(Name + " " + Index);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            GUI_Helper.Vec3(this, Model.Position, "Translate", ref curTranslate, idx, 0.01f);
            GUI_Helper.Vec3(this, Model.Rotate, "Rotate", ref curRotate, idx, 0.01f);
            GUI_Helper.Vec3(this, Model.Scale, "Scale", ref curScale, idx, 0.01f, 0.1f, 100f, "%.3f");
            ImGui.Dummy(new System.Numerics.Vector2(0, 10));
            GUI_Helper.Vec3(this, Model.Material, "Material", ref curMaterial, idx, 0.005f, 0f, 1f, "%.3f");

            if (ImGui.ColorEdit3("Color", ref curColor))
                SetModel(Model.Color, idx, Transform.ToOpenTK(curColor));

            //if (ImGui.Button("Choose Texture"))
            //{
            //    string newPath = NativeFileDialog.OpenFileDialog();
            //    if (newPath is not null)
            //        AddTexture(newPath);
            //}

            //ImGui.SameLine();

            //string briefTexturePath = TexturePath.Length > 20 ? string.Concat("...", TexturePath.AsSpan(TexturePath.Length - 20, 20)) : TexturePath;
            //ImGui.Text(briefTexturePath);


            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.Checkbox("PhysicsEnabled", ref curPE))
                ChangePhysicsEnabled(idx);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.Button("Delete", new System.Numerics.Vector2(100f, 50f)))
            {
                Delete(idx);
                idx = -1;
            }
        }

        public override void Save(StreamWriter writer)
        {
            writer.Write(Name + " " + Index + " " + Path);

            ToolBar.Stream.WriteVector(writer, position);
            ToolBar.Stream.WriteVector(writer, scale);
            ToolBar.Stream.WriteVector(writer, rotate);

            ToolBar.Stream.WriteVector(writer, color);
            ToolBar.Stream.WriteVector(writer, material);

            ToolBar.Stream.WriteVector(writer, velocity);
            writer.Write(" " + mass);
            writer.Write(" " + physicsEnabled);

            writer.WriteLine();
        }

        public static void Load (CustomObject custom, ref string[] settings)
        {
            int start = 3;

            ToolBar.Stream.ReadVector(out Vector3 position, ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 scale,    ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 rotate,   ref start, ref settings);
            
            ToolBar.Stream.ReadVector(out Vector3 color,    ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 material, ref start, ref settings);
            
            ToolBar.Stream.ReadVector(out Vector3 velocity, ref start, ref settings);
            float mass = float.Parse(settings[start++]);
            bool physicsEnabled = bool.Parse(settings[start++]);

            custom.Init(position, rotate, scale, color, velocity, mass, material, physicsEnabled);
        }
    }

}
