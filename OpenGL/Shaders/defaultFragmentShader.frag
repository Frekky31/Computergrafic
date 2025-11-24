#version 400 core

in vec3 color;
in vec2 texCoord;
in vec3 normal;
in vec3 pos;
out vec4 outColor;
uniform sampler2D brickTexture;
uniform float inTime;

void main()
{
	outColor = texture(brickTexture, texCoord);
	//outColor = vec4(fromVertexShaderToFragmentShader, sin(gl_FragCoord.x / 4) * 0.5 + 0.5, 0.0, 1.0);
}