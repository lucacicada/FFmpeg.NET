namespace AVIO;

/// <inheritdoc />
[Serializable]
public class AVIOErrorException : AVIOException
{
    private static string? GetErrorMessage(int error)
    {
        string? message = null;

        if (error < 0)
        {
            var bufferSize = 1024;

            unsafe
            {
                var buffer = stackalloc byte[bufferSize];
                if (ffmpeg.av_strerror(error, buffer, (ulong)bufferSize) == 0)
                {
                    message = Marshal.PtrToStringAnsi((IntPtr)buffer);
                }
            }
        }

        return message;
    }

    /// <summary>
    ///     Represents the AVERROR error code associated with this exception.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref='System.ComponentModel.Win32Exception'/> class with the specified error.
    /// </summary>
    public AVIOErrorException(int error) : this(error, GetErrorMessage(error))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref='System.ComponentModel.Win32Exception'/> class with the specified error and the specified detailed description.
    /// </summary>
    public AVIOErrorException(int error, string? message) : base(message)
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
