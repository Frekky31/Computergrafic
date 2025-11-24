#version 400 core

in vec3 inPos;
in vec3 inColor;
in vec2 inTexCoord;
in vec3 inNormal;
uniform mat4 inMatrix;
uniform float inTime;
uniform sampler2D brickTexture;
out vec3 color;
out vec2 texCoord;
out vec3 pos;
out vec3 normal;

void main()
{
	gl_Position = inMatrix * vec4(inPos, 1.0);
	color = inColor;
	texCoord = inTexCoord;
	pos = inPos;
	normal = inNormal;
}