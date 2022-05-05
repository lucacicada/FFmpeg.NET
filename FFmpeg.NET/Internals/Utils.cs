namespace AVIO;

internal static class Utils
{
    public static int ThrowExceptionIfError(this int error)
    {
        if (error < 0) throw new AVIOErrorException(error);
        return error;
    }

    public static int ThrowExceptionIfError(this int error, string message)
    {
        if (error < 0) throw new AVIOErrorException(error, message);
        return error;
    }

    public static string? AVGet(this IReadOnlyDictionary<string, string>? dictionary, string key)
    {
        if (dictionary != null)
        {
            foreach (KeyValuePair<string, string> tag in dictionary)
            {
                if (string.Compare(tag.Key, key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return tag.Value;
                }
            }
        }

        return null;
    }

    public static unsafe AVCodecContext* GetDecCtx(AVStream* stream)
    {
        if (stream->codecpar->codec_id == AVCodecID.AV_CODEC_ID_PROBE)
        {
            // WARN "Failed to probe codec for input stream %d\n", stream->index
            return null;
        }

        var codec = ffmpeg.avcodec_find_decoder(stream->codecpar->codec_id);
        if (codec == null)
        {
            // WARN "Unsupported codec with id %d for input stream %d\n", stream->codecpar->codec_id, stream->index)
            return null;
        }

        AVCodecContext* dec_ctx = ffmpeg.avcodec_alloc_context3(codec);
        if (codec == null)
        {
            throw new AVIOAllocationException(nameof(ffmpeg.avcodec_alloc_context3), message: null); // Could not allocate context3.
        }

        _ = ffmpeg.avcodec_parameters_to_context(dec_ctx, stream->codecpar).ThrowExceptionIfError();
        dec_ctx->pkt_timebase = stream->time_base;

        ffmpeg.avcodec_open2(dec_ctx, codec, null).ThrowExceptionIfError($"Could not open codec for input stream {stream->index}.");

        // TODO: set options for input stream
        // if ((t = av_dict_get(opts, "", NULL, AV_DICT_IGNORE_SUFFIX)))
        // {
        //     av_log(NULL, AV_LOG_ERROR, "Option %s for input stream %d not found\n",
        //            t->key, stream->index);
        //     return AVERROR_OPTION_NOT_FOUND;
        // }

        return dec_ctx;
    }
}
