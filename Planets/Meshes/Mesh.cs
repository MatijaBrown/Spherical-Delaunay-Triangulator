namespace Planets.Meshes
{
    public struct Mesh
    {

        public readonly uint VertexCount;
        public readonly VAO Vao;

        public Mesh(uint vertexCount, VAO vao)
        {
            VertexCount = vertexCount;
            Vao = vao;
        }

    }
}
