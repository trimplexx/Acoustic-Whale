using System;
using NAudio.Wave;

namespace SM_Audio_Player.SoundTouch;

public class DistortionSampleProvider : ISampleProvider
{
    private readonly ISampleProvider source;
    public float Gain { get; set; } = 1f;
    public float Mix { get; set; } = 1f;

    public DistortionSampleProvider(ISampleProvider source)
    {
        this.source = source;
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = source.Read(buffer, offset, count);
        for (int n = 0; n < samplesRead; n++)
        {
            float cleanSample = buffer[offset + n];
            float distortedSample = cleanSample * Gain;
            distortedSample = (Math.Abs(distortedSample) < 0.01f) ? 2 * distortedSample : (Math.Sign(distortedSample) * (1 - (float)Math.Exp(-Math.Abs(distortedSample))));
            buffer[offset + n] = Mix * distortedSample + (1 - Mix) * cleanSample;
        }
        return samplesRead;
    }
}