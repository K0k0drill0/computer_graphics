using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Shaders
{
    internal class Shader
    {
        private int Handle;
        private Dictionary<string, int> uniforms;

        public Shader(string vertPath, string fragPath)
        {
            var shaderSource = File.ReadAllText(vertPath);
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            CompileShader(vertexShader);

            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            uniforms = [];

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int uniformCount);

            for (int i = 0; i < uniformCount; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);

                uniforms.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);
            if (code != (int)All.True)
                throw new Exception($"Error occurred whilst compiling Shader({shader})");
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);
            if (code != (int)All.True)
                throw new Exception($"Error occurred whilst linking Program({program})");
        }

        public void Use()
            => GL.UseProgram(Handle);

        public void BindAttribLocation(string name, int size, VertexAttribPointerType vertexAttribPointerType, bool normalized, int stride, int offset, bool isInstance = false, int frequency = 1)
        {
            var attrib = GetAttribLocation(name);
            GL.VertexAttribPointer(attrib, size, vertexAttribPointerType, normalized, stride, offset);
            GL.EnableVertexAttribArray(attrib);
            if (isInstance) GL.VertexAttribDivisor(attrib, frequency);
        }

        public int GetAttribLocation(string name)
            => GL.GetAttribLocation(Handle, name);

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(uniforms[name], true, ref matrix);
        }
        public void SetVector3(string name, Vector3 vector)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(uniforms[name], vector);
        }

        public void SetFloat(string name, float num)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniforms[name], num);
        }

        public void SetInt(string name, int num)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniforms[name], num);
        }

        public void SetBool(string name, bool value)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniforms[name], value ? 1 : 0);
        }
    }
}
