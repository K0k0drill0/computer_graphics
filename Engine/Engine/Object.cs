using Engine.Camera;
using Engine.Lights;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine
{
    internal abstract class Object
    {
        public enum Axes
        {
            X, Y, Z
        }

        public enum Model
        {
            Model,

            Position,
            Rotate,
            Scale,

            physicsEnable,
            Velocity,
            Mass,

            Color,
            Material,

            Direction,

            Arguments,
        };

        public abstract string Name { get; }
        public abstract int Count { get; }
        public int Index { get; protected set; }

        public static string StaticName => throw new NotImplementedException();


        protected ObjectManager objectManager;
        protected Object(ObjectManager objectManager) => this.objectManager = objectManager;
        protected void ManagerUpdate(Object obj, string name)
        {
            obj.Index = objectManager.Objects.ContainsKey(name) ? objectManager.Objects[name].Count : 0;
            objectManager.Add(obj, name);
        }


        public abstract void Add(string name);
        public abstract void Draw(BaseCamera camera, Light light, bool isPhysicsWorld);
        public abstract void Update(int idx, float deltaTime);
        public abstract void Undo();
        public abstract void Delete(int idx);

        public abstract bool GetActive(int idx);
        public abstract bool GetPhysicsEnabled(int idx);
        public abstract void ChangePhysicsEnabled(int idx);

        public abstract Vector3 GetVertex(int objectIdx, int vertexIdx);
        public abstract Vector3 GetColor(int idx);
        public abstract Vector3 GetPosition(int idx);
        public abstract Vector3 GetRotate(int idx);
        public abstract Vector3 GetScale(int idx);

        public abstract void SetModel(Model model, int idx, Vector3 value);

        public abstract void ShowNativeDataDialog(ref int idx);

        public abstract void Save(StreamWriter writer);

    }
}
