using System.Collections.Generic;

namespace Planets.Meshes.Planets
{
    public class TriangleTreeNode
    {

        private readonly HashSet<int> _containedSimplices = new();
        private readonly HashSet<uint> _containedIndices = new();

        private readonly DelaunyTriangulator _triangulator;
        private readonly TriangleTree _tree;

        public int Self { get; } // Pointer to the simplex representing this triangle.

        public TriangleTreeNode Child1 { get; set; }

        public TriangleTreeNode Child2 { get; set; }

        public TriangleTreeNode Child3 { get; set; }

        public TriangleTreeNode Parent { get; }

        public TriangleTreeNode Parent2 { get; }

        public uint Depth { get; }

        public TriangleTreeNode(int self, TriangleTreeNode parent, TriangleTreeNode parent2, DelaunyTriangulator triangulator)
        {
            Self = self;
            Parent = parent;
            Parent2 = parent2;
            _triangulator = triangulator;

            _tree = Parent._tree;

            _tree.Nodes.Add(Self, this);

            AddSimplex(Self);

            Depth = Parent.Depth + 1;
        }

        public TriangleTreeNode(int self, TriangleTreeNode parent, DelaunyTriangulator triangulator)
            :this(self, parent, null, triangulator) { }

        public TriangleTreeNode(int self, DelaunyTriangulator triangulator, TriangleTree tree)
        {
            Self = self;
            _triangulator = triangulator;
            _tree = tree;

            _tree.Nodes.Add(Self, this);

            AddSimplex(Self);

            Depth = 0;
        }

        private void AddSimplex(int index)
        {
            Simplex simplex = _triangulator.GetSimplex(index);

            _containedSimplices.Add(index);

            _containedIndices.Add(simplex.A);
            _containedIndices.Add(simplex.B);
            _containedIndices.Add(simplex.C);

            if (Parent != null)
            {
                Parent.AddSimplex(index);
                if (Parent2 != null)
                {
                    Parent2.AddSimplex(index);
                }
            }
        }

        public bool Contains(uint vertex)
        {
            return _triangulator.SimplexContainsVertex(Self, vertex);
        }

        public bool AlreadyContains(uint vertex)
        {
            return _containedIndices.Contains(vertex);
        }

        public TriangleTreeNode FetchSmallestContainingTriangle(uint vertex)
        {
            if ((Child1 == null) && (Child2 == null) && (Child3 == null))
            {
                return this;
            }

            if (Child1.Contains(vertex))
            {
                return Child1.FetchSmallestContainingTriangle(vertex);
            }
            else if (Child2.Contains(vertex))
            {
                return Child2.FetchSmallestContainingTriangle(vertex);
            }
            else if ((Child3 != null) && Child3.Contains(vertex))
            {
                return Child3.FetchSmallestContainingTriangle(vertex);
            }

            throw new System.Exception("Containing triangle happens to be in a superposition, sorry.");
        }

        public bool HasSide(uint a, uint b)
        {
            return _triangulator.SimplexHasSide(Self, a, b);
        }

        public bool ContainsSimplex(int simplexIndex)
        {
            return _containedSimplices.Contains(simplexIndex);
        }

        private static TriangleTreeNode PickHighestNode(TriangleTreeNode n0, TriangleTreeNode n1, TriangleTreeNode n2)
        {
            TriangleTreeNode best = n0;

            if ((best == null) || ((n1 != null) && (n1.Depth > best.Depth)))
            {
                best = n1;
            }

            if ((best == null) || ((n2 != null) && (n2.Depth > best.Depth)))
            {
                best = n2;
            }

            return best;
        }

        private TriangleTreeNode ChildFindAdjacentTriangle(uint a, uint b, TriangleTreeNode searchingFrom)
        {
            if (!(AlreadyContains(a) && AlreadyContains(b)) || searchingFrom.ContainsSimplex(Self))
            {
                return null;
            }

            TriangleTreeNode child1result;
            TriangleTreeNode child2result;
            TriangleTreeNode child3result = null;

            if (Child1 != null)
            {
                child1result = Child1.ChildFindAdjacentTriangle(a, b, searchingFrom);
                child2result = Child2.ChildFindAdjacentTriangle(a, b, searchingFrom);
                if (Child3 != null)
                {
                    child3result = Child3.ChildFindAdjacentTriangle(a, b, searchingFrom);
                }
            }
            else
            {
                return HasSide(a, b) ? this : null;
            }

            TriangleTreeNode best = PickHighestNode(child1result, child2result, child3result);

            return best;
        }

        public TriangleTreeNode FindAdjacentTriangle(uint a, uint b, TriangleTreeNode child)
        {
            TriangleTreeNode child1result = null;
            TriangleTreeNode child2result = null;
            TriangleTreeNode child3result = null;

            if ((Child1 != null) && (Child1 != child))
            {
                child1result = Child1.ChildFindAdjacentTriangle(a, b, child);
            }
            if ((Child2 != null) && (Child2 != child))
            {
                child2result = Child2.ChildFindAdjacentTriangle(a, b, child);
            }
            if ((Child3 != null) && (Child3 != child))
            {
                child3result = Child3.ChildFindAdjacentTriangle(a, b, child);
            }

            TriangleTreeNode bestChild = PickHighestNode(child1result, child2result, child3result);

            if ((bestChild == null) && (Parent != null))
            {
                return Parent.FindAdjacentTriangle(a, b, this);
            }
            else if ((bestChild == null) && (Parent == null))
            {
                if (this == _tree.BottomSupertriangle)
                {
                    return _tree.TopSupertriangle.ChildFindAdjacentTriangle(a, b, this);
                }
                else if (this == _tree.TopSupertriangle)
                {
                    return _tree.BottomSupertriangle.ChildFindAdjacentTriangle(a, b, this);
                }
            }

            return bestChild;
        }

    }
}
