#version 330 core

layout(location = 0) in vec3 inPos;
layout(location = 1) in vec3 inColor;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec2 inTexCoord;

out vec3 fragPos;      // world-space position
out vec3 fragNormal;   // world-space normal
out vec2 fragTex;
out vec3 vertColor;

uniform mat4 inModelMatrix;    // model matrix (object -> world)
uniform mat4 inNormalMatrix;   // normal matrix (transpose(inverse(model)))
uniform mat4 inMatrix;         // model * viewProjection (provided by caller)

void main()
{
    vec4 worldPos = inModelMatrix * vec4(inPos, 1.0);
    fragPos = worldPos.xyz;
    fragNormal = mat3(inNormalMatrix) * inNormal;
    fragTex = inTexCoord;
    vertColor = inColor;

    gl_Position = inMatrix * vec4(inPos, 1.0);
}