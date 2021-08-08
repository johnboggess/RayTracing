using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

using ObjectTK.Textures;
using ObjectTK.Buffers;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using ObjectTK.Shaders;

namespace RayTracing
{
    public class Window : GameWindow
    {
        public float AspectRatio;

        //Shader _rayTracerShader;
        RayTracer RayTracer;
        RenderProgram RenderProgram;
        Texture2D Background;
        Texture2D Noise;
        Texture2D RayTracerOut;

        VertexArray _vertexArray;

        Camera Camera = new Camera(1f, 16f / 9f, 1);
        float speed = .1f;
        Vector2 lastMousePosition = Vector2.Zero;
        Stopwatch stopwatch = new Stopwatch();
        Random random = new Random();

        public Window(int height, float aspectRatio, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i((int)(height * aspectRatio), height), Title = title } )
        {
            log4net.Config.BasicConfigurator.Configure();
            Run();
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);

            lastMousePosition = MouseState.Position;

            RayTracerOut = new Texture2D(SizedInternalFormat.Rgba8, Size.X, Size.Y);
            RayTracerOut.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            RayTracerOut.Bind(TextureUnit.Texture0);

            int noiseWidth = 101;//texture shouldnt be square. This will allow the shader to easly sample every pixel
            int noiseHeight = 100;
            byte[] noiseBytes = new byte[noiseWidth * noiseHeight * 4];
            random.NextBytes(noiseBytes);
            GCHandle handle = GCHandle.Alloc(noiseBytes, GCHandleType.Pinned);
            IntPtr noisePtr = handle.AddrOfPinnedObject();
            Bitmap bmp = new Bitmap(noiseWidth, noiseHeight, noiseWidth * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, noisePtr);
            handle.Free();
            bmp.Save("Noise.bmp");
            BitmapTexture.CreateCompatible(bmp, out Noise);
            Noise.LoadBitmap(bmp);
            bmp.Dispose();
            Noise.Bind(TextureUnit.Texture2);

            Bitmap bckgrnd = new Bitmap("Background.png");
            bckgrnd.RotateFlip(RotateFlipType.RotateNoneFlipY);
            BitmapTexture.CreateCompatible(bckgrnd, out Background);
            Background.LoadBitmap(bckgrnd);
            bckgrnd.Dispose();
            RayTracerOut.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            Background.Bind(TextureUnit.Texture1);

            ProgramFactory.BasePath = "";

            RayTracer = ProgramFactory.Create<RayTracer>();
            RayTracer.Use();
            RayTracer.destTex.Bind(0, RayTracerOut, TextureAccess.WriteOnly);
            RayTracer.backgroundTex.Bind(1, Background, TextureAccess.ReadOnly);
            RayTracer.Noise.Bind(2, Noise, TextureAccess.ReadOnly);
            RayTracer.BackgroundWidth.Set(4096);
            RayTracer.BackgroundHeight.Set(2048);
            RayTracer.WindowWidth.Set(Size.X);
            RayTracer.WindowHeight.Set(Size.Y);

            RayTracer.ViewPortWidth.Set(Camera.ViewPortWidth);
            RayTracer.ViewPortHeight.Set(Camera.ViewPortHeight);

            RenderProgram = ProgramFactory.Create<RenderProgram>();
            RenderProgram.Use();
            RayTracerOut.Bind(TextureUnit.Texture0);
            RenderProgram.Texture.Set(TextureUnit.Texture0);

            Vector3[] vertices = new Vector3[] { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0) };
            Buffer<Vector3> vertexBuffer = new Buffer<Vector3>();
            vertexBuffer.Init(BufferTarget.ArrayBuffer, vertices);

            Vector2[] uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
            Buffer<Vector2> uvBuffer = new Buffer<Vector2>();
            uvBuffer.Init(BufferTarget.ArrayBuffer, uv);

            Buffer<int> indexBuffer = new Buffer<int>();
            indexBuffer.Init(BufferTarget.ElementArrayBuffer, new int[] { 0, 1, 2, 3, 2, 1 });

            _vertexArray = new VertexArray();
            _vertexArray.Bind();
            _vertexArray.BindAttribute(RenderProgram.InPosition, vertexBuffer);
            _vertexArray.BindAttribute(RenderProgram.InUV, uvBuffer);
            _vertexArray.BindElementBuffer(indexBuffer);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Vector3 vel = new Vector3(0, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.A))
                vel += -Camera.Transform.Right;
            if (KeyboardState.IsKeyDown(Keys.D))
                vel += Camera.Transform.Right;
            if (KeyboardState.IsKeyDown(Keys.S))
                vel += -Camera.Transform.Forward;
            if (KeyboardState.IsKeyDown(Keys.W))
                vel += Camera.Transform.Forward;

            if (vel.LengthSquared > 0)
                vel = vel.Normalized() * speed;
            Camera.Transform.Position += vel;

            float diffX = (MousePosition.X - lastMousePosition.X) / 16;
            float diffY = (MousePosition.Y - lastMousePosition.Y) / 16;
            lastMousePosition.X = MousePosition.X;
            lastMousePosition.Y = MousePosition.Y;

            Camera.Transform.Rotation = Quaternion.FromAxisAngle(Camera.Transform.Right, diffY) * Camera.Transform.Rotation;
            Camera.Transform.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, diffX) * Camera.Transform.Rotation;


            RayTracer.Use();
            Matrix4 toWorld = Camera.Transform.GetMatrix();
            toWorld.Transpose();
            RayTracer.ToWorldSpace.Set(toWorld);
            RayTracer.CameraPos.Set(Camera.Transform.Position);

            RayTracer.Dispatch(Size.X / 32, Size.Y / 32, 1);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            RenderProgram.Use();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _vertexArray.DrawElements(PrimitiveType.Triangles, 6);
            //_rayTracerShader.Draw();
            SwapBuffers();

            Console.WriteLine(1.0/e.Time);
        }
    }
}
