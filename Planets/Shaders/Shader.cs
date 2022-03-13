using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Planets.Shaders
{
    public class Shader : IDisposable
    {

        private readonly IDictionary<string, int> _uniformLocations = new Dictionary<string, int>();

        private readonly uint _vertexShaderID;
        private readonly uint _fragmentShaderID;
        private readonly uint _programmeID;

        private readonly GL _gl;

        public Shader(string vertexShaderSource, string fragmentShaderSource, GL gl)
        {
            _gl = gl;

            _vertexShaderID = LoadShader(ShaderType.VertexShader, vertexShaderSource);
            _fragmentShaderID = LoadShader(ShaderType.FragmentShader, fragmentShaderSource);

            _programmeID = _gl.CreateProgram();
            _gl.AttachShader(_programmeID, _vertexShaderID);
            _gl.AttachShader(_programmeID, _fragmentShaderID);
            _gl.LinkProgram(_programmeID);

            _gl.GetProgram(_programmeID, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                throw new Exception("Failed to link programme!");
            }
        }

        private uint LoadShader(ShaderType shaderType, string source)
        {
            string code = File.ReadAllText("./Assets/Shaders/" + source);

            uint id = _gl.CreateShader(shaderType);
            _gl.ShaderSource(id, code);
            _gl.CompileShader(id);

            string infoLog = _gl.GetShaderInfoLog(id);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine(infoLog);
                throw new Exception("Failed to compile shader!");
            }

            return id;
        }

        public void Start()
        {
            _gl.UseProgram(_programmeID);
        }

        public void Stop()
        {
            _gl.UseProgram(0);
        }

        private int UniformLocation(string name)
        {
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                return location;
            }

            int loc = _gl.GetUniformLocation(_programmeID, name);
            _uniformLocations.Add(name, loc);

            return loc;
        }
        public void LoadFloat(string name, float data)
        {
            _gl.Uniform1(UniformLocation(name), data);
        }

        public void LoadVector(string name, Vector2 data)
        {
            _gl.Uniform2(UniformLocation(name), data);
        }
        public void LoadVector(string name, Vector3 data)
        {
            _gl.Uniform3(UniformLocation(name), data);
        }
        public unsafe void LoadMatrix(string name, Matrix4x4 data)
        {
            _gl.UniformMatrix4(UniformLocation(name), 1, false, (float*)&data);
        }

        public void Dispose()
        {
            Stop();

            _gl.DetachShader(_programmeID, _vertexShaderID);
            _gl.DetachShader(_programmeID, _fragmentShaderID);

            _gl.DeleteShader(_vertexShaderID);
            _gl.DeleteShader(_fragmentShaderID);

            _gl.DeleteProgram(_programmeID);
        }

    }
}
