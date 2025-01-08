using Engine.ToolBar;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Engine.Textures;

namespace Engine.Objects
{
    internal abstract class ObjectsInstance(ObjectManager objectManager) : Object(objectManager)
    {
        public struct Data
        {
            public int Index { get; private set; }

            public bool PhysicsEnable { get; set; }
            public Vector3[] Corners { get; set; }

            public Vector3 Position { get; set; }
            public Vector3 InitPosition { get; set; }
            public Vector3 Rotate { get; set; }
            public Vector3 InitRotate { get; set; }
            public Vector3 Scale { get; set; }

            public Vector3 InitVelocity { get; set; }
            public Vector3 Velocity { get; set; }
            public float Mass { get; set; }

            public bool Active { get; set; }
            public bool Runnable { get; set; }

            public Data(Vector3 position, Vector3 rotate, Vector3 scale, Vector3 velocity, float mass, Vector3[] corners, bool physicsEnable, int idx)
            {
                Index = idx;

                PhysicsEnable = physicsEnable;
                Corners = corners;

                Position = position;
                Rotate = rotate;
                Scale = scale;

                Velocity = velocity;
                Mass = mass;

                Runnable = false;
                Active = false;
            }
        }

        public override int Count => ObjectData.Count;
        protected bool NeedToUpdate { get; set; } = false;
        public string TexturePath { get; set; } = "Empty";
        
        protected Texture texture;

        public List<Data> ObjectData { get; private set; } = [];
        public List<float> Models { get; private set; }    = [];
        public List<float> Colors { get; private set; }    = [];
        public List<float> Materials { get; private set; } = [];

