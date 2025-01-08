using Engine.Camera;
using Engine.Lights;
using Engine.ToolBar;
using OpenTK.Mathematics;

namespace Engine.Objects
{
    internal abstract class DisembodiedObject(ObjectManager objectManager) : Object(objectManager)
    {
        protected List<Object?> objects = [];

        public Object? GetBindObject(int idx) => objects[idx];
        public override Vector3 GetPosition(int idx) => Vector3.Zero;
        public override Vector3 GetRotate(int idx)   => Vector3.Zero;
        public override Vector3 GetScale(int idx)    => Vector3.Zero;
        public override sealed bool GetPhysicsEnabled(int idx) => false;
        public override sealed void ChangePhysicsEnabled(int idx) { }

        public override sealed void Draw(BaseCamera camera, Light light, bool isPhysicsWorld = false) { }

        public override sealed Vector3 GetVertex(int objectIdx, int vertexIdx)
            => new(float.MaxValue);
        public override sealed void Update(int idx, float deltaTime) { }

        public override sealed void Undo() { }

    }
}
