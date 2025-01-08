using Engine.Camera;
using Engine.Lights;
using Engine.Objects;
using Engine.Objects.Objects3D;
using Engine.Shaders;
using Engine.Textures;
using Engine.ToolBar;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Engine.Objects.Objects2D
{
    public struct FontCharacter
    {
        public char Character;
        public int Width;
        public int Height;
        public int XOffset;
        public int YOffset;
        public int X;
        public int Y;
        public int XAdvance;
    }

    internal class Font : FlatObjects
    {
        public static new string StaticName => "Font";
        public override string Name => "Font";
        public override int Count => models.Count;

        public Dictionary<char, FontCharacter> Characters = [];
        private static readonly char[] separator = [' '];

        private int vao, vbo;
        private Shader? shader = null;
        private Texture texture;
        public int textureWidth = 512, textureHeight = 512;

        private List<string> texts = [];

        private List<Vector3> rotates = [];
        private List<Vector3> scales  = [];
        private List<Matrix4> models  = [];

        private List<Vector3> colors = [];

        private List<bool> active = [];

        private List<KeyValuePair<Vector2, Vector2>> corners = [];

        public static readonly string Settings = "../../../Fonts/latinText.fnt";
        public static readonly string Image = "../../../Fonts/latinText.png";

        public Font(ObjectManager objectManager) : base(objectManager)
        {
            ManagerUpdate(this, Name);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            shader = new("../../../Shaders/Font/index.vert", "../../../Shaders/Font/index.frag");
            shader.BindAttribLocation("aPos", 2, VertexAttribPointerType.Float, false, 16, 0); // Pos
            shader.BindAttribLocation("aTexCoord", 2, VertexAttribPointerType.Float, false, 16, 8); // Tex
        }

        public override void Add(string text)
        {
            texts.Add(text);

            colors.Add(new(1f));

            {
                float width = 0;
                float height = 45 / windowHeight;

                foreach (char c in text)
                {
                    if (Characters.TryGetValue(c, out FontCharacter character))
                        width += 25;
                }

                width /= windowWidth;

                corners.Add(new KeyValuePair<Vector2, Vector2>(new(-width / 2, height / 2), new(width / 2, -height / 2)));
            }

            rotates.Add(new(0f));
            scales.Add(new(0.001f, 0.001f, 0f));
            active.Add(false);

            Vector2 textCenter = corners[Count].Key;

            models.Add(Common.CalculateModel(new(textCenter.X, textCenter.Y, 0f), scales[Count], rotates[Count]));
        }

        public void Init(string text, Vector3 rotate, Vector3 scale, Vector3 color, KeyValuePair<Vector2, Vector2> corners)
        {
            texts.Add(text);
            rotates.Add(rotate);
            scales.Add(scale);
            colors.Add(color);
            this.corners.Add(corners);
            active.Add(false);

            Vector2 textCenter = corners.Key;
            models.Add(Common.CalculateModel(new(textCenter.X, textCenter.Y, 0f), scale, rotate));
        }

        public override void LoadFiles(string filename, string texPath)
        {
            texture = Texture.LoadFromFile(texPath);

            using StreamReader reader = new(filename);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("info") || line.StartsWith("common") || line.StartsWith("page") || line.StartsWith("chars"))
                    continue;

                if (line.StartsWith("char"))
                {
                    var parts = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    char character = (char)int.Parse(parts[1].Split('=')[1]);

                    int x = int.Parse(parts[2].Split('=')[1]);
                    int y = int.Parse(parts[3].Split('=')[1]);

                    int width  = int.Parse(parts[4].Split('=')[1]);
                    int height = int.Parse(parts[5].Split('=')[1]);

                    int xoffset = int.Parse(parts[6].Split('=')[1]);
                    int yoffset = int.Parse(parts[7].Split('=')[1]);

                    int xAdvance = int.Parse(parts[8].Split('=')[1]);

                    Characters[character] = new FontCharacter
                    {
                        Character = character,
                        X = x,
                        Y = y,
                        Width    = width,
                        Height   = height,
                        XOffset  = xoffset,
                        YOffset  = yoffset,
                        XAdvance = xAdvance
                    };
                }
            }
        }

        public void Resize(float width, float height)
        {
            windowWidth = width;
            windowHeight = height;
        }

        public override void Draw(BaseCamera camera, Light light, bool isPhysicsWorld)
        {

            for (int i = 0; i < Count; i++)
            {
                string text = texts[i];

                float[] vertices = new float[text.Length * 24];

                int index = 0;

                Vector2 position = corners[i].Key;

                foreach (char c in text)
                {
                    if (Characters.TryGetValue(c, out FontCharacter character))
                    {
                        float texX = (float)character.X / textureWidth;
                        float texY = (float)(textureHeight - character.Y) / textureHeight;
                        float texWidth = (float)character.Width / textureWidth;
                        float texHeight = (float)character.Height / textureHeight;

                        float drawX = position.X + character.XOffset;
                        float drawY = position.Y - character.YOffset;

                        vertices[index++] = drawX;
                        vertices[index++] = drawY;
                        vertices[index++] = texX;
                        vertices[index++] = texY;

                        vertices[index++] = drawX + character.Width;
                        vertices[index++] = drawY - character.Height;
                        vertices[index++] = texX + texWidth;
                        vertices[index++] = texY - texHeight;

                        vertices[index++] = drawX + character.Width;
                        vertices[index++] = drawY;
                        vertices[index++] = texX + texWidth;
                        vertices[index++] = texY;

                        // Second Triangle
                        vertices[index++] = drawX;
                        vertices[index++] = drawY;
                        vertices[index++] = texX;
                        vertices[index++] = texY;

                        vertices[index++] = drawX;
                        vertices[index++] = drawY - character.Height;
                        vertices[index++] = texX;
                        vertices[index++] = texY - texHeight;

                        vertices[index++] = drawX + character.Width;
                        vertices[index++] = drawY - character.Height;
                        vertices[index++] = texX + texWidth;
                        vertices[index++] = texY - texHeight;

                        position.X += character.XAdvance;
                    }
                }

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                GL.BindVertexArray(vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                shader.Use();
                texture.Use(TextureUnit.Texture0, Texture.Type.FLAT);
                shader.SetInt("tex", 0);
                shader.SetMatrix4("model", models[i]);
                shader.SetVector3("color", colors[i]);

                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / 4);
            }

        }

        public override void Delete(int idx)
        {
            texts.RemoveAt(idx);
            rotates.RemoveAt(idx);
            scales.RemoveAt(idx);
            models.RemoveAt(idx);
            colors.RemoveAt(idx);
            corners.RemoveAt(idx);
        }

        public override Vector3 GetVertex(int objectIdx, int vertexIdx)
        {
            Vector2 tempCorner;

            if (vertexIdx == 0)
            {
                tempCorner = corners[objectIdx].Key;
                return new(tempCorner.X, tempCorner.Y, 0);
            }

            tempCorner = corners[objectIdx].Value;
            return new(tempCorner.X, tempCorner.Y, 0);
        }

        public override bool GetActive(int idx) => active[idx];

        public override Vector3 GetColor(int idx) => colors[idx];

        public override Vector3 GetPosition(int idx)
            => new((corners[idx].Key.X + corners[idx].Value.X) / 2, (corners[idx].Key.Y + corners[idx].Value.Y) / 2, 0f);
        public override Vector3 GetRotate(int idx) => rotates[idx];
        public override Vector3 GetScale(int idx) => scales[idx];

        public override void SetModel(Model model, int idx, Vector3 value)
        {
            if (Count > idx)
            {
                switch (model)
                {
                    case Model.Position:
                        float width = Math.Abs(corners[idx].Value.X - corners[idx].Key.X);
                        float height = Math.Abs(corners[idx].Value.Y - corners[idx].Key.Y);

                        corners[idx] = new((-width / 2 + value.X, height / 2 + value.Y), new(width / 2 + value.X, -height / 2 + value.Y));
                        break;

                    case Model.Rotate:
                        rotates[idx] = value;
                        break;

                    case Model.Scale:
                        scales[idx] = value;
                        break;

                    case Model.Color:
                        colors[idx] = value;
                        break;

                    default:
                        throw new Exception("Check model of SetModel call");
                }

                if (model == Model.Rotate || model == Model.Position || model == Model.Scale)
                {
                    Vector2 textCenter = corners[idx].Key;
                    models[idx] = Common.CalculateModel(new(textCenter.X, textCenter.Y, 0f), scales[idx], rotates[idx]);
                }
            }
            else
                throw new Exception("Check idx in GetTranslate call");
        }

        private void SetText(int idx, string str) => texts[idx] = str;

        public override void ShowNativeDataDialog(ref int idx)
        {
            System.Numerics.Vector3 curColor = Transform.ToSystemNumerics(GetColor(idx));
            System.Numerics.Vector3 curTranslate = Transform.ToSystemNumerics(GetPosition(idx));
            System.Numerics.Vector3 curRotate = Transform.ToSystemNumerics(GetRotate(idx));
            System.Numerics.Vector3 curScale = Transform.ToSystemNumerics(GetScale(idx));
            string text = texts[idx];
            Console.WriteLine(text);

            ImGui.Text(Name + " " + Index);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            ImGui.InputText("Text", ref text, 32);

            if (ImGui.IsItemActive())
            {
                objectManager.keyIsExpected = true;
                text += objectManager.key.ToString();
                SetText(idx, text);
            }

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            GUI_Helper.Vec3(this, Model.Position, "Translate", ref curTranslate, idx, 0.01f);
            GUI_Helper.Vec3(this, Model.Rotate, "Rotate", ref curRotate, idx, 0.01f);
            GUI_Helper.Vec3(this, Model.Scale, "Scale", ref curScale, idx, 0.005f, 0.001f, 100f, "%.3f");
            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.ColorEdit3("Color", ref curColor))
                SetModel(Model.Color, idx, Transform.ToOpenTK(curColor));

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.Button("Delete", new System.Numerics.Vector2(100f, 50f)))
            {
                if (Count == 1)
                {
                    objectManager.Remove(Name, Index);
                    idx = -1;
                    return;
                }

                Delete(idx);
                idx -= 1;
            }
        }

        public override void Save(StreamWriter writer)
        {
            for (int i = 0; i < Count; i++)
            {
                writer.Write(Name + " " + Index + " " + texts[i]);

                ToolBar.Stream.WriteVector(writer, rotates[i]);
                ToolBar.Stream.WriteVector(writer, scales[i]);
                ToolBar.Stream.WriteVector(writer, colors[i]);
                ToolBar.Stream.WriteVector(writer, corners[i].Key);
                ToolBar.Stream.WriteVector(writer, corners[i].Value);

                writer.WriteLine();
            }
        }

        public static void Load(Font font, ref string[] settings)
        {
            int start = 2;

            string text = settings[start++];

            ToolBar.Stream.ReadVector(out Vector3 rotate,   ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 scale,    ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector3 color,    ref start, ref settings);

            ToolBar.Stream.ReadVector(out Vector2 leftTopCorner,     ref start, ref settings);
            ToolBar.Stream.ReadVector(out Vector2 rightBottomCorner, ref start, ref settings);

            KeyValuePair<Vector2, Vector2> corners = new(leftTopCorner, rightBottomCorner);

            font.Init(text, rotate, scale, color, corners);
        }
    }
}
