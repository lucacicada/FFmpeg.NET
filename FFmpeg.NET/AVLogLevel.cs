namespace AVIO;

/// <summary>
///     Logging Constants.
/// </summary>
[Flags]
public enum AVLogLevel
{
    /// <summary>Print no output.</summary>
    QUIET = ffmpeg.AV_LOG_QUIET,

    /// <summary>Stuff which is only useful for libav* developers.</summary>
    DEBUG = ffmpeg.AV_LOG_DEBUG,

    /// <summary>Something went wrong and cannot losslessly be recovered.</summary>
    ERROR = ffmpeg.AV_LOG_ERROR,
    
    /// <summary>Something went wrong and recovery is not possible.</summary>
    FATAL = ffmpeg.AV_LOG_FATAL,
    
    /// <summary>Standard information.</summary>
    INFO = ffmpeg.AV_LOG_INFO,
    
    /// <summary>Something went really wrong and we will crash now.</summary>
    PANIC = ffmpeg.AV_LOG_PANIC,
    
    /// <summary>Skip repeated messages.</summary>
    SKIP_REPEATED = ffmpeg.AV_LOG_SKIP_REPEATED,
    
    /// <summary>Extremely verbose debugging, useful for libav* development.</summary>
    TRACE = ffmpeg.AV_LOG_TRACE,
    
    /// <summary>Detailed information.</summary>
    VERBOSE = ffmpeg.AV_LOG_VERBOSE,
    
    /// <summary>Something somehow does not look correct.</summary>
    WARNING = ffmpeg.AV_LOG_WARNING,
}
