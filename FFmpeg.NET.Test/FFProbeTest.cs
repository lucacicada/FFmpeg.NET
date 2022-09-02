using AVIO;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace FFmpeg.NET.Test
{
    public class Tests
    {
        string FixturesDirectory = "fixtures";

        string Fixture(string path)
        {
            return Path.Combine(FixturesDirectory, path);
        }

        [SetUp]
        public void Setup()
        {
            FixturesDirectory = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../fixtures"));
        }

        [Test]
        public void TestFile()
        {
            var probe = FFProbe.File(Fixture("Big_Buck_Bunny_1080_10s_1MB.mp4"));

            Assert.AreEqual("mov,mp4,m4a,3gp,3g2,mj2", probe.FormatName);
            Assert.AreEqual("QuickTime / MOV", probe.FormatLongName);
            Assert.AreEqual("mov,mp4,m4a,3gp,3g2,mj2,psp,m4b,ism,ismv,isma,f4v,avif", probe.Extensions);
            Assert.AreEqual(null, probe.MimeType);
            Assert.AreEqual(TimeSpan.Zero, probe.StartTime);
            Assert.AreEqual(TimeSpan.FromSeconds(10), probe.Duration);
            Assert.AreEqual(1046987, probe.Size);
            Assert.AreEqual(837589, probe.BitRate);
            Assert.AreEqual(100, probe.ProbeScore);

            Assert.IsEmpty(probe.Programs);
            Assert.IsEmpty(probe.Chapters);

            Assert.IsEmpty(probe.Audios);
            Assert.IsEmpty(probe.Subtitles);

            Assert.AreEqual(1, probe.Streams.Length);
            Assert.AreEqual(1, probe.Videos.Count());
            var stream = probe.Streams[0];

            Assert.AreEqual(typeof(VideoStream), stream.GetType());
            Assert.AreEqual(FFmpeg.AutoGen.AVMediaType.AVMEDIA_TYPE_VIDEO, stream.CodecType);
            Assert.AreEqual(FFmpeg.AutoGen.AVCodecID.AV_CODEC_ID_H264, stream.CodecId);
            Assert.AreEqual("h264", stream.CodecName);
            Assert.AreEqual(1920, ((VideoStream)stream).Width);
            Assert.AreEqual(1080, ((VideoStream)stream).Height);
        }

        [Test]
        public void TestStream()
        {
            using (var fileStream = File.OpenRead(Fixture("Big_Buck_Bunny_1080_10s_1MB.mp4")))
            {
                var probe = FFProbe.Stream(fileStream);

                Assert.AreEqual("mov,mp4,m4a,3gp,3g2,mj2", probe.FormatName);
                Assert.AreEqual("QuickTime / MOV", probe.FormatLongName);
                Assert.AreEqual("mov,mp4,m4a,3gp,3g2,mj2,psp,m4b,ism,ismv,isma,f4v,avif", probe.Extensions);
                Assert.AreEqual(null, probe.MimeType);
                Assert.AreEqual(TimeSpan.Zero, probe.StartTime);
                Assert.AreEqual(TimeSpan.FromSeconds(10), probe.Duration);
                Assert.AreEqual(1046987, probe.Size);
                Assert.AreEqual(837589, probe.BitRate);
                Assert.AreEqual(100, probe.ProbeScore);

                Assert.IsEmpty(probe.Programs);
                Assert.IsEmpty(probe.Chapters);

                Assert.AreEqual(1, probe.Streams.Length);
                var stream = probe.Streams[0];

                Assert.AreEqual(typeof(VideoStream), stream.GetType());
                Assert.AreEqual(FFmpeg.AutoGen.AVMediaType.AVMEDIA_TYPE_VIDEO, stream.CodecType);
                Assert.AreEqual(FFmpeg.AutoGen.AVCodecID.AV_CODEC_ID_H264, stream.CodecId);
                Assert.AreEqual("h264", stream.CodecName);
            }
        }

        [Test]
        public void TestAudio()
        {
            using (var fileStream = File.OpenRead(Fixture("file_example_MP3_700KB.mp3")))
            {
                var probe = FFProbe.Stream(fileStream);

                Assert.AreEqual("mp3", probe.FormatName);
                Assert.AreEqual("MP2/3 (MPEG audio layer 2/3)", probe.FormatLongName);
                Assert.AreEqual("mp2,mp3,m2a,mpa", probe.Extensions);
            }
        }
    }
}
