using Engine.ToolBar;

using OpenTK.Mathematics;

using ImGuiNET;
using Engine.Camera;

namespace Engine.Lights
{
    internal class DirectionLight : Object
    {
        public static new string StaticName => "DirectionLight";
        public override string Name => "DirectionLight";
        public override int Count => directions.Count;

        private List<Vector3> colors;
        private List<Vector3> directions;
        private List<Shadow>  shadows;
        private List<bool>    activities;

        public DirectionLight(ObjectManager objectManager) : base(objectManager)
        {
            Index = objectManager.Count;
            ManagerUpdate(this, Name);

            colors     = [];
            directions = [];
            shadows    = [];
            activities = [];
        }

        public override bool GetActive(int idx = 0) => false;
        public override Vector3 GetColor(int idx)   => colors[idx];
        public Vector3 GetDirection(int idx)        => directions[idx];
        public Shadow GetShadow(int idx)            => shadows[idx];

        public override void SetModel(Model model, int idx, Vector3 value)
        {
            if (Count > idx)
            {
                switch (model)
                {
                    case Model.Direction:
                        directions[idx] = value;
                        break;

                    case Model.Color:
                        colors[idx] = value;
                        break;

                    default:
                        throw new Exception("Check model of SetModel call");
                }
            }
            else throw new Exception("Check idx in SetTranslate call");
        }

        public void Create()
        {
            colors.Add(new Vector3(0.5f));
            directions.Add(new Vector3(0f, -1f, 0f));
            shadows.Add(new Shadow());
        }

        public void Create(Vector3 color, Vector3 direction)
        {
            colors.Add(color);
            directions.Add(direction);
            shadows.Add(new Shadow());
        }

        public override void Save(StreamWriter writer)
        {
            for (int i = 0; i < Count; i++)
            {
                writer.Write(Name);
                Engine.ToolBar.Stream.WriteVector(writer, colors[i]);
                Engine.ToolBar.Stream.WriteVector(writer, directions[i]);
                writer.WriteLine();
            }
        }

        public static void Load(Light light, ref string[] settings)
        {
            int start = 1;

            Engine.ToolBar.Stream.ReadVector(out Vector3 color, ref start, ref settings);
            Engine.ToolBar.Stream.ReadVector(out Vector3 direction, ref start, ref settings);

            light.CreateDirectionLight(color, direction);
        }

        public override void ShowNativeDataDialog(ref int idx)
        {
            System.Numerics.Vector3 currentColor     = Transform.ToSystemNumerics(GetColor(idx));
            System.Numerics.Vector3 currentDirection = Transform.ToSystemNumerics(GetDirection(idx));

            if (ImGui.ColorEdit3("Color", ref currentColor))
                SetModel(Model.Color, idx, Transform.ToOpenTK(currentColor));

            GUI_Helper.Vec3(this, Model.Direction, "Direction", ref currentDirection, idx, 0.005f, -1f, 1f);
        }

        public override void Add(string name) => throw new NotImplementedException();
        public override void Delete(int idx)  => throw new NotImplementedException();
        public override void Draw(BaseCamera camera, Light light, bool isPhysicsWorld) { }
        public override void Update(int idx, float deltaTime) { }
        public override void Undo() { }

        public override bool GetPhysicsEnabled(int idx)
            => throw new NotImplementedException();
        public override void ChangePhysicsEnabled(int idx) { }
        public override Vector3 GetVertex(int objectIdx, int vertexIdx)
            => new(float.MaxValue);

        public override Vector3 GetPosition(int idx) => throw new NotImplementedException();
        public override Vector3 GetRotate(int idx)   => throw new NotImplementedException();
        public override Vector3 GetScale(int idx)    => throw new NotImplementedException();
    }
}
