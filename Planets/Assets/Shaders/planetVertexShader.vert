#version 450

layout(location = 0) in vec3 position;

layout(location = 0) out vec3 colour;

uniform float radius;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main(void) {
    gl_Position = projectionMatrix * viewMatrix * transformationMatrix * vec4(position * radius, 1.0);

    colour = vec3(1.0, 0.0, 0.0);
}