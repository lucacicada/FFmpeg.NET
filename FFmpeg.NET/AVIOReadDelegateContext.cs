namespace AVIO;

/// <summary>
///     Represents a managed <see cref="AVIOContext" />.
/// </summary>
public abstract class AVIOReadDelegateContext : IDisposable
{
    const int DEFAULT_BUFFER_SIZE = 4 * 1024;

    private readonly avio_alloc_context_read_packet read_packet;
    private readonly avio_alloc_context_seek seek;
    private bool disposedValue;
    private IntPtr ctx;

    /// <summary>
    ///     Creates a new <see cref="AVIOReadDelegateContext" /> instance.
    /// </summary>
    public AVIOReadDelegateContext(int bufferSize)
    {
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

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

    private unsafe int ReadUnsafe(void* _, byte* buf, int buf_size)
    {
        byte[] array = new byte[buf_size];

        try
        {
            int read = Read(array, 0, buf_size);
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
        try
        {
            if (whence == ffmpeg.AVSEEK_SIZE)
                return GetLength();

            return Seek(offset, (SeekOrigin)whence);
        }
        catch
        {
            // we cant throw
        }

        return -1;
    }

    /// <summary>
    ///     Gets the underlying <see cref="AVIOContext"/>* pointer.
    /// </summary>
    public unsafe AVIOContext* ToPointer() => (AVIOContext*)ctx.ToPointer();

    /// <summary>
    ///     Gets the length in bytes of the context.
    /// </summary>
    protected abstract long GetLength();

    /// <summary>
    ///     Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    protected abstract int Read(byte[] buffer, int offset, int count);

    /// <summary>
    ///     Sets the position within the context, -1 if the context is not seekable.
    /// </summary>
    protected abstract long Seek(long offset, SeekOrigin origin);

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (ctx != IntPtr.Zero)
            {
                unsafe
                {
                    var ctxPtr = (AVIOContext*)ctx.ToPointer();
                    ffmpeg.avio_context_free(&ctxPtr);
                    ctx = IntPtr.Zero;
                }
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc />
    ~AVIOReadDelegateContext()
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
