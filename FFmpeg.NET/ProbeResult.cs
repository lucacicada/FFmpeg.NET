namespace AVIO;

using System.Text.Json.Serialization;

[DebuggerDisplay("{FormatLongName}")]
public class ProbeResult
{
    [JsonPropertyName("programs")]
    public ImmutableArray<Program> Programs { get; set; }

    [JsonPropertyName("streams")]
    public ImmutableArray<Stream> Streams { get; set; }

    [JsonPropertyName("chapters")]
    public ImmutableArray<Chapter> Chapters { get; set; }

    [JsonPropertyName("format_name")]
    public string? FormatName { get; set; }

    [JsonPropertyName("format_long_name")]
    public string? FormatLongName { get; set; }

    [JsonPropertyName("extensions")]
    public string? Extensions { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("start_time")]
    public TimeSpan? StartTime { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; } // always > 0, 0 = not initialized

    [JsonPropertyName("bit_rate")]
    public long BitRate { get; set; } // bit per second, always > 0, 0 = not initialized

    [JsonPropertyName("probe_score")]
    public int ProbeScore { get; set; } // 0 to 100, 100 is max score

    [JsonPropertyName("tags")]
    public IImmutableDictionary<string, string>? Tags { get; set; }

    [JsonIgnore]
    public string? Encoder => Tags?.AVGet(nameof(Encoder));

    [JsonIgnore]
    public IEnumerable<VideoStream> Videos => Streams.OfType<VideoStream>();

    [JsonIgnore]
    public IEnumerable<AudioStream> Audios => Streams.OfType<AudioStream>();

    [JsonIgnore]
    public IEnumerable<SubtitleStream> Subtitles => Streams.OfType<SubtitleStream>();
}

[DebuggerDisplay("Program: {Id}, {Num}")]
public class Program
{
    /// <summary> Gets the program id. </summary>
    [JsonPropertyName("program_id")]
    public int Id { get; set; }

    /// <summary> Gets the program number. </summary>
    [JsonPropertyName("program_num")]
    public int Num { get; set; }

    /// <summary> Gets the Program Mapping Table. </summary>
    [JsonPropertyName("pmt_pid")]
    public int MappingTablePid { get; set; }

    /// <summary> Gets the Program Clock Reference. </summary>
    [JsonPropertyName("pcr_pid")]
    public int ClockReferencePid { get; set; }

    /// <summary> Gets the program tags. </summary>
    [JsonPropertyName("tags")]
    public IImmutableDictionary<string, string>? Tags { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Program: {Id}, {Num}";
    }
}

[DebuggerDisplay("Chapter: {Id}, {Title}")]
public class Chapter
{
    /// <summary> Gets the chapter id. </summary>
    [JsonPropertyName("chapter_id")]
    public long Id { get; set; }

    /// <summary> Gets the start time, if any. </summary>
    [JsonPropertyName("start")]
    public TimeSpan? Start { get; set; }

    /// <summary> Gets the end time, if any. </summary>
    [JsonPropertyName("end")]
    public TimeSpan? End { get; set; }

    /// <summary> Gets the chapter tags. </summary>
    [JsonPropertyName("tags")]
    public IImmutableDictionary<string, string>? Tags { get; set; }

    /// <summary> Gets the chapter title, extracted from the tags. </summary>
    /// <remarks> This information is extracted from the tag 'Title'. </remarks>
    [JsonIgnore]
    public string? Title => Tags?.AVGet(nameof(Title));

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Chapter: {Id}, {Title}";
    }
}

