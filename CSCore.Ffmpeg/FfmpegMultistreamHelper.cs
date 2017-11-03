using System;
using System.Collections.Generic;
using System.IO;

namespace CSCore.Ffmpeg
{
    public static class FfmpegMultistreamHelper
    {
        public unsafe static int GetNumberOfStreams(string url)
        {
            var _formatContext = FfmpegCalls.AvformatAllocContext();
            FfmpegCalls.AvformatOpenInput(&_formatContext, url);

            return GetNumberOfStreams(_formatContext);
        }

        public unsafe static int GetNumberOfStreams(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var _ffmpegStream = new FfmpegStream(stream);

            var _formatContext = FfmpegCalls.AvformatAllocContext();
            FfmpegCalls.AvformatOpenInput(&_formatContext, _ffmpegStream.AvioContext);

            return GetNumberOfStreams(_formatContext);
        }

        private unsafe static int GetNumberOfStreams(Interops.AVFormatContext* _formatContext)
        {
            var numOfStreams = _formatContext->nb_streams;

            return checked((int)numOfStreams);
        }

        public static IEnumerable<IWaveSource> GetMultipleStream(string url)
        {
            var num = GetNumberOfStreams(url);

            for (int i = 0; i < num; i++)
            {
                FfmpegIndexedStreamDecoder decoder = null;
                try
                {
                    // Interops.ffmpeg.AV_CH_STEREO_RIGHT
                    decoder = new FfmpegIndexedStreamDecoder(url, i);
                }
                catch (Exception)
                {
                    // swallow exception, the stream at this position might not be an audio stream
                }

                if (decoder != null)
                    yield return decoder;
            }
        }

        public static IEnumerable<IWaveSource> GetMultipleStream(Stream stream)
        {
            var num = GetNumberOfStreams(stream);

            for (int i = 1; i < num; i++)
            {
                yield return new FfmpegIndexedStreamDecoder(stream, i);
            }
        }
    }
}