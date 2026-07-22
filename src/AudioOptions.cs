#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FFmpegFluent
{
    /// <summary>
    /// Fluent class for configuring audio output options.
    /// </summary>
    public sealed class AudioOptions
    {
        private string? _codec;
        private string? _bitrate;
        private int? _sampleRate;
        private int? _channels;
        private double? _volume;
        private bool _noAudio;

        /// <summary>
        /// Sets the audio codec.
        /// </summary>
        public AudioOptions Codec(string codec)
        {
            _codec = codec;
            return this;
        }

        /// <summary>
        /// Sets the audio bitrate.
        /// </summary>
        public AudioOptions Bitrate(string b)
        {
            _bitrate = b;
            return this;
        }

        /// <summary>
        /// Sets the audio sample rate in Hz.
        /// </summary>
        public AudioOptions SampleRate(int hz)
        {
            _sampleRate = hz;
            return this;
        }

        /// <summary>
        /// Sets the number of audio channels.
        /// </summary>
        public AudioOptions Channels(int n)
        {
            _channels = n;
            return this;
        }

        /// <summary>
        /// Sets the audio volume multiplier (ffmpeg filter).
        /// </summary>
        public AudioOptions Volume(double multiplier)
        {
            _volume = multiplier;
            return this;
        }

        /// <summary>
        /// Disables audio output.
        /// </summary>
        public AudioOptions NoAudio()
        {
            _noAudio = true;
            return this;
        }

        /// <summary>
        /// Builds the ffmpeg command line arguments for the configured audio options.
        /// </summary>
        public IEnumerable<string> BuildArgs()
        {
            if (_noAudio)
            {
                yield return "-an";
                yield break;
            }

            if (_codec is not null)
            {
                yield return "-c:a";
                yield return _codec;
            }

            if (_bitrate is not null)
            {
                yield return "-b:a";
                yield return _bitrate;
            }

            if (_sampleRate.HasValue)
            {
                yield return "-ar";
                yield return _sampleRate.Value.ToString();
            }

            if (_channels.HasValue)
            {
                yield return "-ac";
                yield return _channels.Value.ToString();
            }

            if (_volume.HasValue)
            {
                yield return "-af";
                yield return $"volume={_volume.Value.ToString(CultureInfo.InvariantCulture)}";
            }
        }
    }
}
