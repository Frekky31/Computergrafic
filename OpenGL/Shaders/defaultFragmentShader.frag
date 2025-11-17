#version 400 core

in float fromVertexShaderToFragmentShader;
in vec3 color;
out vec4 outColor;

void main()
{
	outColor = vec4(color, 1.0);
	//outColor = vec4(fromVertexShaderToFragmentShader, sin(gl_FragCoord.x / 4) * 0.5 + 0.5, 0.0, 1.0);
}