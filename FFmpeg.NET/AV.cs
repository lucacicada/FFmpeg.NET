namespace AVIO;

/// <summary>
///     Represents libav.
/// </summary>
public static class AV
{
    /// <summary>
    ///     Gets or sets the libav* log level.
    /// </summary>
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
    /// <returns>An emty dictionary if the pointer is <see langword="null" />. </returns>
    public static unsafe Dictionary<string, string> ToDictionary(AVDictionary* dictionary)
    {
        var result = new Dictionary<string, string>();

        if (dictionary != null)
        {
            AVDictionaryEntry* tag = null;
            while ((tag = ffmpeg.av_dict_get(dictionary, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
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

    /// <summary>
    ///     Provides a description to the channel layout.
    /// </summary>
    // see: https://github.com/FFmpeg/FFmpeg/blob/870bfe16a12bf09dca3a4ae27ef6f81a2de80c40/libavutil/channel_layout.c#L183
    public static string GetChannelLayoutName(int channelsCount, ulong channelLayout)
    {
        if (channelsCount <= 0)
            channelsCount = ffmpeg.av_get_channel_layout_nb_channels(channelLayout);

        ulong AV_CH_LAYOUT_HEXADECAGONAL =
            ffmpeg.AV_CH_LAYOUT_OCTAGONAL
            | ffmpeg.AV_CH_WIDE_LEFT
            | ffmpeg.AV_CH_WIDE_RIGHT
            | ffmpeg.AV_CH_TOP_BACK_LEFT
            | ffmpeg.AV_CH_TOP_BACK_RIGHT
            | ffmpeg.AV_CH_TOP_BACK_CENTER
            | ffmpeg.AV_CH_TOP_FRONT_CENTER
            | ffmpeg.AV_CH_TOP_FRONT_LEFT
            | ffmpeg.AV_CH_TOP_FRONT_RIGHT;

        ulong AV_CH_LAYOUT_22POINT2 = ffmpeg.AV_CH_LAYOUT_5POINT1_BACK
            | ffmpeg.AV_CH_FRONT_LEFT_OF_CENTER
            | ffmpeg.AV_CH_FRONT_RIGHT_OF_CENTER
            | ffmpeg.AV_CH_BACK_CENTER
            | ffmpeg.AV_CH_LOW_FREQUENCY_2
            | ffmpeg.AV_CH_SIDE_LEFT
            | ffmpeg.AV_CH_SIDE_RIGHT
            | ffmpeg.AV_CH_TOP_FRONT_LEFT
            | ffmpeg.AV_CH_TOP_FRONT_RIGHT
            | ffmpeg.AV_CH_TOP_FRONT_CENTER
            | ffmpeg.AV_CH_TOP_CENTER
            | ffmpeg.AV_CH_TOP_BACK_LEFT
            | ffmpeg.AV_CH_TOP_BACK_RIGHT
            | ffmpeg.AV_CH_TOP_SIDE_LEFT
            | ffmpeg.AV_CH_TOP_SIDE_RIGHT
            | ffmpeg.AV_CH_TOP_BACK_CENTER
            | ffmpeg.AV_CH_BOTTOM_FRONT_CENTER
            | ffmpeg.AV_CH_BOTTOM_FRONT_LEFT
            | ffmpeg.AV_CH_BOTTOM_FRONT_RIGHT;

        (string, int, ulong)[] channel_layout_map = new (string, int, ulong)[] {
            ("mono", 1, ffmpeg.AV_CH_LAYOUT_MONO),
            ("stereo", 2, ffmpeg.AV_CH_LAYOUT_STEREO),
            ("2.1", 3, ffmpeg.AV_CH_LAYOUT_2POINT1),
            ("3.0", 3, ffmpeg.AV_CH_LAYOUT_SURROUND),
            ("3.0(back)", 3, ffmpeg.AV_CH_LAYOUT_2_1),
            ("4.0", 4, ffmpeg.AV_CH_LAYOUT_4POINT0),
            ("quad", 4, ffmpeg.AV_CH_LAYOUT_QUAD),
            ("quad(side)", 4, ffmpeg.AV_CH_LAYOUT_2_2),
            ("3.1", 4, ffmpeg.AV_CH_LAYOUT_3POINT1),
            ("5.0", 5, ffmpeg.AV_CH_LAYOUT_5POINT0_BACK),
            ("5.0(side)", 5, ffmpeg.AV_CH_LAYOUT_5POINT0),
            ("4.1", 5, ffmpeg.AV_CH_LAYOUT_4POINT1),
            ("5.1", 6, ffmpeg.AV_CH_LAYOUT_5POINT1_BACK),
            ("5.1(side)", 6, ffmpeg.AV_CH_LAYOUT_5POINT1),
            ("6.0", 6, ffmpeg.AV_CH_LAYOUT_6POINT0),
            ("6.0(front)", 6, ffmpeg.AV_CH_LAYOUT_6POINT0_FRONT),
            ("hexagonal", 6, ffmpeg.AV_CH_LAYOUT_HEXAGONAL),
            ("6.1", 7, ffmpeg.AV_CH_LAYOUT_6POINT1),
            ("6.1(back)", 7, ffmpeg.AV_CH_LAYOUT_6POINT1_BACK),
            ("6.1(front)", 7, ffmpeg.AV_CH_LAYOUT_6POINT1_FRONT),
            ("7.0", 7, ffmpeg.AV_CH_LAYOUT_7POINT0),
            ("7.0(front)", 7, ffmpeg.AV_CH_LAYOUT_7POINT0_FRONT),
            ("7.1", 8, ffmpeg.AV_CH_LAYOUT_7POINT1),
            ("7.1(wide)", 8, ffmpeg.AV_CH_LAYOUT_7POINT1_WIDE_BACK),
            ("7.1(wide-side)", 8, ffmpeg.AV_CH_LAYOUT_7POINT1_WIDE),
            ("octagonal", 8, ffmpeg.AV_CH_LAYOUT_OCTAGONAL),
            ("hexadecagonal", 16, AV_CH_LAYOUT_HEXADECAGONAL ),
            ("downmix", 2, ffmpeg.AV_CH_LAYOUT_STEREO_DOWNMIX),
            ("22.2", 24, AV_CH_LAYOUT_22POINT2),
        };

        foreach ((string, int, ulong) data in channel_layout_map)
        {
            var __name = data.Item1;
            var __nb_channels = data.Item2;
            var __layout = data.Item3;

            if (channelsCount == __nb_channels && channelLayout == __layout)
            {
                return __name;
            }
        }

        StringBuilder stringBuilder = new();

        stringBuilder.Append($"{channelsCount} channels");

        if (channelLayout != 0)
        {
            stringBuilder.Append(" (");

            for (int i = 0, ch = 0; i < 64; i++)
            {
                if ((channelLayout & (ffmpeg.UINT64_C(1) << i)) != 0)
                {
                    string? name = GetChannelName(i);

                    if (name != null)
                    {
                        if (ch > 0)
                            stringBuilder.Append('+');

                        stringBuilder.Append(name);
                    }
                    ch++;
                }
            }

            stringBuilder.Append(')');
        }

        return stringBuilder.ToString();

    }

    /// <summary>
    ///     Provides a name to the channel id.
    /// </summary>
    // see: https://github.com/FFmpeg/FFmpeg/blob/870bfe16a12bf09dca3a4ae27ef6f81a2de80c40/libavutil/channel_layout.c#L183
    public static string? GetChannelName(int channelId)
    {
        // sanity check
        if (channelId < 0 || channelId > 40)
            return null;

        var channel_names = new Dictionary<int, (string, string)>()
        {
            { 0,  ( "FL",       "front left"            )},
            { 1,  ( "FR",       "front right"           )},
            { 2,  ( "FC",       "front center"          )},
            { 3,  ( "LFE",      "low frequency"         )},
            { 4,  ( "BL",       "back left"             )},
            { 5,  ( "BR",       "back right"            )},
            { 6,  ( "FLC",      "front left-of-center"  )},
            { 7,  ( "FRC",      "front right-of-center" )},
            { 8,  ( "BC",       "back center"           )},
            { 9,  ( "SL",       "side left"             )},
            { 10, ( "SR",       "side right"            ) },
            { 11, ( "TC",       "top center"            ) },
            { 12, ( "TFL",      "top front left"        ) },
            { 13, ( "TFC",      "top front center"      ) },
            { 14, ( "TFR",      "top front right"       ) },
            { 15, ( "TBL",      "top back left"         ) },
            { 16, ( "TBC",      "top back center"       ) },
            { 17, ( "TBR",      "top back right"        ) },
            { 29, ( "DL",       "downmix left"          ) },
            { 30, ( "DR",       "downmix right"         ) },
            { 31, ( "WL",       "wide left"             ) },
            { 32, ( "WR",       "wide right"            ) },
            { 33, ( "SDL",      "surround direct left"  ) },
            { 34, ( "SDR",      "surround direct right" ) },
            { 35, ( "LFE2",     "low frequency 2"       ) },
            { 36, ( "TSL",      "top side left"         ) },
            { 37, ( "TSR",      "top side right"        ) },
            { 38, ( "BFC",      "bottom front center"   ) },
            { 39, ( "BFL",      "bottom front left"     ) },
            { 40, ( "BFR",      "bottom front right"    ) },
        };

        if (channel_names.TryGetValue(channelId, out var channel))
        {
            return channel.Item1;
        }

        return null;
    }
}
