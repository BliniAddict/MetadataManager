using MetadataManager.Resources;

namespace MetadataManager;

public partial class CustomEntry : ContentView
{
	public CustomEntry()
	{
		InitializeComponent();
	}

  private async void OnInfoButtonClicked(object sender, EventArgs e)
  {
    await Application.Current.MainPage.DisplayAlert(AppResources.DiverseDataManagement1, AppResources.DiverseDataManagement2, "OK");
  }


  public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(CustomEntry), string.Empty);

  public string LabelText
  {
    get => (string)GetValue(LabelTextProperty);
    set => SetValue(LabelTextProperty, value);
  }


  public static readonly BindableProperty EntryTextProperty =
        BindableProperty.Create(nameof(EntryText), typeof(string), typeof(CustomEntry), string.Empty);

  public string EntryText
  {
    get => (string)GetValue(EntryTextProperty);
    set => SetValue(EntryTextProperty, value);
  }


  public static readonly BindableProperty IsInfoButtonVisibleProperty =
        BindableProperty.Create(nameof(IsInfoButtonVisible), typeof(bool), typeof(CustomEntry));

  public bool IsInfoButtonVisible
  {
    get => (bool)GetValue(IsInfoButtonVisibleProperty);
    set => SetValue(IsInfoButtonVisibleProperty, value);
  }



  public static readonly BindableProperty PopupTextProperty =
        BindableProperty.Create(nameof(PopupText), typeof(string), typeof(CustomEntry), string.Empty);

  public string PopupText
  {
    get => (string)GetValue(PopupTextProperty);
    set => SetValue(PopupTextProperty, value);
  }
}