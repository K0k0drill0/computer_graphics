using OpenTK.Mathematics;

namespace Engine.ToolBar
{
    internal class Common
    {
        public static void CalculateModel(ref float[] array, Vector3 translate, Vector3 scale, Vector3 rotate)
            => Transform.Matrix4ToArray(CalculateModel(translate, scale, rotate), ref array);

        public static void CalculateModel(ref float[] array, Matrix4 matrix)
            => Transform.Matrix4ToArray(matrix, ref array);

        public static Matrix4 CalculateModel(Vector3 translate, Vector3 scale, Vector3 rotate)
        {
            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateScale(scale);
            model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotate.X));
            model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotate.Y));
            model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotate.Z));
            model *= Matrix4.CreateTranslation(translate);

            return model;
        }
    }
}
