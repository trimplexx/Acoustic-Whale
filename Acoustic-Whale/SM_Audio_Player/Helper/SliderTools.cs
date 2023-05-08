using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SM_Audio_Player.Helper;

/// <summary>
/// Klasa SliderTools zapożyczona z gotowych rozwiązań znaleziona na StackOverflow, odpowiada naprawienie działania
/// slidera, odpowiadającego za przewijanie się podczas grania muzyki. Dzięki tej właściwości przypisany zostaje
/// event LeftMouseButtonDown dla wybrania losowego miejsca slidera, dzięki czemu ten nie cofa się do poprzedniej wartości
/// w momencie przytrzymania przycisku myszy
/// </summary>
public class SliderTools : DependencyObject
{
    public static bool GetMoveToPointOnDrag(DependencyObject obj)
    {
        return (bool)obj.GetValue(MoveToPointOnDragProperty);
    }

    public static void SetMoveToPointOnDrag(DependencyObject obj, bool value)
    {
        obj.SetValue(MoveToPointOnDragProperty, value);
    }

    public static readonly DependencyProperty MoveToPointOnDragProperty = DependencyProperty.RegisterAttached(
        "MoveToPointOnDrag", typeof(bool), typeof(SliderTools), new PropertyMetadata
        {
            PropertyChangedCallback = (obj, changeEvent) =>
            {
                var slider = (Slider)obj;
                if ((bool)changeEvent.NewValue)
                    slider.MouseMove += (_, mouseEvent) =>
                    {
                        if (mouseEvent.LeftButton == MouseButtonState.Pressed)
                            slider.RaiseEvent(new MouseButtonEventArgs(mouseEvent.MouseDevice, mouseEvent.Timestamp,
                                MouseButton.Left)
                            {
                                RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                                Source = mouseEvent.Source
                            });
                    };
            }
        });
}