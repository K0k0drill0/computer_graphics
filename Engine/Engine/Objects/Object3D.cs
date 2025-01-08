//using ImGuiNET;
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;
//using Engine.ToolBar;

//namespace Engine.Objects
//{
//    internal abstract class Object3D : Object
//    {
//        public enum ObjectType
//        {
//            Cube,
//        }

//        public struct Data
//        {
//            public int Index { get; private set; }

//            public Vector3[] Corners { get; set; }

//            public Vector3 Position { get; set; }
//            public Vector3 Rotate { get; set; }
//            public Vector3 Scale { get; set; }

//            public Vector3 Velocity { get; set; }
//            public float Mass { get; set; }

//            public Vector3 Materials { get; set; }

//            public float[] Model { get; set; }

//            public Data(Vector3 position, Vector3 rotate, Vector3 scale, Vector3 velocity, float mass, Vector3[] corners, int idx)
//            {
//                Index = idx;

//                Corners = corners;

//                Position = position;
//                Rotate = rotate;
//                Scale = scale;

//                Velocity = velocity;
//                Mass = mass;
//            }
//        }
//        public string TexturePath { get; set; }

//        public virtual void AddTexture(string texturePath)
//            => TexturePath = texturePath;

//        public override int Count => ObjectData.Count;
//        public List<Data> ObjectData { get; private set; }
//        public List<float> Models { get; private set; }
//        public List<float> Colors { get; private set; }
//        public List<float> Materials { get; private set; }

//        public Object3D()
//        {
//            ObjectData = new List<Data>();
//            Models = new List<float>();
//            Colors = new List<float>();
//            Materials = new List<float>();
//            TexturePath = "Empty";
//        }

//        protected void Add(ObjectType type)
//        {
//            var corners = defineCorners(type);

//            ObjectData.Add(new Data(new Vector3(0f), new Vector3(0f), new Vector3(1f), new Vector3(0f), 1f, corners, Count));
//            Colors.AddRange([0.5f, 0.5f, 0.5f]);
//            Materials.AddRange([0.45f, 0.8f, 1f]);

//            UpdateModel(ObjectData.Count - 1, new Vector3(0f), new Vector3(0f), new Vector3(1f));
//        }

//        protected void Add(ObjectType type, Vector3 position, Vector3 rotate, Vector3 scale, float[] color, Vector3 velocity, float mass, float ambient, float diffuse, float specular)
//        {
//            var corners = defineCorners(type);

//            ObjectData.Add(new Data(position, rotate, scale, velocity, mass, corners, Count));

//            Colors.AddRange(color);
//            Materials.AddRange([ambient, diffuse, specular]);
//            UpdateModel(ObjectData.Count - 1, position, rotate, scale);
//        }

//        protected void UpdateModel(int idx, Vector3 position, Vector3 rotate, Vector3 scale)
//        {
//            float[] array = new float[16];
//            Common.CalculateModel(ref array, position, scale, rotate);

//            Data temp = ObjectData[idx];
//            temp.Model = array;
//            ObjectData[idx] = temp;

//            if (idx < Models.Count / 16)
//            {
//                Models.RemoveRange(idx * 16, 16);
//                Models.InsertRange(idx * 16, ObjectData[idx].Model);
//            }
//            else Models.AddRange(ObjectData[idx].Model);
//        }

//        public override void Delete(int idx)
//        {
//            ObjectData.RemoveAt(idx);
//            Colors.RemoveRange(idx * 3, 3);
//            Models.RemoveRange(idx * 16, 16);
//            Materials.RemoveRange(idx * 3, 3);
//        }

//        public override Vector3 GetColor(int idx) => (Colors[idx * 3], Colors[idx * 3 + 1], Colors[idx * 3 + 2]);

//        public override Vector3 GetPosition(int idx) => ObjectData[idx].Position;
//        public override Vector3 GetRotate(int idx) => ObjectData[idx].Rotate;
//        public override Vector3 GetScale(int idx) => ObjectData[idx].Scale;
//        public Vector3 GetVelocity(int idx) => ObjectData[idx].Velocity;



//        public override void SetModel(Model model, int idx, Vector3 value)
//        {
//            if (Count > idx)
//            {
//                Data temp = ObjectData[idx];

//                switch (model)
//                {
//                    case Model.Position:
//                        //if (!Window.isPhysicsWorld) temp.StartPosition = value;
//                        temp.Position = value;
//                        ObjectData[idx] = temp;
//                        break;

//                    case Model.Rotate:
//                        //if (!Window.isPhysicsWorld) temp.StartRotate = value;
//                        temp.Rotate = value;
//                        ObjectData[idx] = temp;
//                        break;

