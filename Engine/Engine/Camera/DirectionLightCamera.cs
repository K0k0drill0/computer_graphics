using OpenTK.Mathematics;
using Engine.Lights;

namespace Engine.Camera
{
    internal class DirectionLightCamera : BaseCamera
    {
        Vector3 Target { get; set; }
        const float Accuracy = 100f;

        public DirectionLightCamera(BaseCamera camera, Vector3 direction)
        {
            var frustum = camera.GetFrustum();

            Width = frustum.Width * Accuracy;
            Height = frustum.Height * (Accuracy * 1.2f);
            near = frustum.Near;
            far = frustum.Far;

            Target = Vector3.Zero;
            Position = Vector3.Zero - direction * 40;

            front = Vector3.Normalize(Target - Position);
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        public override Matrix4 GetViewMatrix()
            => Matrix4.LookAt(Position, Target, up);

        public override Matrix4 GetProjectionMatrix()
            //=> Matrix4.CreateOrthographicOffCenter(-Width, Width, -Height, Height, near, 100f);
            => Matrix4.CreateOrthographic(Width, Height, near, far);
    }
}
