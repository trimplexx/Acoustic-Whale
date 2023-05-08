using System;
using NAudio.Wave;

namespace SM_Audio_Player.SoundTouch;

///<summary>
/// Klasa implementująca interfejs ISampleProvider, która dodaje efekt zniekształcenia do sygnału audio.
/// Działa poprzez przekształcenie każdej próbki w sygnale.
/// </summary>
public class DistortionEffect : ISampleProvider
{
    private readonly ISampleProvider _source;

    /// <summary>
    /// Poziom podniesienia głośności utworu.
    /// </summary>
    public float Gain { get; set; } = 1f;

    /// <summary>
    /// Mieszanie sygnału oryginalnego i zniekształconego.
    /// </summary>
    public float Mix { get; set; } = 1f;

    ///<summary>
    /// Konstruktor klasy DistortionSampleProvider
    /// Tworzy nową instancję klasy DelayEffect na podstawie dostarczonego źródła dźwięku, czasu opóźnienia w milisekundach i stopnia zaniku dźwięku.
    /// </summary>
    /// <param name="source">Źródło dźwięku, które ma być zniekształcone</param>
    public DistortionEffect(ISampleProvider source)
    {
        _source = source;
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    ///<summary>
    /// Odczytuje określoną liczbę próbek z bufora wejściowego, stosuje efekt zniekształcenia i miesza go z czystym sygnałem wejściowym. Wynik jest zapisywany do bufora wyjściowego.
    /// </summary>
    ///<param name="buffer">Bufor, do którego mają zostać zapisane wyjściowe próbki.</param>
    ///<param name="offset">Indeks zerowy w buforze, od którego rozpocząć zapisywanie wyjściowych próbek.</param>
    ///<param name="count">Liczba odczytywanych próbek.</param>
    ///<returns>Liczba rzeczywiście odczytanych próbek.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = _source.Read(buffer, offset, count);
        for (var n = 0; n < samplesRead; n++)
        {
            var cleanSample = buffer[offset + n];
            var distortedSample = cleanSample * Gain;
            distortedSample = Math.Abs(distortedSample) < 0.01f
                ? 2 * distortedSample
                : Math.Sign(distortedSample) * (1 - (float)Math.Exp(-Math.Abs(distortedSample)));
            buffer[offset + n] = Mix * distortedSample + (1 - Mix) * cleanSample;
        }

        return samplesRead;
    }
}