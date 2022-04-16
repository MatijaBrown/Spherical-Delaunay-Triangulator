using Planets.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Planets.Meshes.Planets
{
    public class DelaunayTriangulator : IDisposable
    {

        public const uint REQUIRED_EXTRA_SPACE = 4;

        private readonly IList<uint> _indices = new List<uint>();
        private readonly Vector3[] _vertices;
        private readonly int _index;

        private readonly Task<ICollection<uint>> _triangulationTask;

        private DelaunayTree _delaunayTree;

        public DelaunayTriangulator(Vector3[] vertices, int index)
        {
            _vertices = vertices;
            _index = index;

            _triangulationTask = new Task<ICollection<uint>>(Triangulate);
        }

        public void AddIndex(uint index)
        {
            _indices.Add(index);
        }

        public void GenerateSupertriangles()
        {
            float nThOfCirle = 2.0f * MathF.PI / PlanetMeshGenerator.NUMBER_OF_THREADS;

            _vertices[_indices[0]] = new Vector3(MathF.Cos(_index * nThOfCirle),                0.0f, MathF.Sin(_index * nThOfCirle             ));
            _vertices[_indices[1]] = new Vector3(MathF.Cos(_index * nThOfCirle + nThOfCirle),   0.0f, MathF.Sin(_index * nThOfCirle + nThOfCirle));
            _vertices[_indices[2]] = Vector3.UnitY;
            _vertices[_indices[3]] = -Vector3.UnitY;

            DelaunayTreeNode top = SortCCW(_indices[0], _indices[2], _indices[1], null, null, null);
            DelaunayTreeNode bottom = SortCCW(_indices[0], _indices[3], _indices[1], null, null, null);

            top.TB = bottom;
            bottom.TB = top;

            _delaunayTree = new DelaunayTree(top, bottom);
        }

        private DelaunayTreeNode SortCCW(DelaunayTreeNode node)
        {
            if (!Maths.IsCCW(_vertices[node.A], _vertices[node.B], _vertices[node.C]))
            {
                node.FlipVertexOrder();
            }
            return node;
        }

        private DelaunayTreeNode SortCCW(uint a, uint b, uint c, DelaunayTreeNode ta, DelaunayTreeNode tb, DelaunayTreeNode tc)
        {
            return SortCCW(new DelaunayTreeNode(a, b, c, ta, tb, tc));
        }

        private bool ContainsVertex(uint vertex, DelaunayTreeNode node)
        {
            return Maths.IsCCW(_vertices[node.A], _vertices[node.B], _vertices[vertex])
                && Maths.IsCCW(_vertices[node.B], _vertices[node.C], _vertices[vertex])
                && Maths.IsCCW(_vertices[node.C], _vertices[node.A], _vertices[vertex]);
        }

        private static void InsertVertex(uint vertex, DelaunayTreeNode node)
        {
            var s0 = new DelaunayTreeNode(node.A, node.B, vertex, null, null, node.TC);
            var s1 = new DelaunayTreeNode(node.B, node.C, vertex, null, null, node.TA);
            var s2 = new DelaunayTreeNode(node.C, node.A, vertex, null, null, node.TB);

            // Set the opposites of the opposites (us selves) to the newly generated sub-triangles. This will stop us from having to loop again later.
            node.TC?.ReplaceOpposite(node, s0);
            node.TA?.ReplaceOpposite(node, s1);
            node.TB?.ReplaceOpposite(node, s2);

            // Set as each-others' neighbours
            s0.TA = s1;
            s0.TB = s2;

            s1.TA = s2;
            s1.TB = s0;

            s2.TA = s0;
            s2.TB = s1;

            node.AddChild(s0);
            node.AddChild(s1);
            node.AddChild(s2);
        }

        private DelaunayTreeNode InsertTriangleInTree(uint vertex, DelaunayTreeNode node)
        {
            if (node.ChildCount == 0)
            {
                InsertVertex(vertex, node);
                return node;
            }

            var en = node.GetEnumerator();
            while (en.MoveNext())
            {
                DelaunayTreeNode current = en.Current;
                if (ContainsVertex(vertex, current))
                {
                    return InsertTriangleInTree(vertex, current);
                }
            }

            throw new Exception("Vertex at index " + vertex + " (" + _vertices[vertex].ToString() + ") is not inside bounds of this triangulation!");
        }

        private static void Flip(DelaunayTreeNode tri, DelaunayTreeNode opp, out DelaunayTreeNode new0, out DelaunayTreeNode new1)
        {
            uint index = tri.C;
            uint oppositeIndex = opp.ThirdPoint(tri.A, tri.B);

            new0 = new DelaunayTreeNode(
                a: tri.A,
                b: oppositeIndex,
                c: index,

                ta: null,
                tb: tri.TB,
                tc: opp.Opposite(tri.B)
            );
            new1 = new DelaunayTreeNode(
                a: oppositeIndex,
                b: tri.B,
                c: index,

                ta: tri.TA,
                tb: null,
                tc: opp.Opposite(tri.A)
            );

            new0.TC?.ReplaceOpposite(opp, new0);
            new0.TB?.ReplaceOpposite(tri, new0);

            new1.TC?.ReplaceOpposite(opp, new1);
            new1.TA?.ReplaceOpposite(tri, new1);

            new0.TA = new1;
            new1.TB = new0;

            tri.AddChild(new0);
            tri.AddChild(new1);

            opp.AddChild(new0);
            opp.AddChild(new1);
        }

        private void CheckEdge(uint index, DelaunayTreeNode tri, DelaunayTreeNode opp)
        {
            if (opp == null)
            {
                return;
            }

            Vector3 vertex = _vertices[index];

            Vector3 cOpp = Maths.Circumcentre(_vertices[opp.A], _vertices[opp.B], _vertices[opp.C]);
            float rOpp = Maths.Circumradius(_vertices[opp.A], _vertices[opp.B], _vertices[opp.C]);

            if (Maths.ArcLength(vertex, cOpp) < rOpp)
            {
                Flip(tri, opp, out DelaunayTreeNode new0, out DelaunayTreeNode new1);

                CheckEdge(index, new0, new0.TC);
                CheckEdge(index, new1, new1.TC);
            }
        }

        private void CheckEdges(DelaunayTreeNode node)
        {
            CheckEdge(node.A, node, node.TA);
            CheckEdge(node.B, node, node.TB);
            CheckEdge(node.C, node, node.TC);
        }

        private void AddVertex(uint index)
        {
            if (index == 5015)
            {
                Console.WriteLine("TEST");
            }

            DelaunayTreeNode node;
            if (ContainsVertex(index, _delaunayTree.Top))
            {
                node = InsertTriangleInTree(index, _delaunayTree.Top);
            }
            else if (ContainsVertex(index, _delaunayTree.Bottom))
            {
                node = InsertTriangleInTree(index, _delaunayTree.Bottom);
            }
            else
            {
                throw new Exception("Vertex at index " + index + " (" + _vertices[index].ToString() + ") is not inside bounds of this triangulation!");
            }

            // The nodes all have 3 children here!
            CheckEdges(node.Children[0]);
            CheckEdges(node.Children[1]);
            CheckEdges(node.Children[2]);
        }

        private void AddBottomSimplexIndices(DelaunayTreeNode node, IList<uint> list)
        {
            if (node.ChildCount == 0)
            {
                list.Add(node.A);
                list.Add(node.B);
                list.Add(node.C);
                return;
            }

            var en = node.GetEnumerator();
            while (en.MoveNext())
            {
                AddBottomSimplexIndices(en.Current, list);
                en.Current.Clear();
            }
        }

        private IList<uint> GenerateMeshInformation()
        {
            var mesh = new List<uint>();
            AddBottomSimplexIndices(_delaunayTree.Top, mesh);
            AddBottomSimplexIndices(_delaunayTree.Bottom, mesh);
            return mesh;
        }

        public ICollection<uint> Triangulate()
        {
            for (int i = (int)REQUIRED_EXTRA_SPACE; i < _indices.Count; i++)
            {
                AddVertex(_indices[i]);
            }
            return GenerateMeshInformation();
        }

        public async Task<ICollection<uint>> TriangulateAsync()
        {
            _triangulationTask.Start();
            return await _triangulationTask;
        }

        public void Dispose()
        {

        }

    }
}
