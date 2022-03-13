using Planets.Entities;
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using Shader = Planets.Shaders.Shader;

namespace Planets.Rendering
{
    public class PlanetRenderer : IDisposable
    {

        private readonly Shader _shader;

        private readonly GL _gl;

        public PlanetRenderer(GL gl)
        {
            _gl = gl;

            _shader = new Shader("planetVertexShader.vert", "planetFragmentShader.frag", _gl);
            _shader.Start();
            _shader.LoadMatrix("projectionMatrix", Matrix4x4.Identity);
            _shader.Stop();
        }

        public unsafe void Render(Planet planet, Camera camera, Matrix4x4 projectionMatrix)
        {
            var mesh = planet.Mesh;
            var transform = new Transform(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Identity, 1.0f);

            var vao = mesh.Vao;

            vao.Bind();

            _shader.Start();

            _shader.LoadMatrix("projectionMatrix", projectionMatrix);
            _shader.LoadMatrix("transformationMatrix", transform.CreateTransformationMatrix());
            _shader.LoadMatrix("viewMatrix", camera.CalculateViewMatrix());

            _shader.LoadFloat("radius", 10.0f);

            _gl.DrawElements(PrimitiveType.Lines, planet.Mesh.VertexCount, DrawElementsType.UnsignedInt, null);

            _shader.Stop();

            vao.Unbind();
        }

        public void Dispose()
        {
            _shader.Dispose();
        }

    }
}
