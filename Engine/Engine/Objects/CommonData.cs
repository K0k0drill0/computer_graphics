using OpenTK.Mathematics;

namespace Engine.Objects
{
    internal class CommonData
    {
        public readonly static float[] CubeVertices =
        {
             // Position          // Normal              // Textures
             0.5f,  0.5f,  0.5f,   0.0f,  0.0f,  1.0f,   0f, 0f, // Front face
            -0.5f,  0.5f,  0.5f,   0.0f,  0.0f,  1.0f,   1f, 0f,
            -0.5f, -0.5f,  0.5f,   0.0f,  0.0f,  1.0f,   1f, 1f,
            -0.5f, -0.5f,  0.5f,   0.0f,  0.0f,  1.0f,   1f, 1f,
             0.5f, -0.5f,  0.5f,   0.0f,  0.0f,  1.0f,   0f, 1f,
             0.5f,  0.5f,  0.5f,   0.0f,  0.0f,  1.0f,   0f, 0f,

            -0.5f, -0.5f, -0.5f,   0.0f,  0.0f, -1.0f,   1f, 0f, // Back face
            -0.5f,  0.5f, -0.5f,   0.0f,  0.0f, -1.0f,   1f, 1f,
             0.5f,  0.5f, -0.5f,   0.0f,  0.0f, -1.0f,   0f, 1f,
             0.5f, -0.5f, -0.5f,   0.0f,  0.0f, -1.0f,   0f, 0f,
            -0.5f, -0.5f, -0.5f,   0.0f,  0.0f, -1.0f,   1f, 0f,
             0.5f,  0.5f, -0.5f,   0.0f,  0.0f, -1.0f,   0f, 1f,

            -0.5f,  0.5f,  0.5f,  -1.0f,  0.0f,  0.0f,   1f, 0f, // Left face
            -0.5f,  0.5f, -0.5f,  -1.0f,  0.0f,  0.0f,   0f, 0f,
            -0.5f, -0.5f, -0.5f,  -1.0f,  0.0f,  0.0f,   0f, 1f,
            -0.5f, -0.5f, -0.5f,  -1.0f,  0.0f,  0.0f,   0f, 1f,
            -0.5f, -0.5f,  0.5f,  -1.0f,  0.0f,  0.0f,   1f, 1f,
            -0.5f,  0.5f,  0.5f,  -1.0f,  0.0f,  0.0f,   1f, 0f,

             0.5f,  0.5f,  0.5f,   1.0f,  0.0f,  0.0f,   1f, 1f, // Right face
             0.5f, -0.5f, -0.5f,   1.0f,  0.0f,  0.0f,   0f, 0f,
             0.5f,  0.5f, -0.5f,   1.0f,  0.0f,  0.0f,   0f, 1f,
             0.5f, -0.5f,  0.5f,   1.0f,  0.0f,  0.0f,   1f, 0f,
             0.5f, -0.5f, -0.5f,   1.0f,  0.0f,  0.0f,   0f, 0f,
             0.5f,  0.5f,  0.5f,   1.0f,  0.0f,  0.0f,   1f, 1f,

            -0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,   1f, 0f, // Bottom face
             0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,   1f, 1f,
             0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f,   0f, 1f,
             0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f,   0f, 1f,
            -0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f,   0f, 0f,
            -0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,   1f, 0f,

            -0.5f,  0.5f, -0.5f,   0.0f,  1.0f,  0.0f,   0f, 0f, // Top face
             0.5f,  0.5f,  0.5f,   0.0f,  1.0f,  0.0f,   1f, 1f,
             0.5f,  0.5f, -0.5f,   0.0f,  1.0f,  0.0f,   1f, 0f,
            -0.5f,  0.5f,  0.5f,   0.0f,  1.0f,  0.0f,   0f, 1f,
             0.5f,  0.5f,  0.5f,   0.0f,  1.0f,  0.0f,   1f, 1f,
            -0.5f,  0.5f, -0.5f,   0.0f,  1.0f,  0.0f,   0f, 0f,
        };

