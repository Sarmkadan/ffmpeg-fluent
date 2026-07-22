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
        /// <param name="codec">The audio codec name (e.g., "aac", "libmp3lame").</param>
        /// <returns>The same <see cref="AudioOptions"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="codec"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="codec"/> is empty or whitespace.</exception>
        public AudioOptions Codec(string codec)
        {
            ArgumentNullException.ThrowIfNull(codec);
            if (string.IsNullOrWhiteSpace(codec))
            {
                throw new ArgumentException("Codec name cannot be empty or whitespace.", nameof(codec));
            }

            _codec = codec;
            return this;
        }

        /// <summary>
        /// Sets the audio bitrate.
        /// </summary>
        /// <param name="bitrate">The bitrate string (e.g., "192k", "2M").</param>
        /// <returns>The same <see cref="AudioOptions"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitrate"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bitrate"/> is empty or whitespace.</exception>
        public AudioOptions Bitrate(string bitrate)
        {
            ArgumentNullException.ThrowIfNull(bitrate);
            if (string.IsNullOrWhiteSpace(bitrate))
            {
                throw new ArgumentException("Bitrate cannot be empty or whitespace.", nameof(bitrate));
            }

            _bitrate = bitrate;
            return this;
        }

        /// <summary>
        /// Sets the audio sample rate in Hz.
        /// </summary>
        /// <param name="hz">The sample rate in Hz (e.g., 44100, 48000).</param>
        /// <returns>The same <see cref="AudioOptions"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="hz"/> is not positive.</exception>
        public AudioOptions SampleRate(int hz)
        {
            if (hz <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hz), "Sample rate must be a positive value.");
            }

            _sampleRate = hz;
            return this;
        }

        /// <summary>
        /// Sets the number of audio channels.
        /// </summary>
        /// <param name="n">The number of audio channels (e.g., 1 for mono, 2 for stereo).</param>
        /// <returns>The same <see cref="AudioOptions"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is not positive.</exception>
        public AudioOptions Channels(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "Channel count must be a positive value.");
            }

            _channels = n;
            return this;
        }

        /// <summary>
        /// Sets the audio volume multiplier (ffmpeg filter).
        /// </summary>
        /// <param name="multiplier">The volume multiplier (e.g., 1.0 for original volume, 0.5 for half volume).</param>
        /// <returns>The same <see cref="AudioOptions"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="multiplier"/> is negative.</exception>
        public AudioOptions Volume(double multiplier)
        {
            if (multiplier < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier), "Volume multiplier cannot be negative.");
            }

            _volume = multiplier;
            return this;
        }

        /// <summary>
        /// Disables audio output.
        /// </summary>
        /// <returns>The same <see cref="AudioOptions"/> instance for fluent chaining.</returns>
        public AudioOptions NoAudio()
        {
            _noAudio = true;
            return this;
        }

        /// <summary>
        /// Builds the ffmpeg command line arguments for the configured audio options.
        /// </summary>
        /// <returns>An enumerable of command-line arguments.</returns>
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
                yield return _sampleRate.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (_channels.HasValue)
            {
                yield return "-ac";
                yield return _channels.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (_volume.HasValue)
            {
                yield return "-af";
                yield return $"volume={_volume.Value.ToString(CultureInfo.InvariantCulture)}";
            }
        }
    }
}
