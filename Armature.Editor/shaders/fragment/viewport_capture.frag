#version 460
#extension GL_EXT_nonuniform_qualifier : enable

layout(location = 0) out vec4 color;

layout(set = 0, binding = 0) uniform sampler2D textures[];

layout(push_constant) uniform Push {
    layout(offset = 128) int sceneIndex;
    int _pad;
    vec2 invOutputSize;
} pc;

void main()
{
    vec2 uv = gl_FragCoord.xy * pc.invOutputSize;
    color = texture(textures[pc.sceneIndex], uv);
}