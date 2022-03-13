using Planets.Meshes;
using System.Numerics;

namespace Planets.Entities
{
    public class Entity
    {

        public Mesh Mesh;
        public Transform Transform;

        public Entity(Transform transform, Mesh mesh)
        {
            Mesh = mesh;
            Transform = transform;
        }

        public Entity(Vector3 location, Mesh mesh)
        {
            Transform = new Transform(location, Quaternion.Identity, 1.0f);
            Mesh = mesh;
        }

    }
}
