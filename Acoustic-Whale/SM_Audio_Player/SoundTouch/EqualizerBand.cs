namespace SM_Audio_Player.SoundTouch;

///<summary>
/// Klasa reprezentująca pojedyncze pasmo w equalizerze.
/// </summary>
public class EqualizerBand
{
    /// <summary>
    /// Częstotliwość, na której działa pas w Hz.
    /// </summary>
    public float Frequency { get; set; }

    /// <summary>
    /// Wzmocnienie pasma w dB.
    /// </summary>
    public float Gain { get; set; }

    /// <summary>
    /// Szerokość pasma w oktawach.
    /// </summary>
    public float Bandwidth { get; set; }
}