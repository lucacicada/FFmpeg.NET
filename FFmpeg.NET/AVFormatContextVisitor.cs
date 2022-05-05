namespace AVIO;

public interface IAVFormatContextVisitor<out TResult>
{
    unsafe TResult Visit(AVFormatContext* formatContext);
}

// the visitor pattern may not be a good idea...
internal sealed class AVFormatContextVisitorRes : IAVFormatContextVisitor<ProbeResult>
{
    public unsafe ProbeResult Visit(AVFormatContext* formatContext)
    {
        AVCodecContext*[] codecContexts = new AVCodecContext*[formatContext->nb_streams];

        // TODO: this should be optional if we can
        // read AVCodecContext 
        for (int i = 0; i < formatContext->nb_streams; i++)
        {
            AVStream* stream = formatContext->streams[i];
            if (stream is not null)
            {
                codecContexts[i] = Utils.GetDecCtx(stream);
            }
        }

        // visit the programs
        List<Program> programs = new();
        for (int i = 0; i < formatContext->nb_programs; i++)
        {
            AVProgram* program = formatContext->programs[i];
            if (program is not null)
            {
                programs.Add(VisitProgram(program));
            }
        }

        // visit the chapters
        List<Chapter> chapters = new();
        for (int i = 0; i < formatContext->nb_chapters; i++)
        {
            AVChapter* chapter = formatContext->chapters[i];
            if (chapter is not null)
            {
                chapters.Add(VisitChapter(chapter));
            }
        }

        // visit the streams
        List<Stream> streams = new();
        for (int i = 0; i < formatContext->nb_streams; i++)
        {
            AVStream* stream = formatContext->streams[i];
            if (stream is not null)
            {
                streams.Add(VisitStream(formatContext, codecContexts[i], stream));
            }
        }

        // avio_size will consume the context, so call it at the end
        // avio_size can return AVERROR if the pointer is null or the pb is not seekable, so it is safe to ignore
        var size = (formatContext->pb == null) ? -1 : ffmpeg.avio_size(formatContext->pb);

        return new ProbeResult()
        {
            Programs = programs.ToImmutableArray(),
            Streams = streams.ToImmutableArray(),
            Chapters = chapters.ToImmutableArray(),
            FormatName = Marshal.PtrToStringAnsi((IntPtr)formatContext->iformat->name)!,
            FormatLongName = Marshal.PtrToStringAnsi((IntPtr)formatContext->iformat->long_name)!,
            Extensions = Marshal.PtrToStringAnsi((IntPtr)formatContext->iformat->extensions) /* can be null*/,
            MimeType = Marshal.PtrToStringAnsi((IntPtr)formatContext->iformat->mime_type) /* can be null*/,
            StartTime = AV.ToTimeSpan(formatContext->start_time, ffmpeg.AV_TIME_BASE),
            Duration = AV.ToTimeSpan(formatContext->duration, ffmpeg.AV_TIME_BASE),
            BitRate = formatContext->bit_rate > 0 ? formatContext->bit_rate : 0,
            Size = size >= 0 ? size : 0 /* size 0 is not possible; avformat_open_input throw on 0 byte file*/,
            ProbeScore = formatContext->probe_score /* AVPROBE_SCORE_MAX = 100*/,
            Tags = AV.ToDictionary(formatContext->metadata).ToImmutableDictionary()
        };
    }

