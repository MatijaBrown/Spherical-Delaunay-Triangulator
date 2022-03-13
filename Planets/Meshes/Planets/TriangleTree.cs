using System.Collections.Generic;

namespace Planets.Meshes.Planets
{
    public class TriangleTree
    {

        public IDictionary<int, TriangleTreeNode> Nodes = new Dictionary<int, TriangleTreeNode>();

        public TriangleTreeNode TopSupertriangle { get; set; }

        public TriangleTreeNode BottomSupertriangle { get; set; }

        public TriangleTreeNode GetContainingTriangle(uint vertex)
        {
            if (vertex == 50)
            {
                System.Console.WriteLine();
            }

            if (TopSupertriangle.Contains(vertex))
            {
                return TopSupertriangle.FetchSmallestContainingTriangle(vertex);
            }
            else if (BottomSupertriangle.Contains(vertex))
            {
                return BottomSupertriangle.FetchSmallestContainingTriangle(vertex);
            }

            System.Console.Error.WriteLine(vertex);
            throw new System.Exception("Impossible!");
        }

        public TriangleTreeNode NodeFromSimplex(int simplex)
        {
            return Nodes.TryGetValue(simplex, out TriangleTreeNode node) ? node : null;
        }

        public TriangleTreeNode Opposite(uint index, Simplex simplex)
        {
            return NodeFromSimplex(simplex.Opposite(index));
        }

    }
}
