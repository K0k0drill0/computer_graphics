using Engine.Camera;
using Engine.Objects;
using Engine.Shaders;
using Engine.SystemInteraction;
using Engine.ToolBar;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Lights
{
    internal sealed class PointLight : DisembodiedObject
    {
        public static new string StaticName => "PointLight";
        public override string Name => "PointLight";
        public override int Count => colors.Count;

        private List<Vector3> colors     = [];
        private List<int>     indexes    = [];
        private List<bool>    activities = [];
        private List<Vector3> arguments  = [];

        public override void Add(string name)
        {
            colors.Add(new Vector3(0.5f));
            objects.Add(null);
            indexes.Add(-1);
            activities.Add(true);
            arguments.Add(new(1f, 0.09f, 0.032f));
        }

        public void Create(Vector3 color, Vector3 arguments, Object obj, int idx)
        {
            colors.Add(color);
            this.arguments.Add(arguments);
            objects.Add(obj);
            indexes.Add(idx);
            activities.Add(false);
        }

        public PointLight(ObjectManager objectManager) : base(objectManager)
        {
            Index = objectManager.Count;
            ManagerUpdate(this, Name);
        }

        public override bool GetActive(int idx)   => activities[idx];
        public override Vector3 GetColor(int idx) => colors[idx];
        public Vector3 GetArguments(int idx) => arguments[idx];
        public int GetBindObjectIndex(int idx)    => indexes[idx];
        public override Vector3 GetPosition(int idx) 
            => objects[idx] == null ? new Vector3(0f) : objects[idx].GetPosition(indexes[idx]);

        public override void SetModel(Model model, int idx, Vector3 value)
        {
            if (Count > idx)
            {
                switch (model)
                {
                    case Model.Color:
                        colors[idx] = value;
                        break;

                    case Model.Arguments:
                        arguments[idx] = value;
                        break;

                    default:
                        throw new Exception("Check model of SetModel call");
                }
            }
            else throw new Exception("Check idx in SetTranslate call");
        }

        public override void Delete(int idx)
        {
            colors.RemoveAt(idx);
            objects.RemoveAt(idx);
            indexes.RemoveAt(idx);
        }

        public override void Save(StreamWriter writer)
        {
            for (int i = 0; i < Count; i++)
            {
                writer.Write(Name);
                Engine.ToolBar.Stream.WriteVector(writer, colors[i]);
                Engine.ToolBar.Stream.WriteVector(writer, arguments[i]);
                if (objects[i] == null) writer.Write(" null -1");
                else writer.Write(" " + objects[i].Name + " " + objects[i].Index + " " + indexes[i]);
                writer.WriteLine();
            }
        }

        public static void Load(Light light, ref string[] settings)
        {
            int start = 1;

            Engine.ToolBar.Stream.ReadVector(out Vector3 color, ref start, ref settings);
            Engine.ToolBar.Stream.ReadVector(out Vector3 arguments, ref start, ref settings);
            string objName = settings[start++];
            int globalIdx = int.Parse(settings[start++]);
            int localIdx = int.Parse(settings[start++]);

            light.CreatePointLight(color, arguments, light.objectManager.Objects[objName][globalIdx], localIdx);
        }

        public override void ShowNativeDataDialog(ref int idx)
        {
            System.Numerics.Vector3 curColor = Transform.ToSystemNumerics(GetColor(idx));
            System.Numerics.Vector3 curArguments = Transform.ToSystemNumerics(GetArguments(idx));

            ImGui.Text(Name + " " + Index);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            if (ImGui.ColorEdit3("Color", ref curColor))
                SetModel(Model.Color, idx, Transform.ToOpenTK(curColor));

            GUI_Helper.Vec3(this, Model.Arguments, "Arguments", ref curArguments, idx, 0.001f);


            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            GUI_Helper.Bind(objectManager, objects, indexes, idx);

            if (ImGui.Button("Delete", new System.Numerics.Vector2(100f, 50f)))
                ImguiDelete(ref idx);
        }



        public void ImguiDelete(ref int idx)
        {
            if (Count == 1)
            {
                objectManager.Remove(Name, Index);
                idx -= 1;
                return;
            }

            Delete(idx);
        }
    }
}
