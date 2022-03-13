using Planets.Entities;
using Planets.Meshes;
using Planets.Rendering;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Planets
{
    public class Planets : IDisposable
    {

        private readonly IWindow _window;

        private MasterRenderer _renderer;

        private Camera _camera;
        private Planet _planet;

        private GL _gl;

        VAO meshData;
        float[] vertices = {
            /*-10.40f, -9.52f, 99.0f,
            -17.93f, -16.42f, 97.0f,
            -23.02f, -21.09f, 95.0f*/
            -10.4f, -9.52f, 99.0f,
            -17.93f, -16.42f, 97.0f,
            -23.02f, -21.09f, 95.0f
        };

        Entity quad;


        public Planets(IWindow window)
        {
            _window = window;

            _window.Load += Load;
            _window.Update += Update;
            _window.Render += Render;
            _window.Closing += Close;
        }

        public void Run()
        {
            _window.Run();
        }

        private unsafe void Load()
        {
            _gl = GL.GetApi(_window);

            _renderer = new MasterRenderer(_gl);

            meshData = new VAO(_gl);
            meshData.StoreDataInAttributeList<float>(0, 3, (uint)(3 * Marshal.SizeOf<float>()), VertexAttribPointerType.Float, vertices);

            var mesh = new Mesh(3, meshData);
            quad = new Entity(new Vector3(0.0f, 0.0f, -0.5f), mesh);

            _planet = new Planet(_gl);

            _camera = new Camera(Vector3.Zero, _window.CreateInput());

            var glfw = Glfw.GetApi();
            glfw.SetInputMode((WindowHandle*)_window.Handle, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
        }

        private void Update(double delta)
        {
            float dt = (float)delta;

            _camera.Update(dt);

            // quad.Transform.Location.Z -= 5.0f * dt;
        }

        private void Render(double _)
        {
            _gl.Viewport(0, 0, (uint)_window.Size.X, (uint)_window.Size.Y);

            _gl.ClearColor(0.2f, 0.4f, 0.7f, 1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _renderer.UpdateSize(_window.Size.X, _window.Size.Y);

            // _renderer.RenderEntity(quad);
            _renderer.RenderPlanet(_planet);

            _renderer.Render(_camera);
        }

        private void Close()
        {
            _planet.Dispose();
            meshData.Dispose();

            _renderer.Dispose();

            _gl.Dispose();
        }

        public void Dispose()
        {
            _window.Dispose();
        }

    }
}
