using NAudio.Wave;

namespace SM_Audio_Player.Music;

public class SpeedControlSampleProvider : ISampleProvider
{
    private readonly ISampleProvider sourceProvider;

    public SpeedControlSampleProvider(ISampleProvider sourceProvider)
    {
        this.sourceProvider = sourceProvider;
        Speed = 1.0f;
    }

    public float Speed { get; set; }

    public int Read(float[] buffer, int offset, int count)
    {
        var sourceSamplesRequired = (int)(count * Speed);
        var sourceBuffer = new float[sourceSamplesRequired];
        var sourceSamplesRead = sourceProvider.Read(sourceBuffer, 0, sourceSamplesRequired);
        var samplesRead = (int)(sourceSamplesRead / Speed);

        // simple resampling - just repeat or skip samples
        for (var n = 0; n < samplesRead; n++)
        {
            var sourceIndex = (int)(n * Speed);
            buffer[offset + n] = sourceBuffer[sourceIndex];
        }

        return samplesRead;
    }

    public WaveFormat WaveFormat => sourceProvider.WaveFormat;
}