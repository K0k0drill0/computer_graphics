using Engine.Camera;
using Engine.Lights;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Objects
{
    internal abstract class FlatObjects(ObjectManager objectManager) : Object(objectManager)
    {
        protected float windowWidth, windowHeight;

        public void Resize(float width, float height)
        {
            windowWidth = width;
            windowHeight = height;
        }

        public abstract void LoadFiles(string filename, string texPath);

        public override sealed void ChangePhysicsEnabled(int idx)
            => throw new NotImplementedException();

        public override sealed bool GetPhysicsEnabled(int idx) => false;

        public override sealed void Undo() { }

        public override sealed void Update(int idx, float deltaTime)
            => throw new NotImplementedException();
    }
}
