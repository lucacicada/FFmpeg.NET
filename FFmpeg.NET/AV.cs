namespace AVIO;

/// <summary>
///     Represents libav.
/// </summary>
public static class AV
{
    /// <summary>
    ///     Gets or sets the libav log level.
    /// </summary>
    /// <remarks>Alias for <see cref="ffmpeg.av_log_get_level"/> and <see cref="ffmpeg.av_log_set_level(int)"/>.</remarks>
    public static AVLogLevel LogLevel
    {
        get
        {
            return (AVLogLevel)ffmpeg.av_log_get_level();
        }
        set
        {
            ffmpeg.av_log_set_level((int)value);
        }
    }

    /// <summary>
    ///     Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="AVDictionary"/>* pointer.
    /// </summary>
    /// <param name="dictionary">The AV dictionary pointer.</param>
    /// <returns>An empty dictionary if the pointer is <see langword="null" />. </returns>
    public static unsafe Dictionary<string, string> ToDictionary(AVDictionary* dictionary)
    {
        var result = new Dictionary<string, string>();

        if (dictionary is not null)
        {
            AVDictionaryEntry* tag = null;
            while ((tag = ffmpeg.av_dict_get(dictionary, string.Empty, tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) is not null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key)!;
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value)!;

                // this will throw if the av_dict support duplicate key
                result.Add(key, value);
            }
        }

        return result;
    }

    /// <summary>
    ///     Convert a time value into a <see cref="TimeSpan" /> using <see cref="ffmpeg.AV_TIME_BASE"/> as time base.
    /// </summary>
    public static TimeSpan? TimeSpanTime(long ts) => ToTimeSpan(ts, ffmpeg.AV_TIME_BASE);

    /// <summary>
    ///     Convert a time value into a <see cref="TimeSpan" />.
    /// </summary>
    public static TimeSpan? ToTimeSpan(long ts, int timeBase) => ToTimeInternal(ts, timeBase, is_duration: false);

    /// <summary>
    ///     Convert a time value into a <see cref="TimeSpan" />.
    /// </summary>
    public static TimeSpan? ToTimeSpan(long ts, AVRational timeBase) => ToTimeInternal(ts, timeBase, is_duration: false);

    /// <summary>
    ///     Convert a time value into a <see cref="TimeSpan" /> duration.
    /// </summary>
    public static TimeSpan? ToTimeSpanDuration(long ts, AVRational timeBase) => ToTimeInternal(ts, timeBase, is_duration: true);

    //
    private static TimeSpan? ToTimeInternal(long ts, int time_base, bool is_duration = false)
    {
        // sanity check
        if (time_base == 0)
        {
            return null;
        }

        if ((!is_duration && ts == ffmpeg.AV_NOPTS_VALUE) || (is_duration && ts == 0))
        {
            return null;
        }

        // use decimal.Divide to avoid precision loss
        // ts / time_base will get fractional seconds, so * by TicksPerSecond to get ticks
        return new TimeSpan((long)(decimal.Divide(ts, time_base) * TimeSpan.TicksPerSecond));
    }

    private static TimeSpan? ToTimeInternal(long ts, AVRational time_base, bool is_duration = false)
    {
        // sanity check
        if (time_base.den == 0)
        {
            return null;
        }

        if ((!is_duration && ts == ffmpeg.AV_NOPTS_VALUE) || (is_duration && ts == 0))
        {
            return null;
        }

        return new TimeSpan((long)(ts * decimal.Divide(time_base.num, time_base.den) * TimeSpan.TicksPerSecond));
    }
}
