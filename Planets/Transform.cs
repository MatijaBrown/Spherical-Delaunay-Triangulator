using System.Numerics;

namespace Planets
{
    public struct Transform
    {

        public Vector3 Location;
        public Quaternion Rotation;
        public float Scale;

        public Transform(Vector3 location, Quaternion rotation, float scale)
        {
            Location = location;
            Rotation = rotation;
            Scale = scale;
        }

        public Matrix4x4 CreateTransformationMatrix()
        {
            return Matrix4x4.CreateTranslation(Location) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale);
        }

    }
}
