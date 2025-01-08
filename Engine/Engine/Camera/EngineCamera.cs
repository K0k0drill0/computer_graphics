using OpenTK.Mathematics;

namespace Engine.Camera
{
    internal class EngineCamera : BaseCamera
    {
        public EngineCamera(Vector3 position, int width, int height)
        {
            Position = position;
            Width = width;
            Height = height;

            UpdateVectors();

            IsOrtho = false;
        }
    }
}
