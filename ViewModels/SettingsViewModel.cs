using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace MetadataManager
{
  public partial class SettingsViewModel : ObservableObject
  {
    public SettingsViewModel(MediaProcessor processor)
    {
      Media = processor;
      processor.LoadSettings();

      OGLocalPath = processor.LocalPath;
      OGMetadataVersion = processor.SelectedMetadataVersion;
      OGLanguage = processor.SelectedLanguage;
    }


    [ObservableProperty]
    MediaProcessor media;

    [ObservableProperty]
    string oGLocalPath;

    [ObservableProperty]
    string oGMetadataVersion;

    [ObservableProperty]
    string oGLanguage;

    [RelayCommand]
    async Task PickNewOGLocalPath()
    {
      Preferences.Set("LocalPath", await Media.PickLocalPath());
      Media.LocalPath = Preferences.Get("LocalPath", string.Empty);
      OGLocalPath = Media.LocalPath;
    }

    partial void OnOGMetadataVersionChanged(string value)
    {
      if (!string.IsNullOrWhiteSpace(Media.SelectedMetadataVersion) && !string.IsNullOrWhiteSpace(OGMetadataVersion))
        Preferences.Set("MetadataVersion", OGMetadataVersion);
      Media.SelectedMetadataVersion = Preferences.Get("MetadataVersion", Media.MetadataVersionList[1]);
      OGMetadataVersion = Media.SelectedMetadataVersion;
    }

    partial void OnOGLanguageChanged(string value)
    {
      bool isRestartRequired = false;
      if (!string.IsNullOrWhiteSpace(value))
      {
        if (value != Preferences.Get("Language", "English"))
          isRestartRequired = true;
        Preferences.Set("Language", value);
      }

      media.GetLanguage();
      OGLanguage = media.SelectedLanguage;

      if (isRestartRequired)
      {
        var filename = Process.GetCurrentProcess().MainModule.FileName;
        Process.Start(filename);
        Application.Current.Quit();
      }
    }
  }
}