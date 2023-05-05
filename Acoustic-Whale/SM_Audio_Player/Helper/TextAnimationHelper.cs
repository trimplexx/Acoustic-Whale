using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SM_Audio_Player.Helper;

/// <summary>
/// Klasa TextAnimationHelper odpowiada za przewijanie się tekstu tworząc animację, gdy ten do wyświetlenia nie mieści
/// się w pożądanym rozmiarze na playerze aplikacji. 
/// </summary>
public abstract class TextAnimationHelper
{
    [Obsolete("Obsolete")]
    public static void TextAnimation(int textLength, TextBlock textBlock, int length, Canvas xName)
    {
        // Oblicz szerokość tekstu w pikselach
        var text = textBlock.Text;
        var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight,
            textBlock.FontStretch);
        var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface,
            textBlock.FontSize, Brushes.Black);
        var textWidth = formattedText.Width;
        if (textLength > length)
        {
            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = xName.ActualWidth;
            doubleAnimation.To = -textWidth;
            doubleAnimation.AutoReverse = false;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = TimeSpan.FromSeconds(10);
            textBlock.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
        }
        else
        {
            // Jeśli text jest mniejszy niż 16, usuń animację z właściwości Canvas.LeftProperty, aby zatrzymać animację
            textBlock.BeginAnimation(Canvas.LeftProperty, null);
        }
    }
}