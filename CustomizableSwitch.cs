namespace MetadataManager
{
  public class CustomizableSwitch : Switch
  {
    public static readonly BindableProperty OnTextProperty =
        BindableProperty.Create(nameof(OnText), typeof(string), typeof(CustomizableSwitch), "On", propertyChanged: OnTextPropertyChanged);

    public static readonly BindableProperty OffTextProperty =
        BindableProperty.Create(nameof(OffText), typeof(string), typeof(CustomizableSwitch), "Off", propertyChanged: OffTextPropertyChanged);

    public CustomizableSwitch()
    {
#if WINDOWS
        Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping(nameof(CustomizableSwitch), (handler, view) =>
        {
            // Initialwerte der Properties an die PlatformView übergeben
            if (view is CustomizableSwitch maniputableSwitch)
            {
                handler.PlatformView.OffContent = maniputableSwitch.OffText;
                handler.PlatformView.OnContent = maniputableSwitch.OnText;
            }
        });
#endif
    }

    public string OnText
    {
      get => (string)GetValue(OnTextProperty);
      set => SetValue(OnTextProperty, value);
    }

    public string OffText
    {
      get => (string)GetValue(OffTextProperty);
      set => SetValue(OffTextProperty, value);
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
#if WINDOWS
        if (bindable is CustomizableSwitch maniputableSwitch && maniputableSwitch.Handler != null)
        {
            var platformView = (Microsoft.UI.Xaml.Controls.ToggleSwitch)maniputableSwitch.Handler.PlatformView;
            platformView.OnContent = newValue?.ToString();
        }
#endif
    }

    private static void OffTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
#if WINDOWS
        if (bindable is CustomizableSwitch maniputableSwitch && maniputableSwitch.Handler != null)
        {
            var platformView = (Microsoft.UI.Xaml.Controls.ToggleSwitch)maniputableSwitch.Handler.PlatformView;
            platformView.OffContent = newValue?.ToString();
        }
#endif
    }
  }
}