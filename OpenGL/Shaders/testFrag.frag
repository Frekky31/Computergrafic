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
uniform float inTime;

// Equivalent to the helper in the original shader
float cosRange(float amt, float range, float minimum)
{
    return (((1.0 + cos(radians(amt))) * 0.5) * range) + minimum;
}

void main()
{
    vec2  uResolution = vec2(720, 480);
    const int zoom = 60;
    const float brightness = 0.975;

    float time = (inTime-300) * 1.25;
    vec2 fragCoord = gl_FragCoord.xy;
    vec2 uv = fragCoord / uResolution;
    vec2 p = (2.0 * fragCoord - uResolution) / max(uResolution.x, uResolution.y);

    float ct     = cosRange(time * 5.0, 3.0, 1.1);
    float xBoost = cosRange(time * 0.4, 5.0, 5.0);
    float yBoost = cosRange(time * 0.1, 10.0, 5.0);
    float fScale = cosRange(time * 15.5, 1.25, 0.5);

    // Main distortion loop
    for (int i = 1; i < zoom; i++)
    {
        float fi = float(i);
        vec2 newp = p;

        newp.x += 0.25 / fi * sin(fi * p.y + time * cos(ct) * 0.5 / 20.0 + 0.005 * fi) * fScale + xBoost;
        newp.y += 0.25 / fi * sin(fi * p.x + time * ct * 0.3 / 40.0 + 0.03 * float(i + 15)) * fScale + yBoost;

        p = newp;
    }

    // Color
    vec3 col = vec3(
        0.5 * sin(3.0 * p.x) + 0.5,
        0.5 * sin(3.0 * p.y) + 0.5,
        sin(p.x + p.y)
    );

    col *= brightness;

    // Vignette / extrusion
    float vigAmt = 5.0;
    float vignette =
        (1.0 - vigAmt * pow(uv.y - 0.5, 2.0)) *
        (1.0 - vigAmt * pow(uv.x - 0.5, 2.0));

    float extrusion = (col.x + col.y + col.z) / 4.0;
    extrusion *= 1.5;
    extrusion *= vignette;

    outColor = vec4(col * material.color, extrusion);
    outColor *= texture(material.texture, fragTex);
}
