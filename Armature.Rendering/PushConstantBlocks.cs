using System.Runtime.InteropServices;
using Brutal.Numerics;

namespace Armature.Rendering;

public static class PushConstantBlocks
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct ImGuiBlock
    {
        [FieldOffset(0)]
        public float2 Scale;
        
        [FieldOffset(8)]
        public float2 Translate;
    }

    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct VertexBlock
    {
        [FieldOffset(0)]
        public float4x4 Mvp;
        
        [FieldOffset(64)]
        public float4x4 Model;
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct TextureFragmentBlock
    {
        [FieldOffset(0)]
        public uint TextureId;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct ColorFragmentBlock
    {
        [FieldOffset(0)]
        public float3 Color;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct JumpPassBlock
    {
        [FieldOffset(0)]
        public uint JumpDistance;
    }
        
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct EntityIdBlock
    {
        [FieldOffset(0)]
        public uint Id;
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct AtmosphereBlock
    {
        [FieldOffset(0)]
        public float3 Center;

        [FieldOffset(12)] 
        public float Radius;
        
        [FieldOffset(16)] 
        public float3 LightDirection;

        [FieldOffset(28)] 
        public uint Enabled; // 0 or 1
        
        [FieldOffset(32)] 
        public float3 LightColor;
        
        [FieldOffset(44)] 
        public float LightIntensity;
    }
}