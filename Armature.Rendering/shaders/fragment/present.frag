#version 460

#extension GL_EXT_nonuniform_qualifier : require

layout(location = 0) in vec2 inUV;
layout(location = 0) out vec4 outColor;

layout(set = 0, binding = 0) uniform sampler2D textures[];

layout(push_constant) uniform PushConstants
{
    layout(offset = 128) uint textureIndex;
} pc;

void main()
{
    outColor = texture(textures[nonuniformEXT(pc.textureIndex)], inUV);
}