    public unsafe Stream VisitStream(AVFormatContext* fmt_ctx, AVCodecContext* dec_ctx, AVStream* stream)
    {
        // NOTE: here we extrapolate information from the format, the codec and the stream
        // those are 3 structure with some more nested structure
        // we want to give the user a wrapper that make sense to use not just a collection of properties
        // so for now, follow ffprobe.c, extract all variables with default values
        // and then try to sort and make sense of them

        var par = stream->codecpar;

        // -----------------------------------------------------------------------
        // Variable delcarations
        // -----------------------------------------------------------------------
        int index;

        AVCodecID codec_id;
        string? codec_name = null;
        string? codec_long_name = null;

        int profile;
        string? profile_name;

        AVMediaType codec_type;
        string codec_tag_string;
        uint codec_tag;

        // ffprobe.c second pass
        Dictionary<string, string> private_data = new();
        int id = 0;
        AVRational r_frame_rate;
        AVRational avg_frame_rate;
        TimeSpan? start_time;
        TimeSpan? duration;
        long bit_rate = 0;
        long max_bit_rate = 0;
        int bits_per_raw_sample = 0;
        long nb_frames = 0;
        byte[] extradata = Array.Empty<byte>();
        int disposition;
        IReadOnlyDictionary<string, string> tags;

        // -----------------------------------------------------------------------
        // Fist ffprobe.c assignments pass
        // -----------------------------------------------------------------------
        index = stream->index;
        codec_id = par->codec_id;

        AVCodecDescriptor* cd = ffmpeg.avcodec_descriptor_get(par->codec_id);
        if (cd != null)
        {
            codec_name = Marshal.PtrToStringAnsi((IntPtr)cd->name)!;
            codec_long_name = Marshal.PtrToStringAnsi((IntPtr)cd->long_name); // can be null
        }

        profile = par->profile;
        profile_name = ffmpeg.avcodec_profile_name(par->codec_id, par->profile); // can be null
        codec_type = par->codec_type;

        byte[] buffer = new byte[ffmpeg.AV_FOURCC_MAX_STRING_SIZE];
        fixed (byte* pBuffer = buffer)
        {
            codec_tag_string = Marshal.PtrToStringAnsi((IntPtr)ffmpeg.av_fourcc_make_string(pBuffer, par->codec_tag))!;
        }

        codec_tag = par->codec_tag;

        // -----------------------------------------------------------------------
        // Second ffprobe.c assignments pass
        // -----------------------------------------------------------------------
        if (dec_ctx != null && dec_ctx->codec != null && dec_ctx->codec->priv_class != null)
        {
            AVOption* opt = null;
            while ((opt = ffmpeg.av_opt_next((void*)dec_ctx->priv_data, opt)) != null)
            {
                if ((opt->flags & ffmpeg.AV_OPT_FLAG_EXPORT) == 0)
                    continue;

                var opt_name = Marshal.PtrToStringAnsi((IntPtr)opt->name)!;

                byte* str;
                if (ffmpeg.av_opt_get((void*)dec_ctx->priv_data, opt_name, 0, &str) >= 0)
                {
                    var opt_value = Marshal.PtrToStringAnsi((IntPtr)str)!;
                    ffmpeg.av_free(str);

                    private_data[opt_name] = opt_value;
                }
            }
        }

        if ((fmt_ctx->iformat->flags & ffmpeg.AVFMT_SHOW_IDS) != 0)
        {
            id = stream->id;
        }

        r_frame_rate = stream->r_frame_rate;
        avg_frame_rate = stream->avg_frame_rate;
        start_time = AV.ToTimeSpan(stream->start_time, stream->time_base);
        duration = AV.ToTimeSpan(stream->duration, stream->time_base);

        if (par->bit_rate > 0)
        {
            bit_rate = par->bit_rate;
        }

        if (dec_ctx != null && dec_ctx->rc_max_rate > 0)
        {
            max_bit_rate = dec_ctx->rc_max_rate;
        }

        if (dec_ctx != null && dec_ctx->bits_per_raw_sample > 0)
        {
            bits_per_raw_sample = dec_ctx->bits_per_raw_sample;
        }

        if (stream->nb_frames != 0)
        {
            nb_frames = stream->nb_frames;
        }

        if (par->extradata_size > 0)
        {
            extradata = new byte[par->extradata_size];
            Marshal.Copy((IntPtr)par->extradata, extradata, 0, extradata.Length);
        }

        disposition = stream->disposition;

        tags = AV.ToDictionary(stream->metadata);

        // -----------------------------------------------------------------------
        // Specialized codec type pass 
        // -----------------------------------------------------------------------
        switch (par->codec_type)
        {
            case AVMediaType.AVMEDIA_TYPE_VIDEO:
                {
                    int width;
                    int height;

                    uint codec_properties = 0;
                    int coded_width = 0;
                    int coded_height = 0;
                    int video_delay;
                    AVRational sample_aspect_ratio = default;
                    AVRational display_aspect_ratio = default;

                    AVPixelFormat pix_fmt;
                    int level;
                    AVColorRange color_range;
                    AVColorSpace color_space;
                    AVColorTransferCharacteristic color_transfer;
                    AVColorPrimaries color_primaries;
                    AVChromaLocation chroma_location;
                    AVFieldOrder field_order;
                    int refs = 0;

                    width = par->width;
                    height = par->height;

                    if (dec_ctx != null)
                    {
                        codec_properties = dec_ctx->properties;
                        coded_width = dec_ctx->coded_width;
                        coded_height = dec_ctx->coded_height;
                    }

                    video_delay = par->video_delay;

                    AVRational sar = ffmpeg.av_guess_sample_aspect_ratio(fmt_ctx, stream, null);
                    if (sar.num != 0)
                    {
                        sample_aspect_ratio = sar;

                        AVRational dar;
                        ffmpeg.av_reduce(&dar.num, &dar.den,
                            par->width * sar.num,
                            par->height * sar.den,
                            1024 * 1024);

                        display_aspect_ratio = dar;
                    }

                    pix_fmt = (AVPixelFormat)par->format;
                    level = par->level;
                    color_range = par->color_range;
                    color_space = par->color_space;
                    color_transfer = par->color_trc;
                    color_primaries = par->color_primaries;
                    chroma_location = par->chroma_location;
                    field_order = par->field_order;

                    if (dec_ctx != null)
                    {
                        refs = dec_ctx->refs;
                    }

                    return new VideoStream()
                    {
                        Id = id,
                        Index = index,

                        // codec ,
                        CodecId = codec_id,
                        CodecName = codec_name,
                        CodecLongName = codec_long_name,
                        CodecType = codec_type,
                        CodecTag = codec_tag,
                        CodecTagString = codec_tag_string,
                        CodecProperties = codec_properties,

                        Profile = profile,
                        ProfileName = profile_name,
                        FrameRate = r_frame_rate,
                        AverageFrameRate = avg_frame_rate,
                        StartTime = start_time,
                        Duration = duration,
                        BitRate = bit_rate,
                        MaxBitRate = max_bit_rate,
                        BitsPerRawSample = bits_per_raw_sample,
                        FrameCount = nb_frames,
                        Disposition = disposition,
                        Extradata = extradata,
                        PrivateData = private_data.ToImmutableDictionary(),
                        Tags = tags.ToImmutableDictionary(),

                        // video
                        Width = width,
                        Height = height,
                        CodedWidth = coded_width,
                        CodedHeight = coded_height,
                        VideoDelay = video_delay,
                        SampleAspectRatio = sample_aspect_ratio,
                        DisplayAspectRatio = display_aspect_ratio,
                        PixelFormat = pix_fmt,
                        Level = level,
                        ColorRange = color_range,
                        ColorSpace = color_space,
                        ColorTransfer = color_transfer,
                        ColorPrimaries = color_primaries,
                        ChromaLocation = chroma_location,
                        FieldOrder = field_order,
                        Refs = refs,
                    };
                }
            case AVMediaType.AVMEDIA_TYPE_AUDIO:
                {
                    AVSampleFormat sample_fmt;
                    string sample_fmt_name;
                    int sample_rate;
                    int channels;
                    ulong channel_layout = 0;
                    string? channel_layout_name = null;
                    int bits_per_sample;

                    sample_fmt = (AVSampleFormat)par->format;
                    sample_fmt_name = ffmpeg.av_get_sample_fmt_name((AVSampleFormat)par->format);
                    sample_rate = par->sample_rate;
                    channels = par->channels;

                    if (par->channel_layout != 0)
                    {
                        channel_layout = par->channel_layout;
                        channel_layout_name = AV.GetChannelLayoutName(par->channels, par->channel_layout);
                    }

                    bits_per_sample = ffmpeg.av_get_bits_per_sample(par->codec_id);

                    return new AudioStream()
                    {
                        Id = id,
                        Index = index,

                        // codec ,
                        CodecId = codec_id,
                        CodecName = codec_name,
                        CodecLongName = codec_long_name,
                        CodecType = codec_type,
                        CodecTag = codec_tag,
                        CodecTagString = codec_tag_string,
                        CodecProperties = 0,

                        Profile = profile,
                        ProfileName = profile_name,
                        FrameRate = r_frame_rate,
                        AverageFrameRate = avg_frame_rate,
                        StartTime = start_time,
                        Duration = duration,
                        BitRate = bit_rate,
                        MaxBitRate = max_bit_rate,
                        BitsPerRawSample = bits_per_raw_sample,
                        FrameCount = nb_frames,
                        Disposition = disposition,
                        Extradata = extradata,
                        PrivateData = private_data.ToImmutableDictionary(),
                        Tags = tags.ToImmutableDictionary(),

                        // audio
                        SampleFormat = sample_fmt,
                        SampleRate = sample_rate,
                        Channels = channels,
                        ChannelLayout = channel_layout,
                        ChannelLayoutName = channel_layout_name,
                        BitsPerSample = bits_per_sample,
                    };
                }
            case AVMediaType.AVMEDIA_TYPE_SUBTITLE:
                {
                    int width = par->width;
                    int height = par->height;

                    return new SubtitleStream()
                    {
                        Id = id,
                        Index = index,

                        // codec ,
                        CodecId = codec_id,
                        CodecName = codec_name,
                        CodecLongName = codec_long_name,
                        CodecType = codec_type,
                        CodecTag = codec_tag,
                        CodecTagString = codec_tag_string,
                        CodecProperties = 0,

                        Profile = profile,
                        ProfileName = profile_name,
                        FrameRate = r_frame_rate,
                        AverageFrameRate = avg_frame_rate,
                        StartTime = start_time,
                        Duration = duration,
                        BitRate = bit_rate,
                        MaxBitRate = max_bit_rate,
                        BitsPerRawSample = bits_per_raw_sample,
                        FrameCount = nb_frames,
                        Disposition = disposition,
                        Extradata = extradata,
                        PrivateData = private_data.ToImmutableDictionary(),
                        Tags = tags.ToImmutableDictionary(),

                        // subtitle
                        Width = width,
                        Height = height,
                    };
                }
        }

        return new Stream()
        {
            Id = id,
            Index = index,

            // codec ,
            CodecId = codec_id,
            CodecName = codec_name,
            CodecLongName = codec_long_name,
            CodecType = codec_type,
            CodecTag = codec_tag,
            CodecTagString = codec_tag_string,
            CodecProperties = 0,

            Profile = profile,
            ProfileName = profile_name,
            FrameRate = r_frame_rate,
            AverageFrameRate = avg_frame_rate,
            StartTime = start_time,
            Duration = duration,
            BitRate = bit_rate,
            MaxBitRate = max_bit_rate,
            BitsPerRawSample = bits_per_raw_sample,
            FrameCount = nb_frames,
            Disposition = disposition,
            Extradata = extradata,
            PrivateData = private_data.ToImmutableDictionary(),
            Tags = tags.ToImmutableDictionary(),
        };
    }

    //
    public unsafe Program VisitProgram(AVProgram* program)
    {
        for (int k = 0; k < program->nb_stream_indexes; k++)
        {
            // VisitProgramStream(formatContext, program->stream_index[k], true);
        }

        return new Program()
        {
            Id = program->id,
            Num = program->program_num,
            MappingTablePid = program->pmt_pid,
            ClockReferencePid = program->pcr_pid,
            Tags = AV.ToDictionary(program->metadata).ToImmutableDictionary()
        };
    }

    public unsafe Chapter VisitChapter(AVChapter* chapter)
    {
        return new Chapter()
        {
            Id = chapter->id,
            Start = AV.ToTimeSpan(chapter->start, chapter->time_base),
            End = AV.ToTimeSpan(chapter->end, chapter->time_base),
            Tags = AV.ToDictionary(chapter->metadata).ToImmutableDictionary()
        };
    }
}
