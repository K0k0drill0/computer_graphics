using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Engine.Shaders;
using Engine.Camera;
using Engine.Objects.Objects3D;
using Engine.Objects;
using Engine.ToolBar;
using ImGuiNET;
using static Engine.Object;
using static System.Windows.Forms.DataFormats;

namespace Engine.Lights
{
    internal class SpotLight : DisembodiedObject
    {
        public static new string StaticName => "SpotLight";
        public override string Name => "SpotLight";
        public override int Count => colors.Count;

        private List<Vector3> directions    = [];
        private List<Vector3> colors    = [];
        private List<int> indexes       = [];
        private List<bool> activities   = [];
        private List<Vector3> arguments = [];
        private List<Vector2> edges     = [];

        public override void Add(string name)
        {
            directions.Add(new Vector3(0f));

            colors.Add(new Vector3(0.5f));

            objects.Add(null);
            indexes.Add(-1);

            activities.Add(true);
            arguments.Add(new(1f, 0.09f, 0.032f));

            edges.Add(new(7.5f, 12.5f));
        }

        public void Create(Vector3 color, Vector3 arguments, Vector2 edges, Vector3 direction, Object obj, int idx)
        {
            directions.Add(direction);

            colors.Add(color);

            this.arguments.Add(arguments);

            objects.Add(obj);
            indexes.Add(idx);

            activities.Add(false);
            this.edges.Add(edges);
        }

        public SpotLight(ObjectManager objectManager) : base(objectManager)
        {
            Index = objectManager.Count;
            ManagerUpdate(this, Name);
        }

        public override bool GetActive(int idx) => activities[idx];
        public Vector3 GetDirection(int idx) => directions[idx];
        public override Vector3 GetColor(int idx) => colors[idx];
        public Vector3 GetArguments(int idx) => arguments[idx];
        public Vector2 GetCutOff(int idx) => edges[idx];
        public int GetBindObjectIndex(int idx) => indexes[idx];
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

                    case Model.Direction:
                        directions[idx] = value;
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
                Engine.ToolBar.Stream.WriteVector(writer, directions[i]);
                Engine.ToolBar.Stream.WriteVector(writer, colors[i]);
                Engine.ToolBar.Stream.WriteVector(writer, arguments[i]);
                Engine.ToolBar.Stream.WriteVector(writer, edges[i]);
                if (objects[i] == null) writer.Write(" null -1");
                else writer.Write(" " + objects[i].Name + " " + objects[i].Index + " " + indexes[i]);
                writer.WriteLine();
            }
        }

        public static void Load(Light light, ref string[] settings)
        {
            int start = 1;

            Engine.ToolBar.Stream.ReadVector(out Vector3 direction, ref start, ref settings);
            Engine.ToolBar.Stream.ReadVector(out Vector3 color, ref start, ref settings);
            Engine.ToolBar.Stream.ReadVector(out Vector3 arguments, ref start, ref settings);
            Engine.ToolBar.Stream.ReadVector(out Vector2 edges, ref start, ref settings);
            string objName = settings[start++];
            int globalIdx = int.Parse(settings[start++]);
            int localIdx = int.Parse(settings[start++]);

            light.CreateSpotLight(color, arguments, edges, direction, light.objectManager.Objects[objName][globalIdx], localIdx);
        }

        public override void ShowNativeDataDialog(ref int idx)
        {
            System.Numerics.Vector3 curDirection = Transform.ToSystemNumerics(GetDirection(idx));
            System.Numerics.Vector3 curColor = Transform.ToSystemNumerics(GetColor(idx));
            System.Numerics.Vector3 curArguments = Transform.ToSystemNumerics(GetArguments(idx));
            System.Numerics.Vector2 curEdges = Transform.ToSystemNumerics(edges[idx]);

            ImGui.Text(Name + " " + Index);

            ImGui.Dummy(new System.Numerics.Vector2(0, 10));

            GUI_Helper.Vec3(this, Model.Direction, "Direction", ref curDirection, idx, 0.05f, -1f, 1f);

            if (ImGui.ColorEdit3("Color", ref curColor))
                SetModel(Model.Color, idx, Transform.ToOpenTK(curColor));

            GUI_Helper.Vec3(this, Model.Arguments, "Arguments", ref curArguments, idx, 0.001f);

            if (ImGui.DragFloat2("CutOff", ref curEdges, 0.25f, 0f, 45f, "%.2f"))
                edges[idx] = Transform.ToOpenTK(curEdges);

            //GUI_Helper.Vec2(this, Model.Arguments, "CutOff", ref curEdges, idx, 0.25f, 0f, 45f, "%.2f");


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
