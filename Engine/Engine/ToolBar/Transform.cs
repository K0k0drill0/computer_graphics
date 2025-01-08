using OpenTK.Mathematics;

namespace Engine.ToolBar
{
    internal class Transform
    {
        public static void Matrix4ToArray(Matrix4 matrix, ref float[] array)
        {
            if (array.Length < 16) throw new Exception("Check Matrix4ToArray call: Array Lenght");

            array[0] = matrix.M11; array[1] = matrix.M21; array[2] = matrix.M31; array[3] = matrix.M41;
            array[4] = matrix.M12; array[5] = matrix.M22; array[6] = matrix.M32; array[7] = matrix.M42;
            array[8] = matrix.M13; array[9] = matrix.M23; array[10] = matrix.M33; array[11] = matrix.M43;
            array[12] = matrix.M14; array[13] = matrix.M24; array[14] = matrix.M34; array[15] = matrix.M44;
        }

        public static Vector4 ToOpenTK(System.Numerics.Vector4 vec) =>
            new(vec.X, vec.Y, vec.Z, vec.W);

        public static Vector3 ToOpenTK(System.Numerics.Vector3 vec) =>
            new(vec.X, vec.Y, vec.Z);

        public static Vector2 ToOpenTK(System.Numerics.Vector2 vec) =>
            new(vec.X, vec.Y);

        public static System.Numerics.Vector3 ToSystemNumerics(Vector3 vec) =>
            new(vec.X, vec.Y, vec.Z);

        public static System.Numerics.Vector2 ToSystemNumerics(Vector2 vec) =>
            new(vec.X, vec.Y);
    }
}
