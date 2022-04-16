using Planets.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using MathFs = System.MathF;

namespace Planets.Meshes.Planets
{
    public class PlanetMeshGenerator
    {

        // MUST be >= 3! Otherwise the initial spherical triangles can't mathematically exist!
        public const uint NUMBER_OF_THREADS = 4;

        private readonly DelaunayTriangulator[] _triangulators = new DelaunayTriangulator[NUMBER_OF_THREADS];

        private readonly int _vertexCount;

        public Vector3[] Vertices { get; }

        public PlanetMeshGenerator(int vertexCount)
        {
            _vertexCount = vertexCount;

            Vertices = new Vector3[_vertexCount + (NUMBER_OF_THREADS * DelaunayTriangulator.REQUIRED_EXTRA_SPACE)]; // Yes I stick the giant triangle vertices in there, but that makes the code such much more simpler, and honestly it's just 24 floats more in the 10000000 or so vertex sphere.

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

        private static uint CalculateStripe(Vector3 vertex)
        {
            const float SECTION_ANGLE_SIZE = 2.0f * MathFs.PI / (float)NUMBER_OF_THREADS;

            float angle = Maths.AngleFromPointOnUnitCircle(Vector2.Normalize(new Vector2(vertex.X, vertex.Z)));

            uint stripe = (uint)MathFs.Floor(angle / SECTION_ANGLE_SIZE);

            return stripe;
        }

        private void AddVertex(Vector3 vertex, uint index)
        {
            if (index == 5015)
            {
                System.Console.WriteLine("This should work!");
            }

            Vertices[index] = vertex;

            uint stripe = CalculateStripe(vertex);
            _triangulators[stripe].AddIndex(index);
        }

        private void GenerateVertices()
        {
            for (uint i = 0; i < _vertexCount; i++)
            {
                float idx = (float)i + 0.5f;

                float φ = MathFs.Acos(1.0f - 2.0f * idx / (float)_vertexCount);
                float θ = MathFs.PI * (1.0f + MathFs.Sqrt(5.0f)) * idx;

                Vector3 vertex = new()
                {
                    X = MathFs.Cos(θ) * MathFs.Sin(φ),
                    Y = MathFs.Sin(θ) * MathFs.Sin(φ),
                    Z = MathFs.Cos(φ)
                };

                AddVertex(vertex, NUMBER_OF_THREADS * DelaunayTriangulator.REQUIRED_EXTRA_SPACE + i);
            }
        }

        private uint[] DelaunyTriangulate()
        {
            var indices = new List<uint>();

            /*var processingTasks = new Task<ICollection<uint>>[NUMBER_OF_THREADS];
            for (uint i = 0; i < NUMBER_OF_THREADS; i++)
            {
                processingTasks[i] = _triangulators[i].TriangulateAsync();
            }

            Task.WaitAll(processingTasks);

            for (uint i = 0; i < NUMBER_OF_THREADS; i++)
            {
                indices.AddRange(processingTasks[i].Result);
            }

            for (int i = 0; i < _triangulators.Length; i++)
            {
                processingTasks[i].Dispose();

                _triangulators[i].Dispose();
                _triangulators[i] = null;
            }*/

            indices.AddRange(_triangulators[2].Triangulate());

            return indices.ToArray();
        }

        public uint[] Generate()
        {
            GenerateVertices();
            return DelaunyTriangulate();
        }

    }
}
