using NAudio.Wave;

namespace SM_Audio_Player.Music;

public class DelayEffect : ISampleProvider
{
    private readonly ISampleProvider source;
    private int delayBufferSize;
    private float[] delayBuffer;
    private int delayBufferIndex;
    private float decay;

    public DelayEffect(ISampleProvider source, int delayMilliseconds, float decay)
    {
        this.source = source;
        SetDelay(delayMilliseconds);
        SetDecay(decay);
    }

    public void SetDelay(int delayMilliseconds)
    {
        delayBufferSize = (int)(delayMilliseconds / 1000.0 * source.WaveFormat.SampleRate) * source.WaveFormat.Channels;
        delayBuffer = new float[delayBufferSize];
    }

    public void SetDecay(float decay)
    {
        this.decay = decay;
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = source.Read(buffer, offset, count);
        for (int n = 0; n < samplesRead; n++)
        {
            buffer[offset + n] += decay * delayBuffer[delayBufferIndex];
            delayBuffer[delayBufferIndex] = buffer[offset + n];
            delayBufferIndex++;
            if (delayBufferIndex >= delayBufferSize) delayBufferIndex = 0;
        }
        return samplesRead;
    }
}