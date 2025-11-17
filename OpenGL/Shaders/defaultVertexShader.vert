#version 400 core

in vec3 inPos;
in vec3 inColor;
uniform mat4 inMatrix;
uniform float inTime;
out float fromVertexShaderToFragmentShader;
out vec3 color;

void main()
{
	gl_Position = vec4(inPos, 1.0) + vec4(sin(inTime) * 0.5, cos(inTime) * 0.5, 0.0, 0.0);
	fromVertexShaderToFragmentShader = inPos.x + 0.5;
	color = inColor;
}