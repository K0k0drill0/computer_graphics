using Engine.Camera;
using Engine.Objects;
using Engine.ToolBar;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTK.Mathematics;

namespace Engine.ToolBar
{
    internal class AABB
    {
        public enum CollisionSide
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            Front,
            Back
        }

        public static Vector3 GetCubeCorner(Vector3 translate, Vector3 scale, Vector3 corner)
        {
            return corner * scale + translate;
        }

        public static Vector3 GetRayDirection(Vector2 mousePosition, BaseCamera camera, int width, int height)
        {
            float x = 2f * mousePosition.X / width - 1f;
            float y = 1f - 2f * mousePosition.Y / height;

            Vector4 rayClip = new(x, y, -1f, 1f);

            Vector4 rayEye = rayClip * Matrix4.Invert(camera.GetProjectionMatrix());
            rayEye = new Vector4(rayEye.X, rayEye.Y, -1f, 0f);

            Vector3 rayWorld = (rayEye * Matrix4.Invert(camera.GetViewMatrix())).Xyz;
            rayWorld.Normalize();

            return rayWorld;
        }

        public static bool Intersects(Vector3 Min, Vector3 Max, Vector3 rayOrigin, Vector3 rayDirection)
        {
            if (rayDirection.X == 0 && (rayOrigin.X < Min.X || rayOrigin.X > Max.X)) return false;
            if (rayDirection.Y == 0 && (rayOrigin.Y < Min.Y || rayOrigin.Y > Max.Y)) return false;
            if (rayDirection.Z == 0 && (rayOrigin.Z < Min.Z || rayOrigin.Z > Max.Z)) return false;

            float tMin = (rayOrigin.X - Min.X) / rayDirection.X;
            float tMax = (rayOrigin.X - Max.X) / rayDirection.X;

            if (tMin > tMax) Swap(ref tMin, ref tMax);

            float tyMin = (rayOrigin.Y - Min.Y) / rayDirection.Y;
            float tyMax = (rayOrigin.Y - Max.Y) / rayDirection.Y;

            if (tyMin > tyMax) Swap(ref tyMin, ref tyMax);

            if (tMin > tyMax || tyMin > tMax)
                return false;

            if (tyMin > tMin) tMin = tyMin;
            if (tyMax < tMax) tMax = tyMax;

            float tzMin = (rayOrigin.Z - Min.Z) / rayDirection.Z;
            float tzMax = (rayOrigin.Z - Max.Z) / rayDirection.Z;

            if (tzMin > tzMax) Swap(ref tzMin, ref tzMax);

            if (tMin > tzMax || tzMin > tMax)
                return false;

            return true;
        }

        public static bool CheckCollision(Vector3 firstMin, Vector3 firstMax, Vector3 secondMin, Vector3 secondMax)
        {
            if (firstMax.X < secondMin.X || secondMax.X < firstMin.X)
                return false;

            if (firstMax.Y < secondMin.Y || secondMax.Y < firstMin.Y)
                return false;

            if (firstMax.Z < secondMin.Z || secondMax.Z < firstMin.Z)
                return false;

            return true;
        }

        public static bool CheckCollision(Player player, Vector3 firstMin, Vector3 firstMax, Vector3 secondMin, Vector3 secondMax)
        {
            Vector3 velocity = player.GetVelocity();
            Vector3 position = player.GetPosition();

            if (firstMax.X <= secondMin.X || secondMax.X <= firstMin.X ||
                firstMax.Y <= secondMin.Y || secondMax.Y <= firstMin.Y ||
                firstMax.Z <= secondMin.Z || secondMax.Z <= firstMin.Z) return false;

            Vector3 penetration = new Vector3(
                Math.Min(firstMax.X - secondMin.X, secondMax.X - firstMin.X),
                Math.Min(firstMax.Y - secondMin.Y, secondMax.Y - firstMin.Y),
                Math.Min(firstMax.Z - secondMin.Z, secondMax.Z - firstMin.Z)
            );

            if (penetration.X < penetration.Y && penetration.X < penetration.Z)
            {
                if (firstMax.X - secondMin.X < secondMax.X - firstMin.X)
                {
                    if (velocity.X > 0)
                    {
                        position.X -= firstMax.X - secondMin.X;
                        velocity.X = 0;
                    }
                }
                else if (velocity.X < 0)
                {
                    position.X += secondMax.X - firstMin.X;
                    velocity.X = 0;
                }
            }

            if (penetration.Y < penetration.X && penetration.Y < penetration.Z)
            {
                if (firstMax.Y - secondMin.Y < secondMax.Y - firstMin.Y)
                {
                    if (velocity.Y > 0)
                    {
                        position.Y -= firstMax.Y - secondMin.Y;
                        velocity.Y = 0;
                    }
                }
                else if (velocity.Y < 0)
                {
                    position.Y += secondMax.Y - firstMin.Y;
                    velocity.Y = 0;
                }
            }

            if (penetration.Z < penetration.X && penetration.Z < penetration.Y)
            {
                if (firstMax.Z - secondMin.Z < secondMax.Z - firstMin.Z)
                {
                    if (velocity.Z > 0)
                    {
                        position.Z -= firstMax.Z - secondMin.Z;
                        velocity.Z = 0;
                    }
                }
                else if (velocity.Z < 0)
                {
                    position.Z += secondMax.Z - firstMin.Z;
                    velocity.Z = 0;
                }
            }

            player.SetModel(Object.Model.Velocity, 0, velocity);
            player.SetModel(Object.Model.Position, 0, position);

            return true;
        }

        private static void Swap(ref float a, ref float b)
        {
            float temp = a;
            a = b;
            b = temp;
        }

        public static void HandleMouseClick(Vector2 mousePosition, BaseCamera camera, ObjectManager objectManager, HUD hud, int width, int height)
        {
            Vector3 rayOrigin = camera.Position;
            Vector3 rayDirection = GetRayDirection(mousePosition, camera, width, height);

            objectManager.Objects.Values.SelectMany(list => list).ToList().ForEach(obj =>
            {
                if (obj is FlatObjects flat)
                    for (int idx = 0; idx < obj.Count; idx++)
                    {
                        Vector3 leftTopCorner = (obj.GetVertex(idx, 0) + new Vector3(1f)) / 2;
                        Vector3 righBottomCorner = (obj.GetVertex(idx, 1) + new Vector3(1f)) / 2;

                        leftTopCorner.X *= width; 
                        leftTopCorner.Y *= height;
                        righBottomCorner.X *= width;
                        righBottomCorner.Y *= height;

                        if (leftTopCorner.Y > height - mousePosition.Y && height - mousePosition.Y > righBottomCorner.Y &&
                            leftTopCorner.X < mousePosition.X && mousePosition.X < righBottomCorner.X)
                        {
                            hud.FocusObj(obj, idx);
                            return;
                        }

                    }
            });

            objectManager.Objects.Values.SelectMany(list => list).ToList().ForEach(obj =>
            {
                if (obj is not FlatObjects) 
                    for (int idx = 0; idx < obj.Count; idx++)
                    {
                        if (obj.GetVertex(idx, 0) != new Vector3(float.MaxValue))
                        {
                            if (Intersects(obj.GetVertex(idx, 0), obj.GetVertex(idx, 4), rayOrigin, rayDirection))
                            {
                                hud.FocusObj(obj, idx);
                                return;
                            }
                        }
                    }
            });
        }
    }
}
