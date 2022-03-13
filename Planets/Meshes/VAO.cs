using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Planets.Meshes
{
    public class VAO : IDisposable
    {

        private readonly IList<VBO> _vbos = new List<VBO>();
        private readonly IList<uint> _slots = new List<uint>();

        private readonly uint _id;
        private readonly GL _gl;

        public VAO(GL gl)
        {
            _gl = gl;

            _id = _gl.GenVertexArray();
            Bind();
        }

        public void Bind()
        {
            _gl.BindVertexArray(_id);
            foreach (uint slot in _slots)
            {
                _gl.EnableVertexAttribArray(slot);
            }
        }

        public void Unbind()
        {
            _gl.BindVertexArray(0);
            foreach (uint slot in _slots)
            {
                _gl.DisableVertexAttribArray(slot);
            }
        }

        public unsafe void StoreDataInAttributeList<T>(uint slot, int dimensions, uint stride, VertexAttribPointerType dataType, ReadOnlySpan<T> data)
            where T : unmanaged
        {
            _gl.BindVertexArray(_id);

            var vbo = new VBO(BufferTargetARB.ArrayBuffer, _gl);
            vbo.Load(data, BufferUsageARB.StaticDraw);

            _gl.VertexAttribPointer(slot, dimensions, dataType, false, stride, null);

            _vbos.Add(vbo);
            _slots.Add(slot);

            _gl.BindVertexArray(0);
        }

        public unsafe void BindIndicesBuffer(uint[] indices)
        {
            _gl.BindVertexArray(_id);

            var vbo = new VBO(BufferTargetARB.ElementArrayBuffer, _gl);
            vbo.Load<uint>(indices, BufferUsageARB.StaticDraw);

            _vbos.Add(vbo);

            _gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            Unbind();
            foreach (VBO vbo in _vbos)
            {
                vbo.Dispose();
            }
            _gl.DeleteVertexArray(_id);
        }

    }
}
