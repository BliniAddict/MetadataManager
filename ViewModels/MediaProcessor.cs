using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using MetadataManager.Resources;
using System.Diagnostics;
using System.Globalization;
using Xabe.FFmpeg;

namespace MetadataManager
{
  public partial class MediaProcessor : ObservableObject
  {
    public void LoadSettings()
    {
      //Path
      LocalPath = Preferences.Get("LocalPath", string.Empty);
      if (string.IsNullOrWhiteSpace(LocalPath) || !Directory.Exists(LocalPath))
      {
        Preferences.Set("LocalPath", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        LocalPath = Preferences.Get("LocalPath", string.Empty);
      }

      //Idv3Version
      SelectedMetadataVersion = Preferences.Get("MetadataVersion", MetadataVersionList[1]);

      //Language
      GetLanguage();
    }

    #region Language
    public List<string> LanguageList { get; } = ["English", "Deutsch"];
    public string SelectedLanguage;

    public void GetLanguage()
    {
      SelectedLanguage = Preferences.Get("Language", "English");

      CultureInfo? culture = null;
      switch (SelectedLanguage)
      {
        case "English":
          culture = new CultureInfo("en");
          break;
        case "Deutsch":
          culture = new CultureInfo("de");
          break;
        default:
          break;
      }

      Thread.CurrentThread.CurrentCulture = culture;
      Thread.CurrentThread.CurrentUICulture = culture;
      CultureInfo.DefaultThreadCurrentCulture = culture;
      CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
    #endregion

    #region MetadataVersion
    public List<string> MetadataVersionList { get; } = ["ID3v2.2", "ID3v2.3", "ID3v2.4"];

    public string SelectedMetadataVersion;
    #endregion

    #region Download-Path
    public string LocalPath;

    public async Task<string> PickLocalPath(CancellationToken token = new CancellationToken())
    {
      var result = await FolderPicker.Default.PickAsync(token);
      if (result.IsSuccessful)
        return result.Folder.Path;

      return null;
    }
    #endregion

    #region FileName/Path
    public string RenameFileAccurately(string oldPath, string newPath, string container = "")
    {
      string newerPath = GetUniqueFilePath(newPath, container);
      try
      {
        if (!string.IsNullOrWhiteSpace(container))
          File.Move(oldPath + container, newerPath);
        return newerPath;
      }
      catch (IOException) //Name ist nicht schreibbar
      {
        string unnamedPath = oldPath.Remove(oldPath.LastIndexOf("\\")) + "\\" + "Unnamed.";
        newerPath = GetUniqueFilePath(unnamedPath, container);

        if (!string.IsNullOrWhiteSpace(container))
          File.Move(oldPath + container, newerPath);
        return newerPath;
      }
    }
    string GetUniqueFilePath(string basePath, string container = "")
    {
      bool isFolder = string.IsNullOrWhiteSpace(container);
      string pathWithoutExt = basePath.Remove(basePath.Length - 1);
      string uniquePath = isFolder ? basePath : $"{pathWithoutExt}.{container}";
      int index = 1;

      while (File.Exists(uniquePath) || Directory.Exists(uniquePath))
      {
        uniquePath = isFolder ? $"{basePath}({index})" : $"{pathWithoutExt}({index}).{container}";
        index++;
      }


      uniquePath = uniquePath.Replace("<", "");
      uniquePath = uniquePath.Replace(">", "");
      uniquePath = uniquePath.Replace(":", "");
      uniquePath = uniquePath.Replace("\"", "");
      uniquePath = uniquePath.Replace("/", "");
      uniquePath = uniquePath.Replace("\\", "");
      uniquePath = uniquePath.Replace("|", "");
      uniquePath = uniquePath.Replace("?", "");
      uniquePath = uniquePath.Replace("*", "");
      return uniquePath;
    }
    #endregion

    #region FFMPEG-Methods
    public async Task ConvertToMp3(string path)
    {
      string mp3Path = path.Split(".")[0] + ".mp3";
      var snippet = await FFmpeg.Conversions.FromSnippet.Convert(path, mp3Path);
      IConversionResult result = await snippet.Start();

      File.Delete(path);
    }
    #endregion
  }
}