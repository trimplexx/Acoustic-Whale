using NAudio.Wave;

namespace SM_Audio_Player.Music;

public class SpeedControlSampleProvider : ISampleProvider
{
    private ISampleProvider sourceProvider;
    private float speed;

    public SpeedControlSampleProvider(ISampleProvider sourceProvider)
    {
        this.sourceProvider = sourceProvider;
        this.speed = 1.0f;
    }

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int sourceSamplesRequired = (int)(count * speed);
        float[] sourceBuffer = new float[sourceSamplesRequired];
        int sourceSamplesRead = sourceProvider.Read(sourceBuffer, 0, sourceSamplesRequired);
        int samplesRead = (int)(sourceSamplesRead / speed);

        // simple resampling - just repeat or skip samples
        for (int n = 0; n < samplesRead; n++)
        {
            int sourceIndex = (int)(n * speed);
            buffer[offset + n] = sourceBuffer[sourceIndex];
        }

        return samplesRead;
    }

    public WaveFormat WaveFormat
    {
        get { return sourceProvider.WaveFormat; }
    }
}