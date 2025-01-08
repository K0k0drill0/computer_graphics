using OpenTK.Graphics.OpenGL4;
using System.Reflection.Metadata;

namespace Engine
{
    internal class Shadow
    {
        int Handle;
        int FBO;

        public Shadow()
        {
            FBO = GL.GenFramebuffer();
            Load();

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            CreateDepthBuffer();
            CreateFrameBufferTexture(ref Handle);

            Unload();
        }

        public void CreateDepthBuffer()
        {
            int depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32, 1920, 1080);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

            CheckProgress();
        }

        private void CreateFrameBufferTexture(ref int handle)
        {
            handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, 1920, 1080, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, handle, 0);

            CheckProgress();
        }

        private void CheckProgress() 
        {
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Console.WriteLine("Framebuffer is incomplete!");
        }

        public void Load() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        public void Unload() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        public int Use()
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            return 1;
        }
    }
}
