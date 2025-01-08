using OpenTK.Windowing.Desktop;
using Engine.Objects.Objects3D;
using Engine.Lights;
using Engine.ToolBar;
using System.Numerics;
using ImGuiNET;
using Engine.Camera;
using System.Drawing;
using Engine.Objects;
using System.Timers;

namespace Engine
{
    internal class HUD
    {
        ImGuiController _controller;
        float width, height;
        GameWindow window;

        float currentScale, currentAmbient, currentDiffuse, currentSpecular, curCutOff, curOutCutOff;

        ObjectManager objectManager;
        Light light;
        EngineCamera engineCamera;
        GameCamera gameCamera;
        Player player;

        Object activeObject = null;
        int activeObjectIndex = 0;

        string buttonName;

        internal HUD(NativeWindowSettings nativeWindow, GameWindow gameWindow)
        {
            window = gameWindow;
            height = nativeWindow.ClientSize.Y;
            width  = nativeWindow.ClientSize.X;
            _controller = new ImGuiController((int)width, (int)height);
        }

        public void Load(object activeObject)
        {
            switch (activeObject) {
                case Light:
                    light = (Light)activeObject;
                    break;

                case EngineCamera:
                    engineCamera = (EngineCamera)activeObject;
                    break;

                case GameCamera:
                    gameCamera = (GameCamera)activeObject;
                    break;

                case ObjectManager:
                    objectManager = (ObjectManager)activeObject;
                    break;

                case Player:
                    player = (Player)activeObject;
                    break;

                default:
                    throw new Exception("Check type in HUD Load call");
            }
        }

        public void Update(Window window, float time) => _controller.Update(window, time);

        public void FocusObj(Object newActiveObject, int newActiveObjectIndex)
        {
            if (activeObject != null)
            {
                if (activeObject is DisembodiedObject disembodied && disembodied.GetBindObject(activeObjectIndex) is null)
                {
                    activeObject.Delete(activeObjectIndex);
                }
            }

            activeObject = newActiveObject;
            activeObjectIndex = newActiveObjectIndex;
        }

        public void Render(ref bool needToSave, ref bool needToLoad, ref bool isFocused, ref bool isRunning, ref bool firstMove)
        {
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(400, height), ImGuiCond.Always);
            ImGui.SetNextWindowCollapsed(isFocused);

