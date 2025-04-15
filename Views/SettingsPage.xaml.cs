namespace MetadataManager;

public partial class SettingsPage : ContentPage
{
  private SettingsViewModel _viewModel;

  public SettingsPage(SettingsViewModel viewModel)
  {
    InitializeComponent();

    _viewModel = viewModel;
    BindingContext = _viewModel;
  }
}