using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Engine.ToolBar
{
    internal class Stream
    {
        public static void PrintMatrix4(Matrix4 matrix)
        {
            Console.WriteLine(matrix.Column0.X + " " + matrix.Column0.Y + " " + matrix.Column0.Z + " " + matrix.Column0.W);
            Console.WriteLine(matrix.Column1.X + " " + matrix.Column1.Y + " " + matrix.Column1.Z + " " + matrix.Column1.W);
            Console.WriteLine(matrix.Column2.X + " " + matrix.Column2.Y + " " + matrix.Column2.Z + " " + matrix.Column2.W);
            Console.WriteLine(matrix.Column3.X + " " + matrix.Column3.Y + " " + matrix.Column3.Z + " " + matrix.Column3.W);
            Console.WriteLine();
        }

        public static void PrintVec3(Vector3 vector)
            => Console.WriteLine(vector.X + " " + vector.Y + " " + vector.Z);
        public static void ReadVector(out Vector3 vector, ref int index, ref string[] settings)
        {
            vector = new(float.Parse(settings[index++]),
                         float.Parse(settings[index++]),
                         float.Parse(settings[index++]));
        }

        public static void ReadVector(out Vector2 vector, ref int index, ref string[] settings)
        {
            vector = new(float.Parse(settings[index++]),
                         float.Parse(settings[index++]));
        }

        public static void WriteVector(StreamWriter writer, Vector3 vector)
            => writer.Write(" " + vector.X + " " + vector.Y + " " + vector.Z);

        public static void WriteVector(StreamWriter writer, Vector2 vector)
            => writer.Write(" " + vector.X + " " + vector.Y);

        public static void WriteSubList(StreamWriter writer, List<float> list, int start, int count)
        {
            for (int i = 0; i < count; i++) writer.Write(" " + list[start + i]);
        }
    }
}