[DebuggerDisplay("Stream: {Id}, {CodecType}")]
public class Stream
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    // codec
    [JsonPropertyName("codec_id")]
    public AVCodecID CodecId { get; set; }

    [JsonPropertyName("codec_name")]
    public string? CodecName { get; set; }

    [JsonPropertyName("codec_long_name")]
    public string? CodecLongName { get; set; }

    [JsonPropertyName("codec_type")]
    public AVMediaType CodecType { get; set; }

    [JsonPropertyName("codec_tag")]
    public uint CodecTag { get; set; }

    [JsonPropertyName("codec_tag_string")]
    public string? CodecTagString { get; set; }

    [JsonPropertyName("codec_properties")]
    public uint CodecProperties { get; set; }

    [JsonPropertyName("profile")]
    public int Profile { get; set; }

    [JsonPropertyName("profile_name")]
    public string? ProfileName { get; set; }

    [JsonPropertyName("frame_rate")]
    public AVRational FrameRate { get; set; }

    [JsonPropertyName("avg_frame_rate")]
    public AVRational AverageFrameRate { get; set; }

    [JsonPropertyName("start_time")]
    public TimeSpan? StartTime { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration { get; set; }

    [JsonPropertyName("bit_rate")]
    public long BitRate { get; set; }

    [JsonPropertyName("max_bit_rate")]
    public long MaxBitRate { get; set; }

    [JsonPropertyName("bits_per_raw_sample")]
    public int BitsPerRawSample { get; set; }

    [JsonPropertyName("nb_frames")]
    public long FrameCount { get; set; }

    [JsonPropertyName("disposition")]
    public int Disposition { get; set; }

    [JsonPropertyName("extradata")]
    public byte[]? Extradata { get; set; }

    [JsonPropertyName("private_data")]
    public IImmutableDictionary<string, string>? PrivateData { get; set; }

    [JsonPropertyName("tags")]
    public IImmutableDictionary<string, string>? Tags { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Stream: {Id}, {CodecType}";
    }
}

[DebuggerDisplay("Video: {Width}x{Height}, {CodecId}")]
public class VideoStream : Stream
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("coded_width")]
    public int CodedWidth { get; set; }

    [JsonPropertyName("coded_height")]
    public int CodedHeight { get; set; }

    [JsonPropertyName("video_delay")]
    public int VideoDelay { get; set; }

    [JsonPropertyName("sample_aspect_ratio")]
    public AVRational SampleAspectRatio { get; set; }

    [JsonPropertyName("display_aspect_ratio")]
    public AVRational DisplayAspectRatio { get; set; }

    [JsonPropertyName("pix_fmt")]
    public AVPixelFormat PixelFormat { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("color_range")]
    public AVColorRange ColorRange { get; set; }

    [JsonPropertyName("color_space")]
    public AVColorSpace ColorSpace { get; set; }

    [JsonPropertyName("color_transfer")]
    public AVColorTransferCharacteristic ColorTransfer { get; set; }

    [JsonPropertyName("color_primaries")]
    public AVColorPrimaries ColorPrimaries { get; set; }

    [JsonPropertyName("chroma_location")]
    public AVChromaLocation ChromaLocation { get; set; }

    [JsonPropertyName("field_order")]
    public AVFieldOrder FieldOrder { get; set; }

    [JsonPropertyName("refs")]
    public int Refs { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Video: {Width}x{Height}, {CodecId}";
    }
}

[DebuggerDisplay("Audio: {Id}, {CodecId}")]
public class AudioStream : Stream
{
    [JsonPropertyName("sample_fmt")]
    public AVSampleFormat SampleFormat { get; set; }

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; }

    [JsonPropertyName("channels")]
    public int Channels { get; set; }

    [Obsolete("use ChannelLayoutName")]
    [JsonPropertyName("channel_layout")]
    public ulong ChannelLayout { get; set; }

    [JsonPropertyName("channel_layout_order")]
    public AVChannelOrder ChannelLayoutOrder { get; set; }

    [JsonPropertyName("channel_layout_name")]
    public string? ChannelLayoutName { get; set; }

    [JsonPropertyName("bits_per_sample")]
    public int BitsPerSample { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Audio: {Id}, {CodecId}";
    }
}

[DebuggerDisplay("Subtitle: {Id}, {CodecId}")]
public class SubtitleStream : Stream
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Subtitle: {Id}, {CodecId}";
    }
}
