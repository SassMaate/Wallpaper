using Sucrose.Mpv.NET.API.Enums;

namespace Sucrose.Mpv.NET.API.Structs
{
    public unsafe struct MpvRenderParam
    {
        public MpvRenderParamType type;
        public void* data;
    }
}