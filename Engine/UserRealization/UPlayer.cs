using 

namespace UserRealization
{
    internal class UPlayer : Player
    {
        public UPlayer(ObjectManager objectManager, Window window) : base(objectManager, window)
        {
            ManagerUpdate(this, Name);
        }
        public override void KeyboardInteractionControl(ref KeyboardState input, float deltaTime)
        {
            if (input.IsKeyDown(KeyboardManager.GetButton("User Front")))
                position += Vector3.UnitZ * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Back")))
                position -= Vector3.UnitZ * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Left")))
                position -= Vector3.UnitX * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Right")))
                position += Vector3.UnitX * deltaTime;

            if (input.IsKeyDown(KeyboardManager.GetButton("User Up")))
                position += Vector3.UnitY * deltaTime;

        }
    }
}
