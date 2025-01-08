using Engine.Lights;
using Engine.Objects;
using ImGuiNET;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

namespace Engine.ToolBar
{
    internal class GUI_Helper
    {
        //public static void Vec2(Object obj, Object.Model model, string label, ref System.Numerics.Vector2 vec, int idx,
        //    float step, float min = float.NaN, float max = float.NaN, string format = "%.3f")
        //{
        //    if (ImGui.DragFloat2(label, ref vec, step, min, max, format))
        //        obj.SetModel(model, idx, Transform.ToOpenTK(vec));
        //}

        public static void Vec3(Object obj, Object.Model model, string label, ref System.Numerics.Vector3 vec, int idx,
            float step, float min = float.NaN, float max = float.NaN, string format = "%.3f")
        {
            if (ImGui.DragFloat3(label, ref vec, step, min, max, format))
                obj.SetModel(model, idx, Transform.ToOpenTK(vec));
        }

        public static void Vec3(ref Vector3 objVec, string label, ref System.Numerics.Vector3 sysVec,
            float step, float min = float.NaN, float max = float.NaN, string format = "%.3f")
        {
            if (ImGui.DragFloat3(label, ref sysVec, step, min, max, format))
                objVec = Transform.ToOpenTK(sysVec);
        }

        public static void Bind(ObjectManager objectManager, List<Object?> objects, List<int> indexes, int idx)
        {
            objectManager.Objects.Keys.ToList().ForEach(key =>
            {
                if (objectManager.Objects[key].First() is ObjectsInstance)
                {
                    foreach (var el in objectManager.Objects[key])
                    {
                        if (ImGui.TreeNode($"{key} {el.Index}"))
                        {
                            for (int i = 0; i < el.Count; i++)
                            {
                                if (ImGui.Button($"{key} {el.Index} - {i}", new System.Numerics.Vector2(100f, 50f)))
                                {
                                    objects[idx] = el;
                                    indexes[idx] = i;
                                }
                            }

                            ImGui.TreePop();
                        }
                    }
                }
            });
        }

        public static void BindEntity(Light light, Object obj, int idx)
        {
            if (obj is not DirectionLight && obj is not PointLight && obj is not SpotLight)
            {
                if (light.PointLight != null)
                {
                    for (int i = 0; i < light.PointLight.Count; i++)
                    {
                        if (light.PointLight.GetBindObject(i) == obj && light.PointLight.GetBindObjectIndex(i) == idx)
                        {
                            if (ImGui.TreeNode("Point Light Settings"))
                            {
                                light.PointLight.ShowNativeDataDialog(ref i);
                                ImGui.TreePop();
                            }
                        }
                    }
                }
                if (light.SpotLight != null)
                {
                    for (int i = 0; i < light.SpotLight.Count; i++)
                    {
                        if (light.SpotLight.GetBindObject(i) == obj && light.SpotLight.GetBindObjectIndex(i) == idx)
                        {
                            if (ImGui.TreeNode("Spot Light Settings"))
                            {
                                light.SpotLight.ShowNativeDataDialog(ref i);
                                ImGui.TreePop();
                            }
                        }
                    }
                }
            }
        }
    }
}
