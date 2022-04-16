namespace Planets.Meshes.Planets
{
    public class DelaunayTree
    {

        public DelaunayTreeNode Top { get; }

        public DelaunayTreeNode Bottom { get; }

        public DelaunayTree(DelaunayTreeNode top, DelaunayTreeNode bottom)
        {
            Top = top;
            Bottom = bottom;
        }

    }
}
