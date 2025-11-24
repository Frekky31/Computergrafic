#version 400 core

in vec3 color;
in vec2 texCoord;
out vec4 outColor;
uniform sampler2D brickTexture;

void main()
{
	outColor = texture(brickTexture, texCoord);
	//outColor = vec4(fromVertexShaderToFragmentShader, sin(gl_FragCoord.x / 4) * 0.5 + 0.5, 0.0, 1.0);
}