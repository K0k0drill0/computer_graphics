using Engine.Camera;
using Engine.Objects.Objects3D;
using Engine.Lights;
using OpenTK.Mathematics;
using Engine.Objects;
using Engine.ToolBar;
using System.Runtime.Serialization;
using System.Xml.Linq;
using OpenTK.Graphics.ES20;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine
{
    internal class ObjectManager
    {
        public bool keyIsExpected;
        public Keys key;

        public Dictionary<string, (Type ObjectType, bool IsActive)> RegistratedObjects;
        public Dictionary<string , List<Object>> Objects { get; private set; }
        public int Count { get => Objects.Count; }

        int height, width;

        public ObjectManager(int height, int width)
        {
            this.height = height;
            this.width = width;

            Objects = [];
            RegistratedObjects = [];
        }

        public void Registrate(Type type, string name)
        {
            if (!RegistratedObjects.ContainsKey(name))
                RegistratedObjects.Add(name, new(type, false));
        }

        public void Add(Object obj, string name)
        {
            if (!RegistratedObjects.ContainsKey(name))
            {
                throw new Exception(name + " isn't registered.");
            }
            
            if (!Objects.ContainsKey(name)) Objects.Add(name, []);
            Objects[name].Add(obj);

            if (!RegistratedObjects[name].IsActive)
            {
                (Type ObjectType, _) = RegistratedObjects[name];
                RegistratedObjects[name] = (ObjectType, true);
            }
        }

        public Object Add(string name)
        {
            return name switch
            {
                "Cube" => new Cube(this),
                _ => throw new Exception("Check type"),
            };
        }

        public void Remove(string name, int index = -1)
        {
            if (index >= 0 && Objects.ContainsKey(name))
            {
                Objects[name].RemoveAt(index);
            }

            if (Objects[name].Count == 0)
            {
                (Type ObjectType, _) = RegistratedObjects[name];
                RegistratedObjects[name] = (ObjectType, false);

                Objects.Remove(name);
            }
        }

        public void Update(float deltaTime)
        {
            Objects.Values.SelectMany(list => list).ToList().ForEach(firstObject =>
            {
                if (firstObject is ObjectsInstance firstOI)
                {
                    for (int i = 0; i < firstOI.Count; i++)
                    {
                        if (!firstObject.GetPhysicsEnabled(i)) continue;
                        bool enableChange = true;

                        Objects.Values.SelectMany(list => list).ToList().ForEach(secondObject =>
                        {
                            if (secondObject is ObjectsInstance secondOI)
                            {
                                for (int j = 0; j < secondOI.Count; j++)
                                {
                                    if (secondOI.Count > 1)
                                    {
                                        if (firstObject == secondObject && i == j) continue;

                                        if (AABB.CheckCollision(firstOI.GetVertex(i, 4), firstOI.GetVertex(i, 0), secondOI.GetVertex(j, 4), secondOI.GetVertex(j, 0)))
                                        {
                                            enableChange = false;
                                            firstObject.SetModel(Object.Model.Velocity, i, Vector3.Zero);
                                            return;
                                        }
                                    }
                                }
                            }
                        });

                        if (enableChange)
                        {
                            firstOI.SetModel(Object.Model.Velocity, i, calculateVelocityY(firstOI.GetVelocity(i)));
                            firstObject.Update(i, deltaTime);
                        }
                    }
                }
            });
        }

        private Vector3 calculateVelocityY (Vector3 velocity)
        {
            //float eps = -velocity.Y / 9.8f + 1f;

            //float res = Math.Min(9.8f, (float)Math.Exp((double)eps + 0.5));

            return (velocity.X, velocity.Y - 0.01f, velocity.Z);
        }

        //private bool Collision(Object firstObj, int fIdx, Object secondObj, int sIdx)
        //{
        //    float epsilon = 0.01f;

        //    if (firstObj is Object3D object3D && secondObj is Surface terrain)
        //    {
        //        Vector3 obj3DMinVertex = object3D.GetMinVertex(fIdx);

        //        if (!Intersect(terrain, sIdx, obj3DMinVertex.X, obj3DMinVertex.Z)) return false;
        //        else if (obj3DMinVertex.Y - terrain.GetVertex(sIdx, obj3DMinVertex.X, obj3DMinVertex.Z).Y < epsilon) return true;
        //    }

        //    return false;
        //}

        //private bool Intersect(Surface surface, int idx, float x, float z)
        //    => surface.Intersect(idx, x, z);

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(Window.FilePath))
            {
                foreach (var list in Objects.Values)
                    foreach (var obj in list) obj.Save(writer);
            }
        }
        public void Draw(BaseCamera camera, Light light, bool isPhysicsWorld)
        {
            foreach (var list in Objects.Values)
                foreach (var obj in list) obj.Draw(camera, light, isPhysicsWorld);
        }
        public void Undo()
        {
            foreach (var list in Objects.Values)
                foreach (var obj in list) obj.Undo();
        }

        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;

            foreach (var obj in Objects.Values)
            {
                if (obj[0] is FlatObjects flat)
                {
                    flat.Resize(width, height);
                }
            }
        }
    }
}
