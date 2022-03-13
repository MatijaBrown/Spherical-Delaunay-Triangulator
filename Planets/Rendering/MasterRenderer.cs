using Planets.Entities;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Planets.Rendering
{
    public class MasterRenderer : IDisposable
    {

        public const float FIELD_OF_VIEW = 70.0f;
        public const float FIELD_OF_VIEW_RADS = FIELD_OF_VIEW / 180.0f * MathF.PI;

        public const float NEAR_PLANE = 0.1f;
        public const float FAR_PLANE = 1000.0f;

        private readonly IList<Entity> _entities = new List<Entity>();
        private readonly EntityRenderer _entityRenderer;

        private readonly IList<Planet> _planets = new List<Planet>();
        private readonly PlanetRenderer _planetRenderer;

        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

        private readonly GL _gl;

        public MasterRenderer(GL gl)
        {
            _gl = gl;

            _entityRenderer = new EntityRenderer(_gl);
            _planetRenderer = new PlanetRenderer(_gl);
        }

        public void UpdateSize(float newWidth, float newHeight)
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FIELD_OF_VIEW_RADS, newWidth / newHeight, NEAR_PLANE, FAR_PLANE);
        }

        public void RenderEntity(Entity mesh)
        {
            _entities.Add(mesh);
        }

        public void RenderPlanet(Planet mesh)
        {
            _planets.Add(mesh);
        }

        public void Render(Camera camera)
        {
            foreach (Entity entity in _entities)
            {
                _entityRenderer.Render(entity, camera, _projectionMatrix);
            }
            _entities.Clear();

            foreach (Planet planet in _planets)
            {
                _planetRenderer.Render(planet, camera, _projectionMatrix);
            }
            _planets.Clear();
        }

        public void Dispose()
        {
            _entityRenderer.Dispose();
            _planetRenderer.Dispose();
        }

    }
}
