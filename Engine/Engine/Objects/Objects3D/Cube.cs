using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Engine.SystemInteraction;
using Engine.Lights;
using Engine.Shaders;
using Engine.ToolBar;
//using Engine.Textures;
using Engine.Camera;
using Engine.Textures;

namespace Engine.Objects.Objects3D
{
    internal class Cube : ObjectsInstance
    {
        public override string Name => "Cube";
        public static new string StaticName => "Cube";

        int VBO, VAO, Models_VBO, Colors_VBO, Materials_VBO;
        private bool registered = false;
        Shader shader;
        //Texture texture;

        public Cube(ObjectManager objectManager) : base(objectManager)
        {
            ManagerUpdate(this, Name);

            VAO           = GL.GenVertexArray();
            VBO           = GL.GenBuffer();
            Models_VBO    = GL.GenBuffer();
            Colors_VBO    = GL.GenBuffer();
            Materials_VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, CommonData.CubeVertices.Length * sizeof(float), CommonData.CubeVertices, BufferUsageHint.DynamicDraw);

            shader = new Shader("../../../Shaders/Cube/common.vert", "../../../Shaders/common.frag");

            shader.BindAttribLocation("aPos",      3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            shader.BindAttribLocation("aNormal",   3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            shader.BindAttribLocation("aTexCoord", 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        }

        public void ChangeIndex(int idx) => Index = idx;

        public void Create(Vector3 position, Vector3 rotate, Vector3 scale, float[] color,
            Vector3 velocity, float mass, float[] material, bool physicsEnable, string texturePath)
            => Add(Name, position, rotate, scale, color, velocity, mass, material, physicsEnable, texturePath);

        public void Create() => Add(Name);

        private void AfterCreate()
        {
            Register(VAO, Models_VBO, Models, 16);
            var instPos = shader.GetAttribLocation("aModelRow1");

            for (int i = 0; i < 4; i++)
            {
                GL.VertexAttribPointer(instPos + i, 4, VertexAttribPointerType.Float, false, 16 * sizeof(float), i * Vector4.SizeInBytes);
                GL.EnableVertexAttribArray(instPos + i);
                GL.VertexAttribDivisor(instPos + i, 1);
            }

            Register(VAO, Colors_VBO, Colors, 3);
            shader.BindAttribLocation("aColor", 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0, true);

            Register(VAO, Materials_VBO, Materials, 3);
            shader.BindAttribLocation("aMaterial", 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0, true);
        }

        public override void SetModel(Model model, int idx, Vector3 value)
        {
            base.SetModel(model, idx, value);
            
            if (model == Model.Color)
                Register(VAO, Colors_VBO, Colors, 3, false, true, idx);
            else if (model == Model.Material)
                Register(VAO, Materials_VBO, Materials, 3, false, true, idx);
            else
                Register(VAO, Models_VBO, Models, 16, false, true, idx);
        }

        public override void Delete(int idx)
        {
            base.Delete(idx);
            GlobalUpdate();
        }

        private void GlobalUpdate()
        {
            if (!registered)
            {
                Register(VAO, Models_VBO, Models, 16);
                var instPos = shader.GetAttribLocation("aModelRow1");

                for (int i = 0; i < 4; i++)
                {
                    GL.VertexAttribPointer(instPos + i, 4, VertexAttribPointerType.Float, false, 16 * sizeof(float), i * Vector4.SizeInBytes);
                    GL.EnableVertexAttribArray(instPos + i);
                    GL.VertexAttribDivisor(instPos + i, 1);
                }

                Register(VAO, Colors_VBO, Colors, 3);
                shader.BindAttribLocation("aColor", 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0, true);

                Register(VAO, Materials_VBO, Materials, 3);
                shader.BindAttribLocation("aMaterial", 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0, true);
            } 
            else
            {
                Register(VAO, Models_VBO, Models, 16);
                Register(VAO, Colors_VBO, Colors, 3);
                Register(VAO, Materials_VBO, Materials, 3);
            }

        }

        public override void Draw(BaseCamera camera, Light light, bool isPhysicsWorld = false)
        {
            if (Count == 0) return;

            if (NeedToUpdate) GlobalUpdate();

            if (isPhysicsWorld) Register(VAO, Models_VBO, Models, 16, false);

            GL.BindVertexArray(VAO);
            shader.Use();

            shader.SetBool("texIsAdded", texture is not null);
            texture?.Use(TextureUnit.Texture0, Texture.Type.FLAT);

            if (light != null) Lights.ToolBar.AddLight(shader, light, camera);

            shader.SetVector3("viewPos", camera.Position);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, Count);
        }

        public void Bind(int idx)
        {
            ImGui.Text("Bundle");

            ImguiUpdateAxisHelper("X",  ref idx,  Vector3.UnitX);
            ImguiUpdateAxisHelper("-X", ref idx, -Vector3.UnitX);
            ImguiUpdateAxisHelper("Y",  ref idx,  Vector3.UnitY);
            ImguiUpdateAxisHelper("-Y", ref idx, -Vector3.UnitY);
            ImguiUpdateAxisHelper("Z",  ref idx,  Vector3.UnitZ);
            ImguiUpdateAxisHelper("-Z", ref idx, -Vector3.UnitZ);
        }

        public void ImguiUpdateAxisHelper(string label, ref int idx, Vector3 additionalVector)
        {
            if (ImGui.Button(label, new System.Numerics.Vector2(25, 25)))
                SetModel(Model.Position, idx, ObjectData[idx - 1].Position + additionalVector);
            if (label != "-Z") ImGui.SameLine();
        }

        public override void ShowNativeDataDialog(ref int idx)
        {
            System.Numerics.Vector3 curColor     = Transform.ToSystemNumerics(GetColor(idx));
            System.Numerics.Vector3 curTranslate = Transform.ToSystemNumerics(GetPosition(idx));
            System.Numerics.Vector3 curRotate    = Transform.ToSystemNumerics(GetRotate(idx));
            System.Numerics.Vector3 curScale     = Transform.ToSystemNumerics(GetScale(idx));
            System.Numerics.Vector3 curMaterial  = Transform.ToSystemNumerics(GetMaterial(idx));
            bool curPE = GetPhysicsEnabled(idx);

            ImGui.Text(Name + " " + Index);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            GUI_Helper.Vec3(this, Model.Position, "Translate", ref curTranslate, idx, 0.01f);
            GUI_Helper.Vec3(this, Model.Rotate,   "Rotate",    ref curRotate,    idx, 0.01f);
            GUI_Helper.Vec3(this, Model.Scale,    "Scale",     ref curScale,     idx, 0.01f,  0.1f, 100f, "%.3f");
            ImGui.Dummy(new System.Numerics.Vector2(0, 10));
            GUI_Helper.Vec3(this, Model.Material, "Material",  ref curMaterial,  idx, 0.005f, 0f,   1f,   "%.3f");

            if (ImGui.ColorEdit3("Color", ref curColor))
                SetModel(Model.Color, idx, Transform.ToOpenTK(curColor));

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.Button(TexturePath.Equals("Empty") ? "Choose Texture" : "Change Texture"))
            {
                string newPath = NativeFileDialog.OpenFileDialog();
                if (newPath is not null)
                    AddTexture(newPath);
            }

            ImGui.SameLine();

            if (!TexturePath.Equals("Empty"))
            {
                if (ImGui.Button("x"))
                {
                    TexturePath = "Empty";
                    texture = null;
                }
            }

            ImGui.SameLine();

            string briefTexturePath = TexturePath.Length > 14 ? string.Concat("...", TexturePath.AsSpan(TexturePath.Length - 14, 14)) : TexturePath;
            ImGui.Text(briefTexturePath);


            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (idx > 0) Bind(idx);

            ImGui.Text("Create");
            ImguiCreateAxisHelper("+X",  ref idx,  Vector3.UnitX);
            ImguiCreateAxisHelper("--X", ref idx, -Vector3.UnitX);
            ImguiCreateAxisHelper("+Y",  ref idx,  Vector3.UnitY);
            ImguiCreateAxisHelper("--Y", ref idx, -Vector3.UnitY);
            ImguiCreateAxisHelper("+Z",  ref idx,  Vector3.UnitZ);
            ImguiCreateAxisHelper("--Z", ref idx, -Vector3.UnitZ);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.Checkbox("PhysicsEnabled", ref curPE))
                ChangePhysicsEnabled(idx);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.Button("Delete", new System.Numerics.Vector2(100f, 50f)))
                ImguiDelete(ref idx);
        }

        public void ImguiCreateAxisHelper(string label, ref int idx, Vector3 additionalVector)
        {
            if (ImGui.Button(label, new System.Numerics.Vector2(35, 25)))
            {
                Create();
                SetModel(Model.Position, Count - 1, ObjectData[idx].Position + additionalVector);
                SetModel(Model.Color, Count - 1, new Vector3(Colors[idx * 3], Colors[idx * 3 + 1], Colors[idx * 3 + 2]));
                idx = Count - 1;
            }
            if (label != "--Z") ImGui.SameLine();
        }

        public void ImguiDelete(ref int idx)
        {
            if (Count == 1 && Index >= 0)
            {
                objectManager.Remove(Name, Index);
                idx = -1;
                return;
            }
            
            Delete(idx);
            idx -= 1;
        }

        public override void Save(StreamWriter writer)
        {
            base.Save(writer);
        }

        public static void Load(List<Object> cubes, int index, ref string[] settings)
        {
            int start = 2;

            ToolBar.Stream.ReadVector(out Vector3 position, ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 scale, ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 rotate, ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 color, ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 velocity, ref start, ref settings);
            float mass = float.Parse(settings[start++]);

            ToolBar.Stream.ReadVector(out Vector3 material, ref start, ref settings);

            bool physicsEnabled = bool.Parse(settings[start++]);
            
            string texturePath = settings[start++];

            if (cubes[index] is Cube cube)
                cube.Create(position, rotate, scale, [color.X, color.Y, color.Z], velocity, mass, [material.X, material.Y, material.Z], physicsEnabled, texturePath);
        }
    }
}
