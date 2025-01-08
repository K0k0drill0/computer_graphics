using OpenTK.Mathematics;
using Engine.ToolBar;
using Engine.Textures;
using Engine.Shaders;
using OpenTK.Graphics.OpenGL4;
using Engine.Objects;
using Engine.Camera;

namespace Engine.Objects.Objects2D
{
    internal class GUI
    {
        public static new string StaticName => "GUI";
        public string Name => "GUI";
        public int Count { get; private set; }

        List <Matrix4> models;

        List <Vector3> positions;
        List <Vector3> rotates;
        List <Vector3> scales;

        List<string?>  texPaths;
        List<Shadow?>  textures;

        int VAO, VBO;
        Shader shader;
        Shadow texture;

        public GUI ()
        {
            models = [];

            positions = [];
            scales = [];
            rotates = [];

            texPaths = [];
            textures = [];

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, CommonData.GUIPlane.Length * sizeof(float), CommonData.GUIPlane, BufferUsageHint.StaticDraw);

            shader = new Shader("../../../Shaders/GUI/index.vert", "../../../Shaders/GUI/index.frag");
            shader.Use();

            shader.BindAttribLocation("aPos", 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            shader.BindAttribLocation("aTexCoord", 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        }

        public void Create()
        {
            positions.Add(new(0.8f, 0.81f, 0f));
            rotates.Add(new(0f));
            scales.Add(new(0.2f, 0.2f, 0f));
            models.Add(Common.CalculateModel(positions[Count], scales[Count], rotates[Count]));

            texPaths.Add(null);
            textures.Add(null);
            Count++;
        }

        public void Set(Object.Model model, Vector3 value, int idx)
        {
            switch (model)
            {
                case Object.Model.Position:
                    positions[idx] = value;
                    break;

                case Object.Model.Scale:
                    scales[idx] = value;
                    break;

                case Object.Model.Rotate:
                    rotates[idx] = value;
                    break;

                default:
                    throw new Exception("Check modelType in GUI Set call");
            }
        }

        //public void Set(string value, int idx)
        //{
        //    texPaths[idx] = value;
        //    textures[idx] = Texture.LoadFromFile(value);
        //}

        public void Set(Shadow value, int idx)
        {
            textures[idx] = value;
        }

        public void Draw()
        {
            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(VAO);
            shader.Use();

            for (int i = 0; i < Count; i++)
            {
                if (textures[i] == null) continue;

                texture = textures[i];

                shader.SetInt("shadow", texture.Use());
                shader.SetMatrix4("model", models[i]);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            }

            GL.Enable(EnableCap.DepthTest);
        }
    }
}
