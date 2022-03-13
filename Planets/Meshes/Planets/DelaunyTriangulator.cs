using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace Planets.Meshes.Planets
{
    public class DelaunyTriangulator : IDisposable
    {

        public const uint REQUIRED_EXTRA_SPACE = 4;

        private readonly List<Simplex> _simplices = new();
        private readonly HashSet<int> _handledSimplices = new();

        private readonly TriangleTree _triangleTree;

        private readonly PlanetMeshGenerator _meshGenerator;
        private readonly uint _threadIndex;

        private readonly Task<ICollection<uint>> _triangulationTask;

        public List<uint> Indices { get; } // The first REQUIRED_EXTRA_SPACE indices will be reserved for the giant triangle.

        public DelaunyTriangulator(PlanetMeshGenerator meshGenerator, uint threadIndex)
        {
            _meshGenerator = meshGenerator;
            _threadIndex = threadIndex;

            _triangulationTask = new Task<ICollection<uint>>(Triangulate);

            Indices = new List<uint>();

            for (uint i = 0; i < REQUIRED_EXTRA_SPACE; i++)
            {
                Indices.Add((_threadIndex * REQUIRED_EXTRA_SPACE) + i);
            }

            _triangleTree = CreateGiantSimplices();
        }

        /* Using the following layout:
         * 0-2: Northern Hemisphere
         * 3-5: Southern Hemisphere
         * 
         * 0: Point at start
         * 1: Point at end
         * 2: Point at top
         * 
         * 3: Point at start
         * 4: Point at bottom
         * 5: Point at end
         */
        private TriangleTree CreateGiantSimplices()
        {
            const float SECTION_ANGLE_SIZE = 2.0f * MathF.PI / (float)PlanetMeshGenerator.NUMBER_OF_THREADS;

            var tree = new TriangleTree();

            var start = new Vector3()
            {
                X = MathF.Cos(_threadIndex * SECTION_ANGLE_SIZE),
                Y = 0.0f,
                Z = MathF.Sin(_threadIndex * SECTION_ANGLE_SIZE)
            };

            var end = new Vector3()
            {
                X = MathF.Cos(_threadIndex * SECTION_ANGLE_SIZE + SECTION_ANGLE_SIZE),
                Y = 0.0f,
                Z = MathF.Sin(_threadIndex * SECTION_ANGLE_SIZE + SECTION_ANGLE_SIZE)
            };

            // Top
            _meshGenerator.Vertices[Indices[0]] = start;
            _meshGenerator.Vertices[Indices[1]] = end;
            _meshGenerator.Vertices[Indices[2]] = Vector3.UnitY;

            // Bottom
            _meshGenerator.Vertices[Indices[3]] = -Vector3.UnitY;

            // Tree
            _simplices.Add(new Simplex(Indices[0], -1, Indices[1], -1, Indices[2], 1, IsLeft(Indices[0], Indices[1], Indices[2]))); // Adding this first, so at 0.
            tree.TopSupertriangle = new TriangleTreeNode(0, this, tree);

            _simplices.Add(new Simplex(Indices[0], -1, Indices[1], -1, Indices[3], 1, IsLeft(Indices[0], Indices[1], Indices[3]))); // Adding this first, so at 1.
            tree.BottomSupertriangle = new TriangleTreeNode(1, this, tree);

            return tree;
        }

        private static float AreaOfTriangle(Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = new Vector2(b.X - a.X, b.Y - a.Y);
            var ac = new Vector2(c.X - a.X, c.Y - a.Y);

            float crossProduct = (ab.X * ac.Y) - (ab.Y * ac.X);

            return MathF.Abs(crossProduct) / 2.0f;
        }

        private static bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            const float SMALL = 3e-7f;

            float triangleArea = AreaOfTriangle(a, b, c);

            float pab = AreaOfTriangle(point, a, b);
            float pbc = AreaOfTriangle(point, b, c);
            float pac = AreaOfTriangle(point, a, c);

            float area = pab + pbc + pac;

            return MathF.Abs(triangleArea - area) <= SMALL;
        }

        /*
         * Return the steepness values of the line going through the origin and the specified point.
         * I.e. mx, my and mz in the following equation:
         *  (x, y, z) = (mx * t, my * t, mz * t)
         */
        private static (float, float, float) GetLineSteepness(Vector3 pointOnLine)
        {
            var normalizedVector = Vector3.Normalize(pointOnLine); // Lies on the unit sphere, so shouldn't be necessary but do it anyway.
            return (normalizedVector.X, normalizedVector.Y, normalizedVector.Z);
        }

        private static Vector3 PointFromLineSteepnesses(float t, (float, float, float) steepnesses)
        {
            float mx = steepnesses.Item1;
            float my = steepnesses.Item2;
            float mz = steepnesses.Item3;

            return new Vector3(mx * t, my * t, mz * t);
        }

        /*
         * Returns the plain's normal.
         */
        private static Vector3 CreatePlain(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(a - b, b - c);
        }

        /*
         * Inputs the variables (normal of the plain, a point on the plain, the steepnesses of the line passing through the plain)
         * into the plain equation solved for t.
         */
        private static float CalculateTToPlane(Vector3 normal, Vector3 a, (float, float, float) steepnesses)
        {
            float nx = normal.X;
            float ny = normal.Y;
            float nz = normal.Z;

            float ax = a.X;
            float ay = a.Y;
            float az = a.Z;

            float dx = steepnesses.Item1;
            float dy = steepnesses.Item2;
            float dz = steepnesses.Item3;

            return (nx * ax + ny * ay + nz * az) / (nx * dx + ny * dy + nz * dz);
        }

        private static float ArcLength(Vector3 p, Vector3 q)
        {
            return MathF.Acos(Vector3.Dot(p, q));
        }

        private static float Tripple(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Dot(a, Vector3.Cross(b, c));
        }

        private float CircumRadius(Simplex simplex, Vector3 centre)
        {
            Vector3 a = _meshGenerator.Vertices[simplex.A];
            Vector3 b = _meshGenerator.Vertices[simplex.B];
            Vector3 c = _meshGenerator.Vertices[simplex.C];

            return MathF.Acos(Tripple(a, b, c) / (Vector3.Cross(a, b) + Vector3.Cross(b, c) + Vector3.Cross(c, a)).Length());
        }

        private Vector3 Circumcentre(Simplex simplex)
        {
            Vector3 a = _meshGenerator.Vertices[simplex.A];
            Vector3 b = _meshGenerator.Vertices[simplex.B];
            Vector3 c = _meshGenerator.Vertices[simplex.C];

            return Vector3.Cross(b - a, c - a) / Vector3.Cross(b - a, c - a).Length();
        }

        public bool SimplexContainsVertex(int simplexIndex, uint vertexIndex)
        {
            var simplex = _simplices[simplexIndex];

            Vector3 a = _meshGenerator.Vertices[simplex.A];
            Vector3 b = _meshGenerator.Vertices[simplex.B];
            Vector3 c = _meshGenerator.Vertices[simplex.C];

            Vector3 vertex = _meshGenerator.Vertices[vertexIndex];

            float tripABV = Tripple(a, b, vertex);
            float tripBCV = Tripple(b, c, vertex);
            float tripACV = Tripple(a, c, vertex);

            // return (tripABV > 0.0f) && (tripBCV > 0.0f) && (tripACV > 0.0f);

            (float, float, float) steepnesses = GetLineSteepness(vertex);
            Vector3 plainNormal = CreatePlain(a, b, c);

            float t = CalculateTToPlane(plainNormal, a, steepnesses);

            Vector3 pointOnPlane = PointFromLineSteepnesses(t, steepnesses);

            // One axis can safely be ignored here because of the projection. Thus we ignore the z axis.
            var pay = new Vector2(a.X, a.Y);
            var pby = new Vector2(b.X, b.Y);
            var pcy = new Vector2(c.X, c.Y);
            var pvy = new Vector2(pointOnPlane.X, pointOnPlane.Y);

            var paz = new Vector2(a.X, a.Z);
            var pbz = new Vector2(b.X, b.Z);
            var pcz = new Vector2(c.X, c.Z);
            var pvz = new Vector2(pointOnPlane.X, pointOnPlane.Z);

            return IsPointInTriangle(pvy, pay, pby, pcy) && IsPointInTriangle(pvz, paz, pbz, pcz);
        }

        public bool SimplexHasSide(int simplex, uint a, uint b)
        {
            if (a == b)
            {
                throw new Exception();
            }
            var s = _simplices[simplex];
            return ((s.A == a) || (s.B == a) || (s.C == a)) && ((s.A == b) || (s.B == b) || (s.C == b));
        }

        public Simplex GetSimplex(int index)
        {
            return _simplices[index];
        }

        private bool IsLeft(uint a, uint b, uint h) // If h is left of the line ab
        {
            Vector3 x = _meshGenerator.Vertices[a];
            Vector3 y = _meshGenerator.Vertices[b];
            Vector3 z = _meshGenerator.Vertices[h];

            var matrix = new Matrix3X3<float>(x.ToGeneric(), y.ToGeneric(), z.ToGeneric());

            return matrix.GetDeterminant() >= 0.0f;
        }

        private int AddSimplex(uint p, uint a, uint b, TriangleTreeNode parent)
        {
            TriangleTreeNode tcn = parent.FindAdjacentTriangle(a, b, null);

            int tc = tcn == null ? -1 : tcn.Self;

            var simplex = new Simplex(a, b, p, tc, IsLeft(a, b, p));

            _simplices.Add(simplex);

            return _simplices.Count - 1;
        }

        private void SetSimplexNeighbours(int simplexIndex, uint indexA, int neighbourA, uint indexB, int neighbourB)
        {
            Simplex simplex = _simplices[simplexIndex];
            simplex.Opposite(indexA, neighbourA);
            simplex.Opposite(indexB, neighbourB);
            _simplices[simplexIndex] = simplex;
        }

        private static int GetLowestOpp(uint a, uint b, TriangleTreeNode node)
        {
            TriangleTreeNode adj = node.FindAdjacentTriangle(a, b, null);
            return adj == null ? -1 : adj.Self;
        }

        private (TriangleTreeNode, TriangleTreeNode) Flip(uint i, TriangleTreeNode tri, TriangleTreeNode triOpp)
        {
            Simplex p = GetSimplex(tri.Self);
            Simplex next = GetSimplex(triOpp.Self);

            (uint, uint) adj = p.GetOthers(i);

            uint a = adj.Item1;
            uint b = adj.Item2;
            uint t = next.GetThirdPoint(a, b);

            int pta = GetLowestOpp(b, i, tri);
            int ptb = GetLowestOpp(a, i, tri);

            int nta = GetLowestOpp(b, t, triOpp);
            int ntb = GetLowestOpp(a, t, triOpp);

            var new0 = new Simplex(t, ptb, a, _simplices.Count + 1, i, ntb, IsLeft(t, a, i));
            var new1 = new Simplex(t, pta, b, _simplices.Count, i, nta, IsLeft(t, b, i));

            _simplices.Add(new0);
            _simplices.Add(new1);

            var node0 = new TriangleTreeNode(_simplices.Count - 2, tri, triOpp, this);
            var node1 = new TriangleTreeNode(_simplices.Count - 1, tri, triOpp, this);

            tri.Child1 = node0;
            tri.Child2 = node1;
            tri.Child3 = null;

            triOpp.Child1 = node0;
            triOpp.Child2 = node1;
            triOpp.Child3 = null;

            return (node0, node1);
        }

        private void CheckEdge(uint pIndex, TriangleTreeNode tri, TriangleTreeNode triOpp)
        {
            if (triOpp == null)
            {
                return;
            }

            Vector3 p = _meshGenerator.Vertices[pIndex];

            Simplex n = GetSimplex(triOpp.Self);

            Vector3 cOpp = Circumcentre(n);
            float rOpp = CircumRadius(n, cOpp);
            float length = ArcLength(p, cOpp);

            if (length < rOpp)
            {
                (TriangleTreeNode, TriangleTreeNode) newNodes = Flip(pIndex, tri, triOpp);
                CheckEdge(pIndex, newNodes.Item1, _triangleTree.Opposite(pIndex, GetSimplex(newNodes.Item1.Self)));
                CheckEdge(pIndex, newNodes.Item2, _triangleTree.Opposite(pIndex, GetSimplex(newNodes.Item2.Self)));
            }
        }

        private void CheckEdges(TriangleTreeNode node)
        {
            Simplex p = GetSimplex(node.Self);
            CheckEdge(p.A, node, _triangleTree.NodeFromSimplex(p.TA));
            CheckEdge(p.B, node, _triangleTree.NodeFromSimplex(p.TB));
            CheckEdge(p.C, node, _triangleTree.NodeFromSimplex(p.TC));
        }

        private void AddVertex(uint index)
        {
            var node = _triangleTree.GetContainingTriangle(index);
            var simplex = _simplices[(int)node.Self];

            int s0 = AddSimplex(index, simplex.A, simplex.B, node);
            int s1 = AddSimplex(index, simplex.A, simplex.C, node);
            int s2 = AddSimplex(index, simplex.B, simplex.C, node);

            SetSimplexNeighbours(s0, simplex.B, s1, simplex.A, s2);
            SetSimplexNeighbours(s1, simplex.C, s0, simplex.A, s2);
            SetSimplexNeighbours(s2, simplex.C, s0, simplex.B, s1);

            node.Child1 = new TriangleTreeNode(s0, node, this);
            node.Child2 = new TriangleTreeNode(s1, node, this);
            node.Child3 = new TriangleTreeNode(s2, node, this);

            CheckEdges(node.Child1);
            CheckEdges(node.Child2);
            CheckEdges(node.Child3);
        }

        private bool CheckSimplex(Simplex simplex) // Finally delete the large triangles
        {
            HashSet<uint> generated = new(new uint[] { Indices[0], Indices[1], Indices[2], Indices[3] });
            if (generated.Contains(simplex.A) || generated.Contains(simplex.B) || generated.Contains(simplex.C))
            {
                return false;
            }
            return true;
        }

        private IList<uint> GetAllBottomVertices(TriangleTreeNode node)
        {
            var verts = new List<uint>();

            if (_handledSimplices.Contains(node.Self))
            {
                return verts;
            }

            if (node.Child1 != null) // Implies node.Child2 != null is true as well!
            {
                verts.AddRange(GetAllBottomVertices(node.Child1));
                verts.AddRange(GetAllBottomVertices(node.Child2));
            }
            if (node.Child3 != null)
            {
                verts.AddRange(GetAllBottomVertices(node.Child3));
            }

            if (node.Child1 == null)
            {
                Simplex simplex = _simplices[(int)node.Self];
                if (CheckSimplex(simplex))
                {
                    verts.Add(simplex.A);
                    verts.Add(simplex.B);
                    verts.Add(simplex.C);
                }
            }

            _handledSimplices.Add(node.Self);

            return verts;
        }

        private IList<uint> GetAllFinalVertices()
        {
            var verts = new List<uint>();
            _handledSimplices.Clear();

            verts.AddRange(GetAllBottomVertices(_triangleTree.TopSupertriangle));
            verts.AddRange(GetAllBottomVertices(_triangleTree.BottomSupertriangle));

            return verts;
        }

        public ICollection<uint> Triangulate()
        {
            for (int i = (int)REQUIRED_EXTRA_SPACE; i < Indices.Count; i++)
            {
                uint index = Indices[i];

                AddVertex(index);
            }

            return GetAllFinalVertices();
        }

        public async Task<ICollection<uint>> ComputeTriangulation()
        {
            _triangulationTask.Start();
            return await _triangulationTask;
        }

        public void Dispose()
        {
            _handledSimplices.Clear();
            _simplices.Clear();
            Indices.Clear();
            _triangleTree.Nodes.Clear();
            _triangleTree.TopSupertriangle = _triangleTree.BottomSupertriangle = null;
        }

    }
}
