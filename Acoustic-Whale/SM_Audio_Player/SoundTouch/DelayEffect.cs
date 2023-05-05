using System;
using NAudio.Wave;

namespace SM_Audio_Player.SoundTouch;

///<summary>
/// Klasa DelayEffect reprezentuje efekt dźwiękowy opóźnienia sygnału audio. Implementuje interfejs ISampleProvider,
/// co pozwala na przetwarzanie próbek audio.
///</summary>
public class DelayEffect : ISampleProvider
{
    private readonly ISampleProvider _source;
    private int _delayBufferSize;
    private float[]? _delayBuffer;
    private int _delayBufferIndex;
    private float _decay;


    ///<summary>
    /// Konstruktor klasy DelayEffect
    /// Tworzy nową instancję klasy DelayEffect na podstawie dostarczonego źródła dźwięku, czasu opóźnienia w milisekundach i stopnia zaniku dźwięku.
    /// </summary>
    /// <param name="source">Źródło dźwięku, które ma być opóźnione</param>
    /// <param name="delayMilliseconds">Czas opóźnienia w milisekundach</param>
    /// <param name="decay">Stopień zaniku dźwięku (współczynnik od 0 do 1)</param>
    public DelayEffect(ISampleProvider source, int delayMilliseconds, float decay)
    {
        _source = source;
        SetDelay(delayMilliseconds);
        SetDecay(decay);
    }

    ///<summary>
    /// Ustawia czas opóźnienia w milisekundach.
    /// <code>
    /// Jeśli bufor opóźnienia nie istnieje, tworzy go i ustawia jego rozmiar na podstawie czasu opóźnienia i formatu źródłowego dźwięku.
    /// Jeśli bufor opóźnienia istnieje, czyści jego zawartość i ustawia jego rozmiar na nowo.
    /// </code>
    /// </summary>
    /// <param name="delayMilliseconds">Czas opóźnienia w milisekundach</param>
    public void SetDelay(int delayMilliseconds)
    {
        if (_delayBuffer != null) Array.Clear(_delayBuffer, 0, _delayBuffer.Length);
        _delayBufferSize = (int)(delayMilliseconds / 1000.0 * _source.WaveFormat.SampleRate) *
                           _source.WaveFormat.Channels;
        _delayBuffer = new float[_delayBufferSize];
        _delayBufferIndex = 0;
    }

    ///<summary>
    /// Ustawia stopień zaniku dźwięku.
    /// </summary>
    /// <param name="decay">Stopień zaniku dźwięku (współczynnik od 0 do 1)</param>
    public void SetDecay(float decay)
    {
        _decay = decay;
    }

    ///<summary>
    /// Zwraca format fali dźwiękowej źródła dźwięku.
    /// </summary>
    public WaveFormat WaveFormat => _source.WaveFormat;

    ///<summary>
    /// Odczytuje próbki z źródła dźwięku i dodaje je do bufora opóźnienia.
    /// Następnie, dla każdej odczytanej próbki, dodaje do niej odpowiednią wartość z bufora opóźnienia z uwzględnieniem stopnia zaniku dźwięku.
    /// </summary>
    /// <param name="buffer">Bufor, do którego zostaną zapisane próbki dźwiękowe</param>
    /// <param name="offset">Indeks początkowy w buforze, od którego należy zapisywać próbki</param>
    /// <param name="count">Liczba próbek, która ma być odczytana z źródła dźwięku</param>
    /// <returns>Liczba odczytanych próbek</returns>
    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = _source.Read(buffer, offset, count);
        for (var n = 0; n < samplesRead; n++)
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