//                    case Model.Scale:
//                        temp.Scale = value;
//                        ObjectData[idx] = temp;
//                        break;

//                    case Model.Color:
//                        Colors[idx * 3] = value.X;
//                        Colors[idx * 3 + 1] = value.Y;
//                        Colors[idx * 3 + 2] = value.Z;
//                        break;

//                    case Model.Velocity:
//                        temp.Velocity = value;
//                        ObjectData[idx] = temp;
//                        break;


//                    default:
//                        throw new Exception("Check model of SetModel call");
//                }

//                if (model == Model.Rotate || model == Model.Position || model == Model.Scale)
//                    UpdateModel(idx, ObjectData[idx].Position, ObjectData[idx].Rotate, ObjectData[idx].Scale);
//            }
//            else
//                throw new Exception("Check idx in GetTranslate call");
//        }
//        public override void SetModel(Model model, int idx, float value)
//        {
//            if (Count > idx)
//            {
//                Data temp = ObjectData[idx];

//                switch (model)
//                {
//                    case Model.Material:
//                        Materials[idx * 3] = value;
//                        Materials[idx * 3 + 1] = value;
//                        Materials[idx * 3 + 2] = value;
//                        break;

//                    default:
//                        throw new Exception("Check model of SetModel call");
//                }
//            }
//            else
//                throw new Exception("Check idx in GetTranslate call");
//        }

//        //public Vector3 GetMinVertex(int index)
//        //{
//        //    Vector3 res = ObjectData[index].Corners[0];

//        //    foreach (var c in ObjectData[index].Corners)
//        //        if (res.Y > c.Y) res = c;

//        //    return AABB.GetCubeCorner(ObjectData[index].Scale, ObjectData[index].Position, ObjectData[index].Rotate, res);
//        //}

//        //public void Save(StreamWriter writer, int idx, List<Vector3> direction = null, List<float> specialData = null, int specialDataStep = 2)
//        //{
//        //    for (int i = 0; i < Count; i++)
//        //    {
//        //        writer.Write(Name + " " + idx + " " + ObjectData[i].StartPosition.X + " " + ObjectData[i].StartPosition.Y + " " + ObjectData[i].StartPosition.Z + " "
//        //            + ObjectData[i].Scale + " "
//        //            + ObjectData[i].StartRotate.X + " " + ObjectData[i].StartRotate.Y + " " + ObjectData[i].StartRotate.Z + " "
//        //            + Colors[i * 3] + " " + Colors[i * 3 + 1] + " " + Colors[i * 3 + 2] + " "
//        //            + ObjectData[i].Velocity.X + " " + ObjectData[i].Velocity.Y + " " + ObjectData[i].Velocity.Z + " "
//        //            + ObjectData[i].Mass + " " + LightData[i * 3] + " " + LightData[i * 3 + 1] + " " + LightData[i * 3 + 2]);

//        //        if (direction is not null)
//        //            writer.Write(" " + direction[i].X + " " + direction[i].Y + " " + direction[i].Z);

//        //        if (specialData is not null)
//        //            for (int j = 0; j < specialDataStep; j++)
//        //                writer.Write(" " + specialData[i * specialDataStep + j]);

//        //        writer.WriteLine();
//        //    }
//        //}

//        //public void Bind(int idx)
//        //{
//        //    ImGui.Text("Bundle");

//        //    ImguiUpdateAxisHelper("X", ref idx, Vector3.UnitX);
//        //    ImguiUpdateAxisHelper("-X", ref idx, -Vector3.UnitX);
//        //    ImguiUpdateAxisHelper("Y", ref idx, Vector3.UnitY);
//        //    ImguiUpdateAxisHelper("-Y", ref idx, -Vector3.UnitY);
//        //    ImguiUpdateAxisHelper("Z", ref idx, Vector3.UnitZ);
//        //    ImguiUpdateAxisHelper("-Z", ref idx, -Vector3.UnitZ);
//        //}

//        //public void ImguiUpdateAxisHelper(string label, ref int idx, Vector3 additionalVector)
//        //{
//        //    if (ImGui.Button(label, new System.Numerics.Vector2(25, 25)))
//        //        SetModel(Model.Translate, idx, ObjectData[idx - 1].Position + additionalVector);
//        //    if (label != "-Z") ImGui.SameLine();
//        //}

//        private Vector3[] defineCorners(ObjectType type)
//        {
//            switch (type)
//            {
//                case ObjectType.Cube:
//                    return CommonData.CubeCornerns;
//                default:
//                    return [];
//            }
//        }
//    }
//}