            ImGui.Begin("Settings", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

            if (ImGui.BeginMenu("Create"))
            {
                bool allObjectsAreAdded = true;
                objectManager.RegistratedObjects.Keys.ToList().ForEach(key =>
                {
                    if (!objectManager.RegistratedObjects[key].IsActive)
                    {
                        if (ImGui.Button($"{key}", new Vector2(175f, 50f)))
                        {
                            Type classType = objectManager.RegistratedObjects[key].ObjectType;
                            Activator.CreateInstance(classType, objectManager);

                            var activeObject = objectManager.Objects[key][0];

                            if (activeObject is FlatObjects flat)
                            {
                                flat.LoadFiles("../../../Fonts/latinText.fnt", "../../../Fonts/latinText.png");
                                flat.Resize(width, height);
                            } 
                            else if (activeObject is PointLight)
                            {
                                objectManager.Remove(activeObject.Name, activeObject.Index);
                                light.CreatePointLight();
                                activeObject = light.PointLight;
                            }
                            else if (activeObject is SpotLight)
                            {
                                objectManager.Remove(activeObject.Name, activeObject.Index);
                                light.CreateSpotLight();
                                activeObject = light.SpotLight;
                            }

                            objectManager.Objects[key][0].Add(key);
                            FocusObj(activeObject, 0);
                        }
                        allObjectsAreAdded = false;
                    }
                });

                if (allObjectsAreAdded)
                {
                    ImGui.Text("All possible objects have already been added to the scene");
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Add"))
            {
                objectManager.Objects.Keys.ToList().ForEach(key =>
                {
                    if (objectManager.Objects[key][0] is ObjectsInstance)
                    {
                        if (ImGui.BeginMenu(key))
                        {
                            foreach (var el in objectManager.Objects[key])
                            {
                                if (ImGui.Button($"Add {key} {el.Index}", new Vector2(175f, 50f)))
                                {
                                    el.Add(key);
                                    FocusObj(el, el.Count - 1);
                                }
                            }

                            if (ImGui.Button($"Add {key}", new Vector2(175f, 50f)))
                            {
                                objectManager.Add(key).Add(key);
                                FocusObj(objectManager.Objects[key][0], 0);
                            }

                            ImGui.EndMenu();
                        }
                    } 
                    else
                    {
                        if (ImGui.BeginMenu(key))
                        {
                            var el = objectManager.Objects[key][0];

                            if (ImGui.Button($"Add {key}", new Vector2(175f, 50f)))
                            {
                                el.Add(key);
                                activeObject = el;
                                activeObjectIndex = el.Count - 1;
                            }
                            
                            ImGui.EndMenu();
                        }
                    }
                });
                ImGui.EndMenu();
            }

            if (ImGui.TreeNode("Common Settings"))
            {
                if (ImGui.TreeNode("Player"))
                {
                    if (player == null) ImGui.Text("Player don't exist");
                    else player.ShowNativeDataDialog(ref activeObjectIndex);

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Engine Camera"))
                {
                    bool isOrtho = engineCamera.IsOrtho;
                    float fov = engineCamera.Fov;

                    if (ImGui.DragFloat("Fov", ref fov, 0.05f))
                        engineCamera.Fov = fov;

                    if (ImGui.Checkbox("Orthographic projection", ref isOrtho))
                        engineCamera.IsOrtho = isOrtho;


                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Game Camera"))
                {
                    bool isOrtho = gameCamera.IsOrtho;
                    bool movementEnabled = gameCamera.movementEnabled;
                    bool rotateEnabled = gameCamera.rotateEnabled;
                    Vector3 gcPosition = Transform.ToSystemNumerics(gameCamera.Position);
                    float pitch = gameCamera.Pitch, yaw = gameCamera.Yaw, fov = gameCamera.Fov;

                    if (ImGui.DragFloat3("GC Position", ref gcPosition, 0.05f))
                        gameCamera.position = Transform.ToOpenTK(gcPosition);

                    if (ImGui.DragFloat("Pitch", ref pitch, 0.05f))
                        gameCamera.Pitch = pitch;

                    if (ImGui.DragFloat("Yaw", ref yaw, 0.05f))
                        gameCamera.Yaw = yaw;

                    if (ImGui.DragFloat("Fov", ref fov, 0.05f))
                        gameCamera.Fov = fov;
                    ImGui.Dummy(new Vector2(0, 10));


                    if (ImGui.Checkbox("Orthographic projection", ref isOrtho))
                        gameCamera.IsOrtho = isOrtho;
                    ImGui.Dummy(new Vector2(0, 10));


                    ImGui.Checkbox("Pitch Rotate Enabled", ref gameCamera.pitchEnabled);
                    ImGui.Checkbox("Yaw Rotate Enabled", ref gameCamera.yawEnabled);
                    if (ImGui.Checkbox("Rotate Enabled", ref rotateEnabled))
                        gameCamera.RotateEnabled = rotateEnabled;
                    ImGui.Dummy(new Vector2(0, 10));

                    ImGui.Checkbox("X-Axis Movement Enabled", ref gameCamera.xAxisMovementEnabled);
                    ImGui.Checkbox("Y-Axis Movement Enabled", ref gameCamera.yAxisMovementEnabled);
                    ImGui.Checkbox("Z-Axis Movement Enabled", ref gameCamera.zAxisMovementEnabled);
                    if (ImGui.Checkbox("Movement Enabled", ref movementEnabled))
                        gameCamera.MovementEnabled = movementEnabled;
                    ImGui.Dummy(new Vector2(0, 10));

                    ImGui.Checkbox("Front-Back Movement Enabled", ref gameCamera.Keyboard_FrontBackMovement_Enabled);
                    ImGui.Checkbox("Right-Left Movement Enabled", ref gameCamera.Keyboard_RightLeftMovement_Enabled);
                    ImGui.Checkbox("Up-Down Movement Enabled", ref gameCamera.Keyboard_UpDownMovement_Enabled);
                    ImGui.Dummy(new Vector2(0, 10));

                    if (gameCamera.xAxisMovementEnabled || gameCamera.yAxisMovementEnabled || gameCamera.zAxisMovementEnabled) gameCamera.movementEnabled = true;
                    if (!gameCamera.xAxisMovementEnabled && !gameCamera.yAxisMovementEnabled && !gameCamera.zAxisMovementEnabled) gameCamera.movementEnabled = false;
                    if (gameCamera.yawEnabled || gameCamera.pitchEnabled) gameCamera.rotateEnabled = true;
                    if (!gameCamera.yawEnabled && !gameCamera.pitchEnabled) gameCamera.rotateEnabled = false;

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Direction Light Settings"))
                {
                    if (light.DirectionLight == null || light.DirectionLight.Count == 0)
                        ImGui.Text("Direction Light don't exist");
                    else
                    {
                        int DLIndex = light.DirectionLight.Count - 1;
                        light.DirectionLight.ShowNativeDataDialog(ref DLIndex);
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Buttons"))
                {
                    foreach (string s in KeyboardManager.Buttons.Keys)
                    {
                        ImGui.Text(s);
                        ImGui.SameLine();
                        if (ImGui.Button(KeyboardManager.GetButton(s).ToString()))
                            KeyboardManager.ActiveName = s;

                        ImGui.Dummy(new Vector2(0, 5));

                    }

                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }


            if (activeObject != null && activeObjectIndex >= 0)
            {
                activeObject.ShowNativeDataDialog(ref activeObjectIndex);

                if (activeObjectIndex == -1)
                {
                    activeObject = null;
                }

                ImGui.Dummy(new Vector2(0, 10));

                GUI_Helper.BindEntity(light, activeObject, activeObjectIndex);
            }

            //    currentColor = Transform.ToSystemNumerics(activeObject.GetColor(activeObjectIndex));
            //    currentTranslate = Transform.ToSystemNumerics(activeObject.GetPosition(activeObjectIndex));
            //    currentRotate = Transform.ToSystemNumerics(activeObject.GetRotate(activeObjectIndex));
            //    currentScale = activeObject.GetScale(activeObjectIndex);

            //    if (activeObject is SpotLight light)
            //    {
            //        curCutOff = light.GetSpecialData(activeObjectIndex, 2, 0);
            //        curOutCutOff= light.GetSpecialData(activeObjectIndex, 2, 1);
            //        currentDirection = Transform.ToSystemNumerics(light.GetDirection(activeObjectIndex));
            //    }

            //    ImGui.Text(activeObject.Name + " " + activeObject.Index);

            //    if (ImGui.ColorEdit3("Color", ref currentColor))
            //        activeObject.SetModel(Object.Model.Color, activeObjectIndex, Transform.ToOpenTK(currentColor));

            //    if (ImGui.DragFloat3("Translate", ref currentTranslate, 0.1f))
            //        activeObject.SetModel(Object.Model.Translate, activeObjectIndex, Transform.ToOpenTK(currentTranslate));

            //    if (ImGui.DragFloat3("Rotate", ref currentRotate, 0.1f))
            //        activeObject.SetModel(Object.Model.Rotate, activeObjectIndex, Transform.ToOpenTK(currentRotate));

            //    if (ImGui.DragFloat("Scale", ref currentScale, 0.01f, 0.1f, currentScale * 1.1f, "%.2f"))
            //        activeObject.SetModel(Object.Model.Scale, activeObjectIndex, currentScale);

            //    if (activeObject is not PointLight && activeObject is not SpotLight && activeObject is Object3D object3D)
            //    {
            //        currentAmbient = object3D.LightData[activeObjectIndex * 3];
            //        currentDiffuse = object3D.LightData[activeObjectIndex * 3 + 1];
            //        currentSpecular = object3D.LightData[activeObjectIndex * 3 + 2];

            //        if (ImGui.DragFloat("Ambient", ref currentAmbient, 0.005f, 0f, 1f, "%.3f"))
            //            activeObject.SetModel(Object.Model.Ambient, activeObjectIndex, currentAmbient);

            //        if (ImGui.DragFloat("Diffuse", ref currentDiffuse, 0.005f, 0f, 1f, "%.3f"))
            //            activeObject.SetModel(Object.Model.Diffuse, activeObjectIndex, currentDiffuse);

            //        if (ImGui.DragFloat("Specular", ref currentSpecular, 0.005f, 0f, 1f, "%.3f"))
            //            activeObject.SetModel(Object.Model.Specular, activeObjectIndex, currentSpecular);

            //        if (ImGui.Button("Choose Texture"))
            //        {
            //            string newPath = NativeFileDialog.OpenFileDialog();
            //            if (newPath is not null)
            //                object3D.AddTexture(newPath);
            //        }

            //        ImGui.SameLine();

            //        string briefTexturePath = object3D.TexturePath.Length > 20 ? "..." + object3D.TexturePath.Substring(object3D.TexturePath.Length - 20, 20) : object3D.TexturePath;
            //        ImGui.Text(briefTexturePath);
            //    }

            //    if (activeObject is Cube)
            //    {
            //        ImGui.Text("Bundle");

            //        ImGui.SameLine

            //    }




            //    if (activeObject is SpotLight spotLight)
            //    {
            //        if (ImGui.DragFloat("Cut off", ref curCutOff, 0.1f, -90f, curOutCutOff, "%.1f"))
            //            spotLight.SetSpecialData(activeObjectIndex, 2, 0, curCutOff);

            //        if (ImGui.DragFloat("Out cut off", ref curOutCutOff, 0.1f, curCutOff, 90f, "%.1f"))
            //            spotLight.SetSpecialData(activeObjectIndex, 2, 1, curOutCutOff);

            //        if (ImGui.DragFloat3("direction", ref currentDirection, 0.05f, -1f, 1f, "%.2f"))
            //            spotLight.SetDirection(activeObjectIndex, Transform.ToOpenTK(currentDirection));
            //    }


            //    if (ImGui.Button("Delete", new Vector2(100f, 50f)))
            //    {
            //        if (activeObject.Name == "Cube" && activeObject.Count == 1)
            //        {
            //            int ind = activeObject.Index;

            //            physicsManager.Objects.RemoveAt(activeObject.PhysicsIndex);
            //            cubes.RemoveAt(ind - 1);
            //            ind = 1;

            //            foreach(Cube cube in cubes)
            //                cube.ChangeIndex(ind++);
            //        }
            //        else activeObject.Delete(activeObjectIndex);

            //        activeObjectIndex = -1;

            //    }
            //}


            ImGui.SetCursorPosY(ImGui.GetWindowHeight() - 75);
             
            if (ImGui.Button("Run", new Vector2(100f, 50f)))
            {
                isRunning = true;
                firstMove = true;
            }
            ImGui.SameLine();

            if (ImGui.Button("Exit", new Vector2(100f, 50f)))
                window.Close();
            ImGui.SameLine();

            ImGui.BeginGroup();
            ImGui.Checkbox("Load after start", ref needToLoad);
            ImGui.Dummy(new Vector2(10, 0)); 
            ImGui.Checkbox("Save before close", ref needToSave);
            ImGui.EndGroup();

            ImGui.End();

            _controller.Render();
            ImGuiController.CheckGLError("End of frame");
        }

        public void Delete()
        {
            if (activeObject != null && activeObjectIndex >= 0) {
                if (activeObject.Count == 1 && activeObject.Index > 0)
                {
                    objectManager.Objects[activeObject.Name].RemoveAt(activeObject.Index);
                    activeObjectIndex = -1;
                    return;
                }
             
                activeObject.Delete(activeObjectIndex);
                activeObjectIndex -= 1;
            }
        }

        public void OnResize(int w, int h)
        {
            height = h;
            width = w;
            _controller.WindowResized(w, h);
        }
    }
}