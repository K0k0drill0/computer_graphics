using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Camera
{
    internal abstract class BaseCamera
    {
        public virtual bool IsOrtho { get; set; }

        public float cameraSpeed = 4.5f, sensitivity = 0.2f, near = 0.1f, far = 100f;
        public Vector2 lastPos;

        protected Vector3 up = Vector3.UnitY,
                          right = Vector3.UnitX,
                          front = -Vector3.UnitZ;

        protected float pitch,
                        yaw = -MathHelper.PiOver2,
                        fov = MathHelper.PiOver2;

        public Vector3 position;

        public virtual Vector3 Position
        {
            get => position;
            set => position = value;
        }
        public virtual float Width { protected get; set; }
        public virtual float Height { protected get; set; }

        public virtual Vector3 Front => front;
        public virtual Vector3 Right => right;
        public virtual Vector3 Up => up;

        public virtual float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public virtual float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public virtual float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public virtual Matrix4 GetViewMatrix()
            => Matrix4.LookAt(Position, Position + front, up);

        public virtual Matrix4 GetProjectionMatrix()
        {
            return IsOrtho ? Matrix4.CreateOrthographic(19.8f, 10.8f, 0.1f, 100f)
                           : Matrix4.CreatePerspectiveFieldOfView(fov, Width / Height, near, far);
        }

        protected virtual void UpdateVectors()
        {
            front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
            front.Y = MathF.Sin(pitch);
            front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);

            front = Vector3.Normalize(front);
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        public virtual void KeyboardInteractionControl(ref KeyboardState input, float deltaTime)
        {
            if (input.IsKeyDown(KeyboardManager.GetButton("Engine camera Front")))
                Position += Front * cameraSpeed * deltaTime; // Forward

            if (input.IsKeyDown(KeyboardManager.GetButton("Engine camera Back")))
                Position -= Front * cameraSpeed * deltaTime; // Backwards

            if (input.IsKeyDown(KeyboardManager.GetButton("Engine camera Left")))
                Position -= Right * cameraSpeed * deltaTime; // Left

            if (input.IsKeyDown(KeyboardManager.GetButton("Engine camera Right")))
                Position += Right * cameraSpeed * deltaTime; // Right

            if (input.IsKeyDown(KeyboardManager.GetButton("Engine camera Up")))
                Position += Up * cameraSpeed * deltaTime;    // Up

            if (input.IsKeyDown(KeyboardManager.GetButton("Engine camera Down")))
                Position -= Up * cameraSpeed * deltaTime;    // Down
        }

        public virtual void MouseInteractionControl(ref MouseState mouse, ref bool firstMove)
        {
            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                Yaw += deltaX * sensitivity;
                Pitch -= deltaY * sensitivity;
            }
        }

        public (float Width, float Height, float Near, float Far) GetFrustum()
        {
            float tang = (float)Math.Tan(MathHelper.DegreesToRadians(Fov) / 2.0f);
            float height = near * tang;
            float width = height * Width / Height;
            
            return (Width: width, Height: height, Near: near, Far: far);
        }
    }
}
