using Silk.NET.OpenGL;
using System;

namespace Planets.Meshes
{
    public class VBO : IDisposable
    {

        private readonly uint _id;
        private readonly BufferTargetARB _target;

        private readonly GL _gl;

        public VBO(BufferTargetARB target, GL gl)
        {
            _target = target;
            _gl = gl;

            _id = _gl.GenBuffer();
            Bind();
        }

        public void Bind()
        {
            _gl.BindBuffer(_target, _id);
        }

        public void Unbind()
        {
            _gl.BindBuffer(_target, 0);
        }

        public void Load<T>(ReadOnlySpan<T> data, BufferUsageARB usage)
            where T : unmanaged
        {
            Bind();
            _gl.BufferData(_target, data, usage);
        }

        public void Dispose()
        {
            Unbind();
            _gl.DeleteBuffer(_id);
        }

    }
}
