using System.Linq;
using NAudio.Dsp;
using NAudio.Wave;

namespace SM_Audio_Player.Music;

public class EqualizerSampleProvider : ISampleProvider
{
    private readonly EqualizerBand[] _bands;
    private readonly BiQuadFilter[] _filters;
    public readonly ISampleProvider SourceProvider;

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

    public WaveFormat WaveFormat => SourceProvider.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = SourceProvider.Read(buffer, offset, count);
        for (var n = 0; n < samplesRead; n++)
        {
            var sample = buffer[offset + n];
            foreach (var filter in _filters) sample = filter.Transform(sample);
            buffer[offset + n] = sample;
        }

        return samplesRead;
    }

    // Metoda aktualizująca wartości pasm ekwilizatora na podstawie wartości suwaków.
    public void UpdateEqualizer(double sld1Value, double sld2Value, double sld3Value, double sld4Value,
        double sld5Value, double sld6Value, double sld7Value, double sld8Value)
    {
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