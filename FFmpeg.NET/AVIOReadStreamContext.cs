namespace AVIO;

// see: https://stackoverflow.com/a/20610535/18773026
// see: https://github.com/GoaLitiuM/Pulsus/blob/master/Pulsus/FFmpeg/FFmpegContext.cs
// see: https://github.com/Ryujinx/Ryujinx/blob/master/Ryujinx.Graphics.Nvdec.FFmpeg/FFmpegContext.cs
/// <summary>
///     Represents a managed <see cref="AVIOContext" />.
/// </summary>
[DebuggerDisplay("stream")]
public sealed class AVIOReadStreamContext : IDisposable
{
    const int DEFAULT_BUFFER_SIZE = 4 * 1024;

    private readonly avio_alloc_context_read_packet read_packet;
    private readonly avio_alloc_context_seek seek;
    private readonly System.IO.Stream stream;
    private IntPtr ctx;

    /// <summary>
    ///     Creates a new <see cref="AVIOReadStreamContext" /> instance.
    /// </summary>
    public AVIOReadStreamContext(System.IO.Stream stream!!)
        : this(stream, DEFAULT_BUFFER_SIZE)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="AVIOReadStreamContext" /> instance.
    /// </summary>
    public AVIOReadStreamContext(System.IO.Stream stream!!, int bufferSize)
    {
        if (!stream.CanRead) throw new ArgumentException($"{nameof(stream)} is not readable", nameof(stream));
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

        this.stream = stream;

        unsafe
        {
            byte* buffer = (byte*)ffmpeg.av_malloc((ulong)bufferSize + ffmpeg.AV_INPUT_BUFFER_PADDING_SIZE);

            if (buffer == null)
                throw new AVIOAllocationException(nameof(ffmpeg.av_malloc), message: null);

            // prevent the GC from colleting the function pointer
            // TODO: it should be unnecessary however keep the reference for now
            read_packet = new avio_alloc_context_read_packet(ReadUnsafe);
            seek = new avio_alloc_context_seek(SeekUnsafe);

            AVIOContext* ctxPtr = ffmpeg.avio_alloc_context(buffer, bufferSize, 0, null, read_packet, null, seek);

            if (ctxPtr == null)
                throw new AVIOAllocationException(nameof(ffmpeg.avio_alloc_context), message: null);

            ctx = new IntPtr(ctxPtr);
        }
    }

    /// <summary>
    ///     Gets the underlying <see cref="AVIOContext"/>* pointer.
    /// </summary>
    public unsafe AVIOContext* ToPointer() => (AVIOContext*)ctx.ToPointer();

    private unsafe int ReadUnsafe(void* _, byte* buf, int buf_size)
    {
        byte[] array = new byte[buf_size];

        try
        {
            int read = stream.Read(array, 0, buf_size);
            Marshal.Copy(array, 0, (IntPtr)buf, read);
            return read;
        }
        catch
        {
            // we cant throw
        }

        return 0;
    }

    private unsafe long SeekUnsafe(void* _, long offset, int whence)
    {
        if (whence == ffmpeg.AVSEEK_SIZE)
            return stream.Length;

        if (!stream.CanSeek)
            return -1;

        try
        {
            return stream.Seek(offset, (SeekOrigin)whence);
        }
        catch
        {
            // we cant throw
        }

        return -1;
    }

    private bool disposedValue;
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (ctx != IntPtr.Zero)
            {
                unsafe
                {
                    var ctxPtr = (AVIOContext*)ctx.ToPointer();

                    ffmpeg.avio_context_free(&ctxPtr);
                    //ffmpeg.avio_close(ctxPtr);
                    //ffmpeg.av_free(ctxPtr->buffer);
                    //ctxPtr->buffer = null;
                    //// ffmpeg.av_free(ctxPtr);
                    ctx = IntPtr.Zero;
                }
            }

            if (disposing)
            {
                stream.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc />
    ~AVIOReadStreamContext()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
