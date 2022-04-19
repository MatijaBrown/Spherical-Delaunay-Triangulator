using Planets.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Planets.Meshes.Planets
{
    public class PlanetMeshGenerator
    {

        // MUST be >= 3! Otherwise the initial spherical triangles can't mathematically exist!
        public const uint NUMBER_OF_THREADS = 16;

        private readonly DelaunayTriangulator[] _triangulators = new DelaunayTriangulator[NUMBER_OF_THREADS];

        private readonly int _vertexCount;

        public Vector3D[] Vertices { get; }

        public PlanetMeshGenerator(int vertexCount)
        {
            _vertexCount = vertexCount;

            Vertices = new Vector3D[_vertexCount + (NUMBER_OF_THREADS * DelaunayTriangulator.REQUIRED_EXTRA_SPACE)]; // Yes I stick the giant triangle vertices in there, but that makes the code such much more simpler, and honestly it's just 24 floats more in the 10000000 or so vertex sphere.

            for (int i = 0; i < NUMBER_OF_THREADS; i++)
            {
                _triangulators[i] = new DelaunayTriangulator(Vertices, i);
                for (int j = 0; j < DelaunayTriangulator.REQUIRED_EXTRA_SPACE; j++)
                {
                    _triangulators[i].AddIndex((uint)(i * DelaunayTriangulator.REQUIRED_EXTRA_SPACE) + (uint)j); // Add the indices for the supertriangles.
                }
                _triangulators[i].GenerateSupertriangles();
            }
        }

        private uint CalculateStripe(Vector3D vertex)
        {
            for (uint i = 0; i < NUMBER_OF_THREADS; i++)
            {
                if (_triangulators[i].IsResponsibleFor(vertex))
                {
                    return i;
                }
            }

            throw new System.Exception("No responsible triangulator found for vertex <" + vertex.ToString() + "> !");
        }

        private void AddVertex(Vector3D vertex, uint index)
        {
            Vertices[index] = vertex;

            uint stripe = CalculateStripe(vertex);
            _triangulators[stripe].AddIndex(index);
        }

        private void GenerateVertices()
        {
            for (uint i = 0; i < _vertexCount; i++)
            {
                double idx = i + 0.5;

                double φ = Math.Acos(1.0 - 2.0 * idx / _vertexCount);
                double θ = Math.PI * (1.0 + Math.Sqrt(5.0)) * idx;

                Vector3D vertex = new()
                {
                    X = Math.Cos(θ) * Math.Sin(φ),
                    Y = Math.Sin(θ) * Math.Sin(φ),
                    Z = Math.Cos(φ)
                };

                AddVertex(vertex, NUMBER_OF_THREADS * DelaunayTriangulator.REQUIRED_EXTRA_SPACE + i);
            }
        }

        private static void AddTriangle(uint a, uint b, uint c, IList<uint> indices)
        {
            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
        }

        private uint[] DelaunyTriangulate()
        {
            var indices = new List<uint>();

            var processingTasks = new Task[NUMBER_OF_THREADS];
            for (uint i = 0; i < NUMBER_OF_THREADS; i++)
            {
                processingTasks[i] = _triangulators[i].TriangulateAsync();
            }

            Task.WaitAll(processingTasks);

            for (uint i = 0; i < NUMBER_OF_THREADS; i++)
            {
                indices.AddRange(_triangulators[i].CompletedTriangulation);
            }

            for (int i = 0; i < _triangulators.Length; i++)
            {
                processingTasks[i].Dispose();

                _triangulators[i].Dispose();
                _triangulators[i] = null;
            }

            return indices.ToArray();
        }

        public uint[] Generate()
        {
            GenerateVertices();
            return DelaunyTriangulate();
        }

    }
}
