namespace AVIO;

/// <summary>
///     Represents errors that occur during memory allocation.
/// </summary>
[Serializable]
public class AVIOAllocationException : Exception
{
    private static string GetErrorMessage(string? functionName, string? message)
    {
        return message ?? $"Could not allocate {functionName ?? "memory"}.";
    }

    /// <summary>
    ///     The function that have caused the memory allocation error.
    /// </summary>
    public string? FunctionName { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AVIOAllocationException" /> class with a specified error message.
    /// </summary>
    /// <param name="functionName">The function that have caused the memory allocation error.</param>
    /// <param name="message">The message that describes the error.</param>
    public AVIOAllocationException(string? functionName, string? message)
        : base(GetErrorMessage(functionName, message))
    {
        FunctionName = functionName;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AVIOAllocationException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="functionName">The function that have caused the memory allocation error.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public AVIOAllocationException(string? functionName, string? message, Exception? innerException)
        : base(GetErrorMessage(functionName, message), innerException)
    {
        FunctionName = functionName;
    }

    /// <inheritdoc />
    protected AVIOAllocationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        FunctionName = info.GetString(nameof(FunctionName));
    }

    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(FunctionName), FunctionName, typeof(string));
    }
}
