#version 410 core

in vec3 fragPos;
in vec3 fragNormal;
in vec2 fragTex;
in vec3 vertColor;

out vec4 outColor;

// Material struct must match names used by Material.Apply(...)
struct Material {
    vec3 color;
    float shininess;
    float specularStrength;
    sampler2D texture;
    int hasTexture;
};
uniform Material material;

// Lights (must match upload names from SceneGraphNode)
struct Light {
    vec3 position;
    vec3 color;
    float intensity;
};
uniform int lightCount;
uniform Light lights[8];

// camera / scene
uniform vec3 viewPos;
uniform vec3 ambientColor = vec3(0.08, 0.08, 0.08);
uniform float ambientIntensity = 1.0;

void main()
{
    vec3 N = normalize(fragNormal);

    // sample texture (may contain alpha) or use color
    vec4 texCol = vec4(0.0, 0.0, 0.0, 1.0);
    if (material.hasTexture == 1)
        texCol = texture(material.texture, fragTex);

    vec3 baseColor = material.hasTexture == 1 ? texCol.rgb : material.color * vertColor;
    if (!any(notEqual(vertColor, vec3(0.0)))) // if vertColor is zero, still allow material.color
        baseColor = material.hasTexture == 1 ? texCol.rgb : material.color;

    float alpha = material.hasTexture == 1 ? texCol.a : 1.0;

    // ambient
    vec3 result = ambientColor * ambientIntensity * baseColor;

    vec3 V = normalize(viewPos - fragPos);

    // accumulate lights
    for (int i = 0; i < lightCount; ++i)
    {
        Light L = lights[i];
        vec3 Ldir = normalize(L.position - fragPos); // point lights (SceneGraphNode uploads world-space positions)
        vec3 radiance = L.color * L.intensity;

        // diffuse
        float diff = max(dot(N, Ldir), 0.0);
        vec3 diffuse = radiance * diff;

        // specular (Blinn-Phong)
        vec3 H = normalize(Ldir + V);
        float spec = 0.0;
        if (diff > 0.0)
            spec = pow(max(dot(N, H), 0.0), max(material.shininess, 1.0)) * material.specularStrength;

        result += (diffuse + vec3(spec)) * baseColor;
    }

    // output with alpha from texture when present
    // option: discard tiny alpha fragments (uncomment if you want cutout behavior)
    // if (alpha < 0.01) discard;
    outColor = vec4(result, alpha);
}