using System.Runtime.InteropServices;

namespace Sucrose.Mpv.NET.API.Structs
{
    /// <summary>
    /// <para>Deprecated. For MPV_RENDER_PARAM_DRM_DISPLAY.</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MpvOpenglDrmParams
    {
        public int fd;
        public int crtc_id;
        public int connector_id;
        public DrmModeAtomicReq** atomic_request_ptr;
        public int render_fd;
    }
}