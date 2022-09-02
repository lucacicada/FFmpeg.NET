namespace AVIO;

/// <summary>
///     Represents FFProbe.
/// </summary>
public static partial class FFProbe
{
    /// <summary>
    ///     Probe the specified file.
    /// </summary>
    /// <param name="fileName">The file path.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null.</exception>
    /// <exception cref="FileNotFoundException">The specified file was not found.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static ProbeResult File(string fileName) => File(fileName, new AVFormatContextVisitorRes());

    /// <summary>
    ///     Probe the specified http/s uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not absolute.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not http/s.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static ProbeResult Http(Uri uri) => Http(uri, new AVFormatContextVisitorRes());

    /// <summary>
    ///     Probe the specified uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not absolute.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static ProbeResult Uri(Uri uri) => Uri(uri, new AVFormatContextVisitorRes());

    /// <summary>
    ///     Probe the specified input url.
    /// </summary>
    /// <param name="url">The input url.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="url"/> is null.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    /// <remarks>
    ///     This method does not validate the input url
    /// </remarks>
    public static ProbeResult Input(string url) => Input(url, new AVFormatContextVisitorRes());

    /// <summary>
    ///     Probe the specified stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>The probed result data.</returns>
    /// <remarks>The stream is not diposed.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static ProbeResult Stream(System.IO.Stream stream) => Stream(stream, new AVFormatContextVisitorRes());

    /// <summary>
    ///     Probe the specified file.
    /// </summary>
    /// <param name="fileName">The file path.</param>
    /// <param name="visitor">The visitor.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null.</exception>
    /// <exception cref="FileNotFoundException">The specified file was not found.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static TResult File<TResult>(string fileName, IAVFormatContextVisitor<TResult> visitor)
    {
        if (fileName is null) throw new ArgumentNullException(nameof(fileName));
        if (visitor is null) throw new ArgumentNullException(nameof(visitor));

        var path = Path.GetFullPath(fileName);

        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException("Unable to find the specified file.", fileName);
        }

        return Input(path, visitor);
    }

    /// <summary>
    ///     Probe the specified http/s uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="visitor">The visitor.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not absolute.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not http/s.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static TResult Http<TResult>(Uri uri, IAVFormatContextVisitor<TResult> visitor)
    {
        if (uri is null) throw new ArgumentNullException(nameof(uri));
        if (!uri.IsAbsoluteUri) throw new ArgumentException($"{nameof(uri)} is not absolute.", nameof(uri));
        if (uri.Scheme != System.Uri.UriSchemeHttp && uri.Scheme != System.Uri.UriSchemeHttps) throw new ArgumentException($"{nameof(uri)} is not http/s.", nameof(uri));
        if (visitor is null) throw new ArgumentNullException(nameof(visitor));

        return Input(uri.AbsoluteUri, visitor);
    }

    /// <summary>
    ///     Probe the specified uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="visitor">The visitor.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not absolute.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static TResult Uri<TResult>(Uri uri, IAVFormatContextVisitor<TResult> visitor)
    {
        if (uri is null) throw new ArgumentNullException(nameof(uri));
        if (!uri.IsAbsoluteUri) throw new ArgumentException($"{nameof(uri)} is not absolute.", nameof(uri));
        if (visitor is null) throw new ArgumentNullException(nameof(visitor));

        return Input(uri.AbsoluteUri, visitor);
    }

    /// <summary>
    ///     Probe the specified input url.
    /// </summary>
    /// <param name="url">The input url.</param>
    /// <param name="visitor">The visitor.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="url"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    /// <remarks>
    ///     This method does not validate the input url
    /// </remarks>
    public static TResult Input<TResult>(string url, IAVFormatContextVisitor<TResult> visitor)
    {
        if (url is null) throw new ArgumentNullException(nameof(url));
        if (visitor is null) throw new ArgumentNullException(nameof(visitor));

        unsafe
        {
            return ProbeContext(null, url, visitor);
        }
    }

    /// <summary>
    ///     Probe the specified stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="visitor">The visitor.</param>
    /// <returns>The probed result data.</returns>
    /// <remarks>The stream is not diposed.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static TResult Stream<TResult>(System.IO.Stream stream, IAVFormatContextVisitor<TResult> visitor)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead) throw new ArgumentException($"{nameof(stream)} is not readable.", nameof(stream));
        if (visitor is null) throw new ArgumentNullException(nameof(visitor));

        unsafe
        {
            AVFormatContext* fmt_ctx = ffmpeg.avformat_alloc_context();

            if (fmt_ctx is null)
            {
                throw new AVIOAllocationException(nameof(ffmpeg.avformat_alloc_context), message: null);
            }

            using var f = new AVIOReadStreamContext(stream);
            fmt_ctx->pb = f.ToPointer();

            return ProbeContext(fmt_ctx, string.Empty, visitor);
        }
    }

    private static unsafe TResult ProbeContext<TResult>(AVFormatContext* fmt_ctx, string url, IAVFormatContextVisitor<TResult> visitor)
    {
        Debug.Assert(url is not null);

        try
        {
            // avformat_open_input call avformat_alloc_context
            _ = ffmpeg.avformat_open_input(&fmt_ctx, url, null, null).ThrowExceptionIfError(nameof(ffmpeg.avformat_open_input));
            _ = ffmpeg.avformat_find_stream_info(fmt_ctx, null).ThrowExceptionIfError(nameof(ffmpeg.avformat_find_stream_info));

            return visitor.Visit(fmt_ctx);
        }
        finally
        {
            ffmpeg.avformat_close_input(&fmt_ctx);
        }
    }
}
