using System.Collections.Generic;
using NAudio.Wave;

namespace SM_Audio_Player.SoundTouch;

/// <summary>
/// Klasa ChorusEffect dziedzicząca po interfejsie ISampleProvider służąca do nałożenia efektu 'Chorus' na obecnie grany
/// utwór.
/// </summary>
public class ChorusEffect : ISampleProvider
{
    private readonly ISampleProvider _source;
    private Queue<float> _buffer;
    private int _delayInSamples;
    private float _depth;

    /// <summary>
    /// Konstruktor klasy ChorusEffect, w którym przypisywane są poniższe parametry dla podanej ścieżki dźwiękowej.
    /// </summary>
    /// <param name="source">Podane źródło, z którego odtwarzany jest utwór.</param>
    /// <param name="delayInMilliseconds">Czas w milisekundach opóźnienia nałożonego na grany utwór</param>
    /// <param name="depth">Parametr głębi efektu</param>
    public ChorusEffect(ISampleProvider source, int delayInMilliseconds, float depth)
    {
        _source = source;
        _depth = depth;
        SetDelay(delayInMilliseconds);
        _buffer = new Queue<float>(_delayInSamples);
        for (var i = 0; i < _delayInSamples; i++)
            _buffer.Enqueue(0);
    }

    ///<summary>
    /// Zwraca format fali dźwiękowej źródła dźwięku.
    /// </summary>
    public WaveFormat WaveFormat => _source.WaveFormat;

    /// <summary>
    /// Metoda Read odczytuje próbki z podanego źródła i nakłada na nie efekt Chorus poprzez dodanie opóźnionej próbki
    /// pomnożonej przez parametr głębi.
    /// </summary>
    /// <param name="buffer">Bufor do którego zostaną zapisane próbki po nałożeniu efektu Chorus.</param>
    /// <param name="offset">Przesunięcie w buforze, od którego zaczyna się zapisywanie próbek.</param>
    /// <param name="count">Liczba próbek do odczytania.</param>
    /// <returns>Liczba odczytanych próbek.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = _source.Read(buffer, offset, count);
        for (var i = 0; i < samplesRead; i++)
        {
            var delayedSample = _buffer.Dequeue();
            buffer[offset + i] += delayedSample * _depth;
            _buffer.Enqueue(buffer[offset + i]);
        }

        return samplesRead;
    }

    /// <summary>
    /// Metoda służąca zmianie parametru opóźnienia dla efektu Chorus.
    /// </summary>
    /// <param name="delayInMilliseconds">Czas w milisekundach opóźnienia nałożonego na grany utwór</param>
    public void SetDelay(int delayInMilliseconds)
    {
        _delayInSamples = (int)(delayInMilliseconds / 1000.0 * _source.WaveFormat.SampleRate);
        _buffer = new Queue<float>(_delayInSamples);
        for (var i = 0; i < _delayInSamples; i++)
            _buffer.Enqueue(0);
    }

    /// <summary>
    /// Metoda służąca zmianie parametru głębi dla efektu Chorus.
    /// </summary>
    /// <param name="depth">Parametr głębi efektu</param>
    public void SetDepth(float depth)
    {
        _depth = depth;
    }
}