namespace AVIO;

/// <summary>
///    Represents a visitor for <see cref="AVFormatContext"/> pointers.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface IAVFormatContextVisitor<out TResult>
{
    /// <summary>
    ///     Visit the <see cref="AVFormatContext"/> pointer.
    /// </summary>
    /// <param name="formatContext">The <see cref="AVFormatContext"/> pointer.</param>
    /// <returns>A <typeparamref name="TResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatContext"/> is null.</exception>
    unsafe TResult Visit(AVFormatContext* formatContext);
}
