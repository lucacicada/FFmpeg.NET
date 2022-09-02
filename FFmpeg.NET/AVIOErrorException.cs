namespace AVIO;

/// <inheritdoc />
[Serializable]
public class AVIOErrorException : AVIOException
{
    /// <summary>
    ///     Returns a description of the AVERROR code, if such description is available, null otherwise.
    /// </summary>
    /// <param name="error">The AVERROR code.</param>
    /// <returns>A string describing the AVERROR code.</returns>
    /// <remarks>
    ///     It always return <see langword="null" /> if the error is a non negative number.
    ///     It is an alias for <see cref="ffmpeg.av_strerror(int, byte*, ulong)" />.
    /// </remarks>
    public static string? GetErrorMessage(int error)
    {
        const int BUFFER_SIZE = 1024;

        if (error < 0)
        {
            unsafe
            {
                var buffer = stackalloc byte[BUFFER_SIZE];
                if (ffmpeg.av_strerror(error, buffer, (ulong)BUFFER_SIZE) == 0)
                {
                    return Marshal.PtrToStringAnsi((IntPtr)buffer);
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Represents the AVERROR error code associated with this exception.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AVIOErrorException"/> class with the specified error.
    /// </summary>
    public AVIOErrorException(int error) : this(error, GetErrorMessage(error))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AVIOErrorException"/> class with the specified error and the specified detailed description.
    /// </summary>
    public AVIOErrorException(int error, string? message) : base(message ?? GetErrorMessage(error))
    {
        ErrorCode = error;
    }

    /// <inheritdoc />
    protected AVIOErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ErrorCode = info.GetInt32(nameof(ErrorCode));
    }

    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }
}