        public readonly static float[] PyramidVertices = {
            // Position          // Normal              // Texture Coordinates
            -0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,   1f, 0f, // Bottom face
             0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,   1f, 1f,
             0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f,   0f, 1f,
             0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f,   0f, 1f,
            -0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f,   0f, 0f,
            -0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,   1f, 0f,

            -0.5f,  -.5f, -0.5f,   0.0f,  0.5f,  0.0f,   0f, 0f, // Left face
             0.0f,  0.5f,  0.0f,   0.0f,  0.5f,  0.0f,  .5f,.5f,
             0.5f,  -.5f, -0.5f,   0.0f,  0.5f,  0.0f,   1f, 0f,

             0.5f,  -.5f, -0.5f,   0.0f,  0.5f,  0.0f,   1f, 0f, // Right face
             0.0f,  0.5f,  0.0f,   0.0f,  0.5f,  0.0f,  .5f,.5f,
             0.5f,  -.5f,  0.5f,   0.0f,  0.5f,  0.0f,   1f, 1f,

             0.5f,  -.5f,  0.5f,   0.0f,  0.5f,  0.0f,   1f, 1f, // Back face
             0.0f,  0.5f,  0.0f,   0.0f,  0.5f,  0.0f,  .5f,.5f,
            -0.5f,  -.5f,  0.5f,   0.0f,  0.5f,  0.0f,   0f, 1f,

            -0.5f,  -.5f,  0.5f,   0.0f,  0.5f,  0.0f,   0f, 1f, // Front face
             0.0f,  0.5f,  0.0f,   0.0f,  0.5f,  0.0f,  .5f,.5f,
            -0.5f,  -.5f, -0.5f,   0.0f,  0.5f,  0.0f,   0f, 0f,
        };


        public static float[] SkyBoxVertices =
        {
             // Position
             0.5f,  0.5f,  0.5f,    // Front face
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,

            -0.5f, -0.5f, -0.5f,    // Back face
            -0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,

            -0.5f,  0.5f,  0.5f,    // Left face
            -0.5f,  0.5f, -0.5f,   
            -0.5f, -0.5f, -0.5f,   
            -0.5f, -0.5f, -0.5f,   
            -0.5f, -0.5f,  0.5f,   
            -0.5f,  0.5f,  0.5f,   

             0.5f,  0.5f,  0.5f,    // Right face
             0.5f, -0.5f, -0.5f,   
             0.5f,  0.5f, -0.5f,   
             0.5f, -0.5f,  0.5f,   
             0.5f, -0.5f, -0.5f,   
             0.5f,  0.5f,  0.5f,   

            -0.5f, -0.5f, -0.5f,    // Bottom face
             0.5f, -0.5f, -0.5f,   
             0.5f, -0.5f,  0.5f,   
             0.5f, -0.5f,  0.5f,   
            -0.5f, -0.5f,  0.5f,   
            -0.5f, -0.5f, -0.5f,   

            -0.5f,  0.5f, -0.5f,    // Top face
             0.5f,  0.5f,  0.5f,   
             0.5f,  0.5f, -0.5f,   
            -0.5f,  0.5f,  0.5f,   
             0.5f,  0.5f,  0.5f,   
            -0.5f,  0.5f, -0.5f,   
        };

        public static Vector3[] CubeCornerns = [
            new ( 0.5f,  0.5f,  0.5f),
            new (-0.5f,  0.5f,  0.5f),
            new ( 0.5f, -0.5f,  0.5f),
            new ( 0.5f,  0.5f, -0.5f),
            new (-0.5f, -0.5f, -0.5f),
            new ( 0.5f, -0.5f, -0.5f),
            new (-0.5f,  0.5f, -0.5f),
            new (-0.5f, -0.5f,  0.5f),
        ];

        public static float[] GUIPlane = [
             //Coord     //Tex
             -1f,  1f,   0f, 1f,
             -1f, -1f,   0f, 0f,
              1f,  1f,   1f, 1f,
              1f, -1f,   1f, 0f
        ];
    }
}
