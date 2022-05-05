namespace AVIO;

/// <inheritdoc />
public class AVIOException : Exception
{
    /// <inheritdoc />
    public AVIOException()
    {
    }

    /// <inheritdoc />
    public AVIOException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public AVIOException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected AVIOException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
