using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace Engine.Camera
{
    internal class GameCamera : BaseCamera
    {

        Player player;
        public bool yawEnabled,
                    pitchEnabled,
                    rotateEnabled,

                    xAxisMovementEnabled,
                    yAxisMovementEnabled,
                    zAxisMovementEnabled,
                    movementEnabled,

                    Keyboard_FrontBackMovement_Enabled,
                    Keyboard_RightLeftMovement_Enabled,
                    Keyboard_UpDownMovement_Enabled,

                    firstFrame = true;

        Stopwatch stopwatch;

        public bool RotateEnabled
        {
            set
            {
                yawEnabled = value;
                pitchEnabled = value;
                rotateEnabled = value;
            }
        }

        public bool MovementEnabled
        {
            set
            {
                xAxisMovementEnabled = value;
                yAxisMovementEnabled = value;
                zAxisMovementEnabled = value;
                movementEnabled = value;
            }
        }

        public GameCamera(Player player, Vector3 position, int width, int height)
        {
            this.player = player;
            Position = position;
            Width = width;
            Height = height;

            UpdateVectors();

            stopwatch = new Stopwatch();

            IsOrtho = false;

            yawEnabled = false;
            pitchEnabled = false;

            xAxisMovementEnabled = true;
            yAxisMovementEnabled = false;
            zAxisMovementEnabled = true;

            Keyboard_FrontBackMovement_Enabled = false;
            Keyboard_RightLeftMovement_Enabled = false;
            Keyboard_UpDownMovement_Enabled = false;
        }

        public override float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                if (pitchEnabled)
                {
                    var angle = MathHelper.Clamp(value, -89f, 89f);
                    pitch = MathHelper.DegreesToRadians(angle);
                    UpdateVectors();
                }
            }
        }

        public override float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                if (yawEnabled)
                {
                    yaw = MathHelper.DegreesToRadians(value);
                    UpdateVectors();
                }

            }
        }

        public override Vector3 Position
        {
            get => position;
            set
            {
                if (xAxisMovementEnabled) position.X = value.X;
                if (yAxisMovementEnabled) position.Y = value.Y;
                if (zAxisMovementEnabled) position.Z = value.Z;
            }
        }

        public override void KeyboardInteractionControl(ref KeyboardState input, float deltaTime)
        {
            if (input.IsKeyDown(KeyboardManager.GetButton("Game camera Front")))
                if (!Keyboard_FrontBackMovement_Enabled) alternateMoveZ(Front * cameraSpeed * deltaTime, Keys.W);
                else Position += Front * cameraSpeed * deltaTime; // Forward

            if (input.IsKeyDown(KeyboardManager.GetButton("Game camera Back")))
                if (!Keyboard_FrontBackMovement_Enabled) alternateMoveZ(-Front * cameraSpeed * deltaTime, Keys.S);
                else Position -= Front * cameraSpeed * deltaTime; // Backwards

            if (input.IsKeyDown(KeyboardManager.GetButton("Game camera Left")))
                if (!Keyboard_RightLeftMovement_Enabled) alternateMoveX(Right * cameraSpeed * deltaTime, Keys.A);
                else Position -= Right * cameraSpeed * deltaTime; // Left

            if (input.IsKeyDown(KeyboardManager.GetButton("Game camera Right")))
                if (!Keyboard_RightLeftMovement_Enabled) alternateMoveX(-Right * cameraSpeed * deltaTime, Keys.D);
                else Position += Right * cameraSpeed * deltaTime; // Right

            if (input.IsKeyDown(KeyboardManager.GetButton("Game camera Up")))
                if (!Keyboard_UpDownMovement_Enabled) alternateMoveY(Up * cameraSpeed * deltaTime, Keys.Space);
                else Position += Up * cameraSpeed * deltaTime;    // Up

            if (input.IsKeyDown(KeyboardManager.GetButton("Game camera Down")))
                if (!Keyboard_UpDownMovement_Enabled) alternateMoveY(Up * cameraSpeed * deltaTime, Keys.LeftShift);
                else Position -= Up * cameraSpeed * deltaTime;    // Down

            position.Y = player.GetPosition().Y + 10f;
            UpdateVectors();

            if (firstFrame)
            {
                if (!Keyboard_RightLeftMovement_Enabled) alternateMoveX(-Right * cameraSpeed * deltaTime, Keys.D);
                firstFrame = false;
            }
        }

        private void alternateMoveX(Vector3 move, Keys key)
        {
            if (!stopwatch.IsRunning) stopwatch.Start();
            else if (stopwatch.ElapsedMilliseconds < 300) return;
            else stopwatch.Restart();
            
            UpdateButtons(key);

            const float radius = 15f;

            yaw += key == Keys.D ? MathHelper.PiOver2 : -MathHelper.PiOver2;

            float x = radius * (float)Math.Cos(yaw);
            float z = radius * (float)Math.Sin(yaw);

            Position = new Vector3(x, position.Y, z);
        }

        private void alternateMoveY(Vector3 move, Keys key) { }
        private void alternateMoveZ(Vector3 move, Keys key) { }

        protected override void UpdateVectors()
        {
            front = Vector3.Zero - Position;

            front = Vector3.Normalize(front);
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        protected void UpdateButtons(Keys key)
        {
            Keys front = KeyboardManager.GetButton("User Front");
            Keys back = KeyboardManager.GetButton("User Back");
            Keys left = KeyboardManager.GetButton("User Left");
            Keys right = KeyboardManager.GetButton("User Right");

            if (key == Keys.D)
            {
                KeyboardManager.SetButton("User Front", left);
                KeyboardManager.SetButton("User Back", right);
                KeyboardManager.SetButton("User Left", back);
                KeyboardManager.SetButton("User Right", front);
            }
            else
            {
                KeyboardManager.SetButton("User Front", right);
                KeyboardManager.SetButton("User Back", left);
                KeyboardManager.SetButton("User Left", front);
                KeyboardManager.SetButton("User Right", back);
            }
        }
    }
}
