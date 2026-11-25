#version 460
#extension GL_EXT_nonuniform_qualifier : enable

layout(location = 0) out vec4 color;

layout(set = 0, binding = 0) uniform sampler2D  textures[];
layout(set = 0, binding = 0) uniform isampler2D itextures[];

layout(push_constant) uniform Push {
    int sceneIndex;
    int seedIndex;
} pc;

void main()
{
    ivec2 pixel = ivec2(gl_FragCoord.xy);

    vec4 sceneColor = texelFetch(textures[pc.sceneIndex], pixel, 0);
    ivec2 seed = texelFetch(itextures[pc.seedIndex], pixel, 0).xy;

    if (seed.x >= 0)
    {
        vec2 delta = vec2(pixel - seed);
        float distSquared = dot(delta, delta);
        float thickness = 1.0;

        if (distSquared <= thickness * thickness)
        {
            color = vec4(1.0);
            return;
        }
    }

    color = sceneColor;
}