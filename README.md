# FFmpeg.NET

FFmpeg wrapper around FFmpeg.AutoGen for .NET.

> `AVIO` is the root namespace of this library.

## Probing

Use `AVIO.FFProbe` for probing files.

The return data is very similar to the output from `ffprobe`.

```c#
using AVIO;

ProbeResult probeResult = FFProbe.File("...");

probeResult.FormatName;
probeResult.FormatLongName;
probeResult.Extensions;
probeResult.MimeType;
probeResult.StartTime;
probeResult.Duration;
probeResult.Size;
probeResult.BitRate;
probeResult.ProbeScore;
probeResult.Tag;

// loop the video streams
foreach (VideoStream videoStream in probe.Videos)
{
    videoStream.Width;
    videoStream.Height;
}
```

### Probe visitor pattern

You can also manually parse the `AVFormatContext`, the library will take care of freeing the resources, you need to manually free the visitor by yourself.

```c#
using AVIO;

CustomData data = FFProbe.File("...", new Visitor());

class Visitor : IAVFormatContextVisitor<CustomData>
{
    public unsafe CustomData Visit(AVFormatContext* formatContext)
    {
        // ... your code can iterate trough the context and return a CustomData instance.
    }
}
```

## Tests

Fixture video for this project are taken from [Test-Videos](https://test-videos.co.uk/bigbuckbunny/mp4-h264), not included in this project.
