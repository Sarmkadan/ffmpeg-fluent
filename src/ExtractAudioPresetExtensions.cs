namespace FFmpegFluent
{
    /// <summary>
    /// Provides extension methods for configuring audio extraction presets.
    /// </summary>
    public static class ExtractAudioPresetExtensions
    {
        /// <summary>
        /// Sets the output format to MP3 with a default bitrate of 192 kbps.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <returns>A new preset instance with MP3 codec and 192 kbps bitrate configured.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        public static ExtractAudioPreset AsMp3(this ExtractAudioPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);
            return preset.WithCodec("libmp3lame").WithBitrate(192);
        }

        /// <summary>
        /// Sets the output format to AAC with a default bitrate of 128 kbps.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <returns>A new preset instance with AAC codec and 128 kbps bitrate configured.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        public static ExtractAudioPreset AsAac(this ExtractAudioPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);
            return preset.WithCodec("aac").WithBitrate(128);
        }

        /// <summary>
        /// Sets the output format to FLAC for lossless audio extraction.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <returns>A new preset instance with FLAC codec configured for lossless output.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        public static ExtractAudioPreset AsFlac(this ExtractAudioPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);
            return preset.WithCodec("flac");
        }

        /// <summary>
        /// Sets the output format to WAV for uncompressed audio extraction.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <returns>A new preset instance with PCM_S16LE codec configured for WAV output.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        public static ExtractAudioPreset AsWav(this ExtractAudioPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);
            return preset.WithCodec("pcm_s16le");
        }

        /// <summary>
        /// Sets a custom bitrate for the audio output.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="bitrateKbps">The target bitrate in kilobits per second.</param>
        /// <returns>The preset instance with the specified bitrate configured.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitrateKbps"/> is not positive.</exception>
        public static ExtractAudioPreset WithBitrate(this ExtractAudioPreset preset, int bitrateKbps)
        {
            ArgumentNullException.ThrowIfNull(preset);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitrateKbps);
            return preset.WithBitrate(bitrateKbps);
        }

        /// <summary>
        /// Extracts audio from the first audio stream without re-encoding (stream copy).
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <returns>The preset instance configured to copy the audio stream.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        public static ExtractAudioPreset CopyAudioStream(this ExtractAudioPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);
            return preset.CopyStream();
        }

        /// <summary>
        /// Extracts audio from a specific stream by index.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="streamIndex">The zero-based index of the audio stream to extract.</param>
        /// <returns>The preset instance configured to extract the specified stream.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="streamIndex"/> is negative.</exception>
        public static ExtractAudioPreset FromStream(this ExtractAudioPreset preset, int streamIndex)
        {
            ArgumentNullException.ThrowIfNull(preset);
            ArgumentOutOfRangeException.ThrowIfNegative(streamIndex);
            return preset.StreamIndex(streamIndex);
        }
    }
}