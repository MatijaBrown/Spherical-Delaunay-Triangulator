using Planets.Meshes;
using Planets.Meshes.Planets;
using Planets.Utils;
using Silk.NET.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace Planets.Entities
{
    public class Planet : Entity, IDisposable
    {

        private const int VERTEX_COUNT = 100000;

        private readonly PlanetMeshGenerator _meshGenerator;
        private readonly VAO _vao;

        private readonly GL _gl;

        public Planet(GL gl)
            : base(new Transform(), default)
        {
            _gl = gl;

            _meshGenerator = new PlanetMeshGenerator(VERTEX_COUNT);
            uint[] indices = _meshGenerator.Generate();

            _vao = new VAO(_gl);
            _vao.StoreDataInAttributeList<Vector3D>(0, 3, (uint)(Marshal.SizeOf<Vector3D>()), VertexAttribPointerType.Double, _meshGenerator.Vertices);
            _vao.BindIndicesBuffer(indices);

            Mesh = new Mesh((uint)indices.Length, _vao);
        }

        public void Dispose()
        {
            _vao.Dispose();
        }

    }
}
