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
    public static ProbeResult File(string fileName!!)
    {
        var path = Path.GetFullPath(fileName);

        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException("Unable to find the specified file.", fileName);
        }

        return Input(path);
    }

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
    public static ProbeResult Http(Uri uri!!)
    {
        if (!uri.IsAbsoluteUri) throw new ArgumentException($"{nameof(uri)} is not absolute.", nameof(uri));
        if (uri.Scheme != System.Uri.UriSchemeHttp && uri.Scheme != System.Uri.UriSchemeHttps) throw new ArgumentException($"{nameof(uri)} is not http/s.", nameof(uri));

        return Input(uri.AbsoluteUri);
    }

    /// <summary>
    ///     Probe the specified uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The probed result data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not absolute.</exception>
    /// <exception cref="AVIOAllocationException">Fail to allocate memory.</exception>
    /// <exception cref="AVIOException">Generic error.</exception>
    public static ProbeResult Uri(Uri uri!!)
    {
        if (!uri.IsAbsoluteUri) throw new ArgumentException($"{nameof(uri)} is not absolute.", nameof(uri));

        return Input(uri.AbsoluteUri);
    }

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
    public static ProbeResult Input(string url!!)
    {
        unsafe
        {
            return ProbeContext(null, url);
        }
    }

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
    public static ProbeResult Stream(System.IO.Stream stream!!)
    {
        if (!stream.CanRead) throw new ArgumentException($"{nameof(stream)} is not readable.", nameof(stream));

        unsafe
        {
            AVFormatContext* fmt_ctx = ffmpeg.avformat_alloc_context();

            if (fmt_ctx == null)
            {
                throw new AVIOAllocationException(nameof(ffmpeg.avformat_alloc_context), message: null);
            }

            using var f = new AVIOReadStreamContext(stream);
            fmt_ctx->pb = f.ToPointer();

            return ProbeContext(fmt_ctx, string.Empty);
        }
    }

    private static unsafe ProbeResult ProbeContext(AVFormatContext* fmt_ctx, string url)
    {
        Debug.Assert(url != null);

        try
        {
            // avformat_open_input call avformat_alloc_context
            _ = ffmpeg.avformat_open_input(&fmt_ctx, url, null, null).ThrowExceptionIfError();
            _ = ffmpeg.avformat_find_stream_info(fmt_ctx, null).ThrowExceptionIfError();

            var formatContext = new AVFormatContextVisitorRes();

            return formatContext.Visit(fmt_ctx);
        }
        finally
        {
            ffmpeg.avformat_close_input(&fmt_ctx);
        }
    }
}

public static partial class FFProbe
{
    public static TResult File<TResult>(string fileName!!, IAVFormatContextVisitor<TResult> visitor!!)
    {
        var path = Path.GetFullPath(fileName);

        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException("Unable to find the specified file.", fileName);
        }

        return Input(path, visitor);
    }
    public static TResult Http<TResult>(Uri uri!!, IAVFormatContextVisitor<TResult> visitor!!)
    {
        if (!uri.IsAbsoluteUri) throw new ArgumentException($"{nameof(uri)} is not absolute.", nameof(uri));
        if (uri.Scheme != System.Uri.UriSchemeHttp && uri.Scheme != System.Uri.UriSchemeHttps) throw new ArgumentException($"{nameof(uri)} is not http/s.", nameof(uri));

        return Input(uri.AbsoluteUri, visitor);
    }
    public static TResult Uri<TResult>(Uri uri!!, IAVFormatContextVisitor<TResult> visitor!!)
    {
        if (!uri.IsAbsoluteUri) throw new ArgumentException($"{nameof(uri)} is not absolute.", nameof(uri));

        return Input(uri.AbsoluteUri, visitor);
    }
    public static TResult Input<TResult>(string url!!, IAVFormatContextVisitor<TResult> visitor!!)
    {
        unsafe
        {
            return ProbeContext(null, url, visitor);
        }
    }
    public static TResult Stream<TResult>(System.IO.Stream stream!!, IAVFormatContextVisitor<TResult> visitor!!)
    {
        if (!stream.CanRead) throw new ArgumentException($"{nameof(stream)} is not readable.", nameof(stream));

        unsafe
        {
            AVFormatContext* fmt_ctx = ffmpeg.avformat_alloc_context();

            if (fmt_ctx == null)
            {
                throw new AVIOAllocationException(nameof(ffmpeg.avformat_alloc_context), message: null);
            }

            using var f = new AVIOReadStreamContext(stream);
            fmt_ctx->pb = f.ToPointer();

            return ProbeContext(fmt_ctx, string.Empty, visitor);
        }
    }

    private static unsafe TResult ProbeContext<TResult>(AVFormatContext* fmt_ctx, string url, IAVFormatContextVisitor<TResult> visitor!!)
    {
        Debug.Assert(url != null);

        try
        {
            // avformat_open_input call avformat_alloc_context
            _ = ffmpeg.avformat_open_input(&fmt_ctx, url, null, null).ThrowExceptionIfError();
            _ = ffmpeg.avformat_find_stream_info(fmt_ctx, null).ThrowExceptionIfError();

            return visitor.Visit(fmt_ctx);
        }
        finally
        {
            ffmpeg.avformat_close_input(&fmt_ctx);
        }
    }
}
