using System.Collections.Generic;
using NAudio.Wave;

namespace SM_Audio_Player.SoundTouch;

public class ChorusEffect : ISampleProvider
{
    private readonly ISampleProvider _source;
    private  Queue<float> _buffer;
    private int _delayInSamples;
    private float _depth;

    public ChorusEffect(ISampleProvider source, int delayInMilliseconds, float depth)
    {
        this._source = source;
        this._depth = depth;
        SetDelay(delayInMilliseconds);
        _buffer = new Queue<float>(_delayInSamples);
        for (int i = 0; i < _delayInSamples; i++)
            _buffer.Enqueue(0);
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        for (int i = 0; i < samplesRead; i++)
        {
            float delayedSample = this._buffer.Dequeue();
            buffer[offset + i] += delayedSample * _depth;
            this._buffer.Enqueue(buffer[offset + i]);
        }
        return samplesRead;
    }

    public void SetDelay(int delayInMilliseconds)
    {
        _delayInSamples = (int)(delayInMilliseconds / 1000.0 * _source.WaveFormat.SampleRate);
        _buffer = new Queue<float>(_delayInSamples);
        for (int i = 0; i < _delayInSamples; i++)
            _buffer.Enqueue(0);
    }

    public void SetDepth(float depth)
    {
        this._depth = depth;
    }
}