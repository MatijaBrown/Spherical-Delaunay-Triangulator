using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Planets
{
    public class Camera
    {

        private const float EXTREME_PITCH = 89.0f;
        private const float SPEED = 10.0f;

        private const float MOUSE_SENSITIVITY = 0.5f;

        private static readonly Vector3 WORLD_UP = new(0.0f, 1.0f, 0.0f);

        private readonly IReadOnlyList<IKeyboard> _keyboards;
        private readonly IReadOnlyList<IMouse> _mice;

        private Vector3 _position;
        private Vector3 _front, _right, _worldUp, _forward;

        private float _pitch, _yaw;

        private bool _firstMouse;
        private Vector2 _lastMousePos;

        public Vector3 Position
        {
            get => _position;
            set => _position = value;
        }

        public Camera(Vector3 initialLocation, IInputContext input)
        {
            _keyboards = input.Keyboards;
            _mice = input.Mice;

            _position = initialLocation;
            _worldUp = WORLD_UP;

            _pitch = 0.0f;
            _yaw = 0.0f;

            UpdateVectors();

            _firstMouse = true;
        }

        public void ResetPosition()
        {
            Position = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public Matrix4x4 CalculateViewMatrix()
        {
            return Matrix4x4.CreateLookAt(_position, _position + Vector3.Normalize(_front), _worldUp);
        }

        private void UpdateVectors()
        {
            _front = new Vector3(
                x: MathF.Cos(ToRadians(_yaw)) * MathF.Cos(ToRadians(_pitch)),
                y: MathF.Sin(ToRadians(_pitch)),
                z: MathF.Sin(ToRadians(_yaw)) * MathF.Cos(ToRadians(_pitch))
            );

            _front = Vector3.Normalize(_front);

            _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
            _forward = Vector3.Normalize(Vector3.Cross(_right, _worldUp));
        }

        private void CalculateFacingVectorsFromMouse(float xOffset, float yOffset)
        {
            xOffset *= MOUSE_SENSITIVITY;
            yOffset *= MOUSE_SENSITIVITY;

            _yaw += xOffset;
            _pitch += yOffset;

            _pitch = MathF.Min(_pitch, EXTREME_PITCH);
            _pitch = MathF.Max(_pitch, -EXTREME_PITCH);

            if (_yaw > 360.0f)
            {
                _yaw = 0.0f;
            }
            else if (_yaw < 0.0f)
            {
                _yaw = 360.0f;
            }

            UpdateVectors();
        }

        private void UpdateDirections()
        {
            foreach (IMouse ms in _mice)
            {
                Vector2 mousePos = ms.Position;
                if (_firstMouse)
                {
                    _lastMousePos = mousePos;
                    _firstMouse = false;
                }

                float xOffset = mousePos.X - _lastMousePos.X;
                float yOffset = _lastMousePos.Y - mousePos.Y;

                _lastMousePos = mousePos;

                CalculateFacingVectorsFromMouse(xOffset, yOffset);
            }
        }

        private Vector3 DetermineDirectionOfMovement()
        {
            Vector3 direction = Vector3.Zero;

            foreach (IKeyboard kb in _keyboards)
            {
                if (kb.IsKeyPressed(Key.R))
                {
                    ResetPosition();
                }

                if (kb.IsKeyPressed(Key.W))
                {
                    direction += _front;
                }
                if (kb.IsKeyPressed(Key.S))
                {
                    direction -= _front;
                }
                if (kb.IsKeyPressed(Key.A))
                {
                    direction -= _right;
                }
                if (kb.IsKeyPressed(Key.D))
                {
                    direction += _right;
                }
                if (kb.IsKeyPressed(Key.Space))
                {
                    direction += _worldUp;
                }
                if (kb.IsKeyPressed(Key.ShiftLeft))
                {
                    direction -= _worldUp;
                }
            }

            return direction;
        }

        private void Move(float deltaTime, Vector3 velocity)
        {
            Vector3 deltaLoc = deltaTime * velocity;
            _position += deltaLoc;
        }

        public void Update(float delta)
        {
            UpdateDirections();
            Vector3 direction = DetermineDirectionOfMovement();
            Vector3 velocity = direction * SPEED;
            Move(delta, velocity);
        }

        private static float ToRadians(float degrees)
        {
            return degrees * MathF.PI / 180.0f;
        }

    }
}
