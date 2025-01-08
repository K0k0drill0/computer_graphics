using Engine.Objects;
using Engine.ToolBar;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections;
using System.Linq;

namespace Engine.UserClass
{
    internal class UPlayer(ObjectManager objectManager, Window window) : Player(objectManager, window)
    {
        private bool isGrounded = false;
        private const float jumpForce = 10f;
        private float constant = 1f;
        private float acceleration = 0.01f;
        private float deceleration = 0.009f;
        private Vector3 maxSpeed = new(3f, 20f, 3f);

        public override void KeyboardInteractionControl(ref KeyboardState input, float deltaTime)
        {
            Vector3 targetVelocity = Vector3.Zero;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Front")))
                targetVelocity.Z += constant;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Back")))
                targetVelocity.Z -= constant;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Left")))
                targetVelocity.X += constant;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Right")))
                targetVelocity.X -= constant;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Up")) && isGrounded)
                velocity.Y = 15f;
                isGrounded = false;

            if (LengthSquared(targetVelocity) > 0)
                velocity += targetVelocity * acceleration;
            else if (LengthSquared(velocity) != 0)
                velocity -= velocity.Normalized() * deceleration;

            bool enableChange = true;

            if (physicsEnabled)
            {
                objectManager.Objects.Values.ToList().SelectMany(list => list).ToList().ForEach(secondObject =>
                {
                    if (secondObject is ObjectsInstance secondOI)
                    {
                        for (int j = 0; j < secondOI.Count; j++)
                        {
                            if (AABB.CheckCollision(this, GetVertex(0, 4), GetVertex(0, 0), secondOI.GetVertex(j, 4), secondOI.GetVertex(j, 0)))
                            {
                                enableChange = false;
                                isGrounded = true;
                                break;
                            }
                        }
                    }
                });
            }

            if (enableChange) CalculateVelocityY(deltaTime);

            if (velocity.X * velocity.X > maxSpeed.X * maxSpeed.X)
                velocity.X = velocity.Normalized().X * maxSpeed.X;

            if (velocity.Y * velocity.Y > maxSpeed.Y * maxSpeed.Y)
                velocity.Y = velocity.Normalized().Y * maxSpeed.Y;

            if (velocity.Z * velocity.Z > maxSpeed.Z * maxSpeed.Z)
                velocity.Z = velocity.Normalized().Z * maxSpeed.Z;

            Update(0, deltaTime);
        }

        private void CalculateVelocityY(float deltaTime)
        {
            float gravity = 9.81f;
            velocity.Y -= gravity * mass;
        }

        private static float LengthSquared(Vector3 vec)
            => vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z;

    }
}