        protected void AddTexture(string texturePath)
        {
            TexturePath = texturePath;
            try
            {
                texture = Texture.LoadFromFile(texturePath);
                texture.Use(TextureUnit.Texture0, Texture.Type.FLAT);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public override void Add(string name)
        {
            var corners = defineCorners(name);
            NeedToUpdate = true;

            ObjectData.Add(new Data(new Vector3(0f), new Vector3(0f), new Vector3(1f), new Vector3(0f), 1f, corners, false, Count));
            Colors.AddRange([0.5f, 0.5f, 0.5f]);
            Materials.AddRange([0.45f, 0.8f, 1f]);

            UpdateModel(ObjectData.Count - 1, new Vector3(0f), new Vector3(0f), new Vector3(1f));
        }

        protected void Add(string name, Vector3 position, Vector3 rotate, Vector3 scale, float[] color, 
            Vector3 velocity, float mass, float[] material, bool physicsEnable, string texturePath)
        {
            var corners = defineCorners(name);
            NeedToUpdate = true;

            ObjectData.Add(new Data(position, rotate, scale, velocity, mass, corners, physicsEnable, Count));

            Colors.AddRange(color);
            Materials.AddRange(material);
            UpdateModel(ObjectData.Count - 1, position, rotate, scale);

            if (!texturePath.Equals(TexturePath))
            {
                AddTexture(texturePath);
            }
        }

        protected void UpdateModel(int idx, Vector3 position, Vector3 rotate, Vector3 scale)
        {
            float[] array = new float[16];
            Common.CalculateModel(ref array, position, scale, rotate);

            Data temp = ObjectData[idx];
            ObjectData[idx] = temp;

            if (idx < Models.Count / 16)
            {
                Models.RemoveRange(idx * 16, 16);
                Models.InsertRange(idx * 16, array);
            }
            else Models.AddRange(array);
        }

        public override void Delete(int idx)
        {
            ObjectData.RemoveAt(idx);
            Colors.RemoveRange(idx * 3, 3);
            Models.RemoveRange(idx * 16, 16);
            Materials.RemoveRange(idx * 3, 3);
        }

        public override bool GetActive(int idx) => ObjectData[idx].Active;
        public override Vector3 GetColor(int idx) => (Colors[idx * 3], Colors[idx * 3 + 1], Colors[idx * 3 + 2]);

        public override Vector3 GetPosition(int idx) => ObjectData[idx].Position;
        public override Vector3 GetRotate(int idx) => ObjectData[idx].Rotate;
        public override Vector3 GetScale(int idx) => ObjectData[idx].Scale;
        public override bool GetPhysicsEnabled(int idx) => ObjectData[idx].PhysicsEnable;
        public override void ChangePhysicsEnabled(int idx)
        {
            Data temp = ObjectData[idx];
            temp.PhysicsEnable = !temp.PhysicsEnable;
            ObjectData[idx] = temp;
        }

        public Vector3 GetMaterial(int idx) => new (Materials[idx * 3], Materials[idx * 3 + 1], Materials[idx * 3 + 2]);
        public Vector3 GetVelocity(int idx) => ObjectData[idx].Velocity;

        public override void SetModel(Model model, int idx, Vector3 value)
        {
            if (Count > idx)
            {
                Data temp = ObjectData[idx];

                switch (model)
                {
                    case Model.Position:
                        temp.Position = value;
                        ObjectData[idx] = temp;
                        break;

                    case Model.Rotate:
                        temp.Rotate = value;
                        ObjectData[idx] = temp;
                        break;

                    case Model.Scale:
                        temp.Scale = value;
                        ObjectData[idx] = temp;
                        break;

                    case Model.Color:
                        Colors[idx * 3] = value.X;
                        Colors[idx * 3 + 1] = value.Y;
                        Colors[idx * 3 + 2] = value.Z;
                        break;

                    case Model.Material:
                        Materials[idx * 3] = value.X;
                        Materials[idx * 3 + 1] = value.Y;
                        Materials[idx * 3 + 2] = value.Z;
                        break;

                    case Model.Velocity:
                        temp.Velocity = value;
                        ObjectData[idx] = temp;
                        break;


                    default:
                        throw new Exception("Check model of SetModel call");
                }

                if (model == Model.Rotate || model == Model.Position || model == Model.Scale)
                    UpdateModel(idx, ObjectData[idx].Position, ObjectData[idx].Rotate, ObjectData[idx].Scale);
            }
            else
                throw new Exception("Check idx in GetTranslate call");
        }

        private Vector3[] defineCorners(string name)
        {
            return name switch
            {
                "Cube" => CommonData.CubeCornerns,
                "Pyramid" => CommonData.CubeCornerns,
                _ => [],
            };
        }

        //public (Vector3, Vector3) GetDiapsonVertexY(int index)
        //{
        //    Vector3 min = AABB.GetCubeCorner(ObjectData[index].Scale, ObjectData[index].Position, ObjectData[index].Rotate, ObjectData[index].Corners[0]);
        //    Vector3 max = min;

        //    foreach (var c in ObjectData[index].Corners)
        //    {
        //        var newCorner = AABB.GetCubeCorner(ObjectData[index].Scale, ObjectData[index].Position, ObjectData[index].Rotate, c);
        //        if (min.Y > newCorner.Y) min = newCorner;
        //        if (max.Y < newCorner.Y) max = newCorner;
        //    }

        //    return (min, max);
        //}

        public override Vector3 GetVertex(int objectIdx, int vertexIdx)
            => AABB.GetCubeCorner(ObjectData[objectIdx].Position, ObjectData[objectIdx].Scale, ObjectData[objectIdx].Corners[vertexIdx]);

        protected void Register(int VAO, int VBO, List<float> Data, int byteCount, bool update = true, bool sub = false, int idx = 0)
        {
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            if (update)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (Count - 1) * byteCount * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BufferData(BufferTarget.ArrayBuffer, Data.Count * sizeof(float), Data.ToArray(), BufferUsageHint.DynamicDraw);
            }
            else
            {
                if (!sub)
                    GL.BufferSubData(BufferTarget.ArrayBuffer, 0, Data.Count * sizeof(float), Data.ToArray());
                else
                    GL.BufferSubData(BufferTarget.ArrayBuffer, idx * byteCount * sizeof(float),
                                                            byteCount * sizeof(float), Data.GetRange(idx * byteCount, byteCount).ToArray());
            }
        }
        public override void Update(int idx, float deltaTime)
        {
            if (!ObjectData[idx].Runnable)
            {
                Data temp = ObjectData[idx];
                temp.InitPosition = temp.Position;
                temp.InitRotate = temp.Rotate;
                temp.InitVelocity = temp.Velocity;
                temp.Runnable = true;
                ObjectData[idx] = temp;
            }

            {
                Data temp = ObjectData[idx];
                temp.Position += ObjectData[idx].Velocity * deltaTime;
                ObjectData[idx] = temp;
            }

            UpdateModel(idx, ObjectData[idx].Position, ObjectData[idx].Rotate, ObjectData[idx].Scale);

        }

        public override void Undo()
        {
            for (int idx = 0; idx < ObjectData.Count; idx++)
            {
                if (!ObjectData[idx].PhysicsEnable) continue;

                Data temp = ObjectData[idx];
                temp.Position = temp.InitPosition;
                temp.Rotate = temp.InitRotate;
                temp.Velocity = temp.InitVelocity;
                temp.Runnable = false;
                ObjectData[idx] = temp;

                UpdateModel(idx, ObjectData[idx].Position, ObjectData[idx].Rotate, ObjectData[idx].Scale);
            }
        }

        public override void Save(StreamWriter writer)
        {
            for (int i = 0; i < Count; i++)
            {
                writer.Write(Name + " " + Index);

                ToolBar.Stream.WriteVector(writer, ObjectData[i].Position);
                ToolBar.Stream.WriteVector(writer, ObjectData[i].Scale);
                ToolBar.Stream.WriteVector(writer, ObjectData[i].Rotate);

                ToolBar.Stream.WriteSubList(writer, Colors, i * 3, 3);

                ToolBar.Stream.WriteVector(writer, ObjectData[i].Velocity);
                writer.Write(" " + ObjectData[i].Mass);

                ToolBar.Stream.WriteSubList(writer, Materials, i * 3, 3);

                writer.Write(" " + ObjectData[i].PhysicsEnable);

                writer.Write(" " + TexturePath);

                writer.WriteLine();
            }
        }
    }
}
