using System;
using NAudio.Wave;

namespace SM_Audio_Player.SoundTouch;

public class DelayEffect : ISampleProvider
{
    private readonly ISampleProvider _source;
    private int _delayBufferSize;
    private float[]? _delayBuffer;
    private int _delayBufferIndex;
    private float _decay;

    public DelayEffect(ISampleProvider source, int delayMilliseconds, float decay)
    {
        this._source = source;
        SetDelay(delayMilliseconds);
        SetDecay(decay);
    }

    public void SetDelay(int delayMilliseconds)
    {
        if (_delayBuffer != null)
        {
            Array.Clear(_delayBuffer, 0, _delayBuffer.Length);
        }
        _delayBufferSize = (int)(delayMilliseconds / 1000.0 * _source.WaveFormat.SampleRate) * _source.WaveFormat.Channels;
        _delayBuffer = new float[_delayBufferSize];
        _delayBufferIndex = 0;
    }

    public void SetDecay(float decay)
    {
        this._decay = decay;
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        for (int n = 0; n < samplesRead; n++)
        {
            if (_delayBuffer != null)
            {
                buffer[offset + n] += _decay * _delayBuffer[_delayBufferIndex];
                _delayBuffer[_delayBufferIndex] = buffer[offset + n];
            }

            _delayBufferIndex++;
            if (_delayBufferIndex >= _delayBufferSize) _delayBufferIndex = 0;
        }
        return samplesRead;
    }
}