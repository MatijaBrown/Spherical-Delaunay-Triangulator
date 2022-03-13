using Planets.Entities;
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using Shader = Planets.Shaders.Shader;

namespace Planets.Rendering
{
    public class EntityRenderer : IDisposable
    {

        private readonly Shader _shader;

        private readonly GL _gl;

        public EntityRenderer(GL gl)
        {
            _gl = gl;

            _shader = new Shader("quadVertexShader.vert", "quadFragmentShader.frag", _gl);
            _shader.Start();
            _shader.LoadMatrix("projectionMatrix", Matrix4x4.Identity);
            _shader.Stop();
        }

        public void Render(Entity entity, Camera camera, Matrix4x4 projectionMatrix)
        {
            var mesh = entity.Mesh;
            var transform = entity.Transform;

            var vao = mesh.Vao;

            vao.Bind();

            _shader.Start();

            _shader.LoadMatrix("projectionMatrix", projectionMatrix);
            _shader.LoadMatrix("transformationMatrix", transform.CreateTransformationMatrix());
            _shader.LoadMatrix("viewMatrix", camera.CalculateViewMatrix());

            _gl.DrawArrays(PrimitiveType.Triangles, 0, mesh.VertexCount);

            _shader.Stop();

            vao.Unbind();
        }

        public void Dispose()
        {
            _shader.Dispose();
        }

    }
}
