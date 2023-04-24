using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace SM_Audio_Player.Music;

public class ChorusEffect : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly Queue<float> buffer;
    private int delayInSamples;
    private float depth;

    public ChorusEffect(ISampleProvider source, int delayInMilliseconds, float depth)
    {
        this.source = source;
        this.depth = depth;
        SetDelay(delayInMilliseconds);
        buffer = new Queue<float>(delayInSamples);
        for (int i = 0; i < delayInSamples; i++)
            buffer.Enqueue(0);
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = source.Read(buffer, offset, count);
        for (int i = 0; i < samplesRead; i++)
        {
            float delayedSample = this.buffer.Dequeue();
            buffer[offset + i] += delayedSample * depth;
            this.buffer.Enqueue(buffer[offset + i]);
        }
        return samplesRead;
    }

    public void SetDelay(int delayInMilliseconds)
    {
        delayInSamples = (int)(delayInMilliseconds / 1000.0 * source.WaveFormat.SampleRate);
    }

    public void SetDepth(float depth)
    {
        this.depth = depth;
    }
}