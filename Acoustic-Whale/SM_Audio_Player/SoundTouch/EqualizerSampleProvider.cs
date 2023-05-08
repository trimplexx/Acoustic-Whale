using System.Linq;
using NAudio.Dsp;
using NAudio.Wave;

namespace SM_Audio_Player.SoundTouch;

///<summary>
/// Klasa Implementuje interfejs ISampleProvider, który reprezentuje dostawcę próbek audio.
/// EqualizerSampleProvider zapewnia funkcjonalność zmiany ustawień pasm equalizatora na próbkach audio,
/// które są pobierane przez obiekt ISampleProvider, który został przekazany do konstruktora.
///</summary>
public class EqualizerSampleProvider : ISampleProvider
{
    /// <summary>
    /// Tablica pasm equalizatora.
    /// </summary>
    private readonly EqualizerBand[] _bands;

    /// <summary>
    /// Tablica obiektów filtrów BiQuadFilter.
    /// </summary>
    private readonly BiQuadFilter[] _filters;

    /// <summary>
    /// Obiekt ISampleProvider, który dostarcza próbki audio.
    /// </summary>
    public readonly ISampleProvider SourceProvider;

    ///<summary>
    /// Tworzy nowy obiekt klasy EqualizerSampleProvider na podstawie obiektu ISampleProvider, który dostarcza próbki audio.
    /// Inicjalizuje tablicę pasm equalizatora oraz tablicę obiektów filtrów BiQuadFilter.
    ///</summary>
    ///<param name="sourceProvider">Obiekt ISampleProvider, który dostarcza próbki audio.</param>
    public EqualizerSampleProvider(ISampleProvider sourceProvider)
    {
        SourceProvider = sourceProvider;
        // Ustawienie pasm equalizatora.
        _bands = new[]
        {
            new() { Bandwidth = 0.8f, Frequency = 100, Gain = 1 },
            new EqualizerBand { Bandwidth = 0.8f, Frequency = 200, Gain = 1 },
            new EqualizerBand { Bandwidth = 0.8f, Frequency = 400, Gain = 1 },
            new EqualizerBand { Bandwidth = 0.8f, Frequency = 800, Gain = 1 },
            new EqualizerBand { Bandwidth = 0.8f, Frequency = 1200, Gain = 1 },
            new EqualizerBand { Bandwidth = 0.8f, Frequency = 2400, Gain = 1 },
            new EqualizerBand { Bandwidth = 0.8f, Frequency = 4000, Gain = 1 },
            new EqualizerBand { Bandwidth = 0.8f, Frequency = 6000, Gain = 1 }
        };
        // Tworzenie instancji obiektów BiQuadFilter na podstawie ustawień pasm equalizatora.
        _filters = _bands.Select(b =>
            BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, b.Frequency, b.Bandwidth, b.Gain)).ToArray();
    }

    ///<summary>
    /// Zwraca format próbki audio, który jest dostarczany przez obiekt ISampleProvider przekazany w konstruktorze.
    ///</summary>
    public WaveFormat WaveFormat => SourceProvider.WaveFormat;

    ///<summary>
    /// Odczytuje próbki audio z obiektu ISampleProvider, przekształcając je przy użyciu filtrów BiQuadFilter
    /// zdefiniowanych w tablicy _filters i zapisując wynik z powrotem do bufora.
    ///</summary>
    ///<param name="buffer">Bufor, do którego zostaną zapisane próbki audio.</param>
    /// <param name="offset">Przesunięcie początkowe w buforze</param>
    /// <param name="count">Liczba próbek do odczytania z bufora</param>
    /// <returns>Liczba odczytanych próbek</returns>
    public int Read(float[] buffer, int offset, int count)
    {
        // Odczytanie próbek z dostawcy źródłowego.
        var samplesRead = SourceProvider.Read(buffer, offset, count);
        // Przetworzenie próbek przez filtry pasmowe.
        for (var n = 0; n < samplesRead; n++)
        {
            var sample = buffer[offset + n];
            foreach (var filter in _filters) sample = filter.Transform(sample);
            buffer[offset + n] = sample;
        }

        return samplesRead;
    }

    /// <summary>
    /// Aktualizuje ustawienia pasm equalizatora na podstawie wartości przekazanych przez suwaki.
    /// </summary>
    /// <param name="sld1Value">Wartość pierwszego suwaka</param>
    /// <param name="sld2Value">Wartość drugiego suwaka</param>
    /// <param name="sld3Value">Wartość trzeciego suwaka</param>
    /// <param name="sld4Value">Wartość czwartego suwaka</param>
    /// <param name="sld5Value">Wartość piątego suwaka</param>
    /// <param name="sld6Value">Wartość szóstego suwaka</param>
    /// <param name="sld7Value">Wartość siódmego suwaka</param>
    /// <param name="sld8Value">Wartość ósmego suwaka</param>
    public void UpdateEqualizer(double sld1Value, double sld2Value, double sld3Value, double sld4Value,
        double sld5Value, double sld6Value, double sld7Value, double sld8Value)
    {
        // Ustawienie wartości dla każdego z filtrów pasmowych zgodnie z wartością przekazaną przez odpowiedni suwak.
        _filters[0].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[0].Frequency, _bands[0].Bandwidth,
            (float)sld1Value);
        _filters[1].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[1].Frequency, _bands[1].Bandwidth,
            (float)sld2Value);
        _filters[2].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[2].Frequency, _bands[2].Bandwidth,
            (float)sld3Value);
        _filters[3].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[3].Frequency, _bands[3].Bandwidth,
            (float)sld4Value);
        _filters[4].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[4].Frequency, _bands[4].Bandwidth,
            (float)sld5Value);
        _filters[5].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[5].Frequency, _bands[5].Bandwidth,
            (float)sld6Value);
        _filters[6].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[6].Frequency, _bands[6].Bandwidth,
            (float)sld7Value);
        _filters[7].SetPeakingEq(SourceProvider.WaveFormat.SampleRate, _bands[7].Frequency, _bands[7].Bandwidth,
            (float)sld8Value);
    }
}