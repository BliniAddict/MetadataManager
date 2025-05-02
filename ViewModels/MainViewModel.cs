using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System.Collections.ObjectModel;
using TagLib;
using MetadataManager.Resources;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Xabe.FFmpeg.Downloader;
using System.Globalization;
using System.Diagnostics;

namespace MetadataManager
{
  public partial class MainViewModel : ObservableObject
  {
    public readonly string _diverse = "diverse";
    public readonly string _diverseCover = "these_are_different_files_bro.png";
    private byte _tempMetadataVersion;
    private IToast _toast;

    public MainViewModel(MediaProcessor processor)
    {
      FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
      Media = processor;
      processor.LoadSettings();

      TempPath = Media.LocalPath;
      _tempMetadataVersion = byte.Parse(Media.SelectedMetadataVersion[^1..]);
    }

    public event Action<List<MediaFile>>? ValidateSelectedFilesRequested;
    public event Action<Group<MediaFile>, MediaFile> ScrollToItemRequested;

    #region Properties
    [ObservableProperty]
    MediaProcessor media;

    [ObservableProperty]
    string tempPath;
    partial void OnTempPathChanged(string value) => GetAllFiles();

    [ObservableProperty]
    ObservableCollection<Group<MediaFile>> groupedFilesList = new ObservableCollection<Group<MediaFile>>();

    [ObservableProperty]
    ObservableCollection<object> selectedFiles = [];

    [ObservableProperty]
    bool isGroupingByAlbum = false;
    partial void OnIsGroupingByAlbumChanged(bool value) => GetAllFiles();

    [ObservableProperty]
    MediaFile? changableFile;

    [ObservableProperty]
    bool isSavable = false;
    #endregion

    #region Methods
    [RelayCommand]
    async void PickDifferentCover()
    {
      FileResult? result = await FilePicker.PickAsync(new PickOptions
      {
        FileTypes = FilePickerFileType.Images
      });
      if (result != null && this != null)
      {
        if (ImageSource.FromFile(result.FullPath) is FileImageSource fileSource)
        {
          using var stream = System.IO.File.OpenRead(fileSource.File);
          using var skBitmap = SKBitmap.Decode(stream);
          if (skBitmap.Width < 500 || skBitmap.Height < 500)
          {
            if (await Application.Current.MainPage.DisplayAlert
              (AppResources.Warning, AppResources.BadPicQualityManagement, AppResources.Yes, AppResources.No) == false)
              return;
          }

          ChangableFile.Cover = fileSource;
          ChangableFile.CoverPath = result.FullPath;
        }
      }
    }

    [RelayCommand]
    async void SaveCoverLocally()
    {
      if (ChangableFile != null && ChangableFile.Cover != null && ChangableFile.Cover is StreamImageSource streamImageSource)
      {
        Stream stream = await streamImageSource.Stream(CancellationToken.None);
        string defautName = ChangableFile.FileName + "_cover.jpg";

        FileSaverResult res = await FileSaver.Default.SaveAsync(Environment.SpecialFolder.MyPictures.ToString(), defautName, stream, CancellationToken.None);

        if (res.IsSuccessful)
          _toast = Toast.Make(string.Format(AppResources.FileSavedSuccessfullyManagement, res.FilePath), ToastDuration.Short);
        else
          _toast = Toast.Make(string.Format(AppResources.FileSavedSuccessfullyManagement, res.Exception.Message), ToastDuration.Short);
        _toast.Show(CancellationToken.None);
      }
    }

    [RelayCommand]
    void RemoveCover()
    {
      if (ChangableFile?.Cover != null)
        ChangableFile.Cover = null;
    }

    [RelayCommand]
    async Task ConvertVideoToMP3()
    {
      TagLib.File file = TagLib.File.Create(ChangableFile.FullFilePath);
      await Media.ConvertToMp3(ChangableFile.FullFilePath);
      GetAllFiles();

      _toast = Toast.Make(AppResources.ConvertedSuccessfullyManagement, ToastDuration.Short);
      _toast.Show(CancellationToken.None);
    }


    [RelayCommand]
    public async Task PickLocalPath() => TempPath = await Media.PickLocalPath() ?? Media.LocalPath;

    [RelayCommand]
    void ReloadFiles() => GetAllFiles();

    [RelayCommand]
    void ClearSelectedFiles()
    {
      selectedFiles.Clear();
      ValidateSelectedFilesRequested?.Invoke(new List<MediaFile>());
    }


    [RelayCommand]
    void FileSelectionChanged()
    {
      List<MediaFile> files = [];
      if (SelectedFiles == null || SelectedFiles.Count < 1)
      {
        ChangableFile = null;
        IsSavable = false;
      }
      else
      {
        files = SelectedFiles.OfType<MediaFile>().ToList();
        ChangableFile = files.LastOrDefault().Clone();
        IsSavable = true;

        void AssignProperty<T>(List<MediaFile> files, Func<MediaFile, T> selector, Action<string> assign)
        {
          var distinctValues = files.Select(selector).Distinct().ToList();
          assign(distinctValues.Count == 1 ? distinctValues[0]?.ToString() : _diverse);
        }

        AssignProperty(files, f => f.Title, value => ChangableFile.Title = value);
        AssignProperty(files, f => f.Artist, value => ChangableFile.Artist = value);
        AssignProperty(files, f => f.Album, value => ChangableFile.Album = value);
        AssignProperty(files, f => f.AlbumArtist, value => ChangableFile.AlbumArtist = value);
        AssignProperty(files, f => f.Genre, value => ChangableFile.Genre = value);
        AssignProperty(files, f => f.Year, value => ChangableFile.Year = value);
        AssignProperty(files, f => f.TitleNumber, value => ChangableFile.TitleNumber = value);
        AssignProperty(files, f => f.CDNumber, value => ChangableFile.CDNumber = value);
        AssignProperty(files, f => f.FileName, value => ChangableFile.FileName = value);
      }
      ValidateSelectedFilesRequested?.Invoke(files);
    }

    [RelayCommand]
    void SelectGroup(Group<MediaFile> group)
    {
      List<MediaFile> sortedGroup = [.. group.OrderBy(name => name.FileName)];
      List<MediaFile> sortedPreviousFiles = [.. SelectedFiles.OfType<MediaFile>().OrderBy(name => name.FileName)];

      foreach (var file in group)
      {
        if (sortedGroup.SequenceEqual(sortedPreviousFiles))
          SelectedFiles.Remove(file);

        else if (!SelectedFiles.Contains(file))
          SelectedFiles.Add(file);
      }
    }


    [RelayCommand]
    async Task DeleteFiles()
    {
      if (await Application.Current.MainPage.DisplayAlert
        (AppResources.Warning, string.Format(AppResources.UnwantedFilesManagement, selectedFiles.Count), AppResources.Yes, AppResources.No) == true)
      {
        List<MediaFile> files = [.. SelectedFiles.OfType<MediaFile>()];
        foreach (var file in files)
        {
          if (System.IO.File.Exists(file.FullFilePath))
            System.IO.File.Delete(file.FullFilePath);

          Group<MediaFile>? parent = GroupedFilesList.FirstOrDefault(a => !IsGroupingByAlbum ? file.AlbumArtist == a.FirstTitle : file.Album == a.FirstTitle);
          parent.Remove(file);

          if (parent.Count == 0)
            GroupedFilesList.Remove(parent);
        }

        _toast = Toast.Make(AppResources.FilesDeletedManagement, ToastDuration.Short);
        _toast.Show(CancellationToken.None);
      }
    }

    [RelayCommand]
    async void SaveChanges()
    {
      if (SelectedFiles.Count < 1)
        return;

      MediaFile changes = new(ChangableFile);
      List<MediaFile> files = [.. SelectedFiles.OfType<MediaFile>()];
      MediaFile? lastSelectedFile = null;
      bool shouldReloadFiles = false;

      foreach (MediaFile oldFile in files)
      {
        Group<MediaFile>? parent = GroupedFilesList.FirstOrDefault(a => !IsGroupingByAlbum ? oldFile.AlbumArtist == a.FirstTitle : oldFile.Album == a.FirstTitle);
        int fileIndex = parent.IndexOf(oldFile);

        MediaFile newFile = ChangeFileMetaData(oldFile, changes);
        shouldReloadFiles = (IsGroupingByAlbum && newFile.Album != oldFile.Album) || (!IsGroupingByAlbum && newFile.AlbumArtist != oldFile.AlbumArtist);

        parent.RemoveAt(fileIndex);
        if (!shouldReloadFiles)
          parent.Insert(fileIndex, newFile);

        lastSelectedFile = newFile;
      }
      ChangableFile = null;

      if (shouldReloadFiles)
        GetAllFiles();

        Group<MediaFile>? lastSelectedArtist = GroupedFilesList.LastOrDefault(a => !IsGroupingByAlbum ? a.FirstTitle == files.Last().AlbumArtist : a.FirstTitle == files.Last().Album);
        if (lastSelectedArtist != null)
          ScrollToItemRequested?.Invoke(lastSelectedArtist, lastSelectedFile);

      if (files.Count > 2)
      {
        var currentShell = Shell.Current;

        if (currentShell != null)
        {
          var currentRoute = Shell.Current.CurrentState.Location.ToString();
          await Shell.Current.GoToAsync("..");
          await Shell.Current.GoToAsync(currentRoute);
        }
      }

      _toast = Toast.Make(string.Format(AppResources.ChangesSavedManagement, files.Count), ToastDuration.Short);
      _toast.Show(CancellationToken.None);
    }

    #region Helper-Methods

    public void GetAllFiles()
    {
      List<MediaFile> _filesList = [];
      GroupedFilesList.Clear();

      string[] Files = Directory.GetFiles(TempPath ?? Media.LocalPath)
                                .Where(file => file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                               file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)).ToArray();
      foreach (string file in Files)
      {
        TagLib.File tagLibFile = TagLib.File.Create(file.Replace("\\\\", "\\"));
        _filesList.Add(ConvertToMediaFile(tagLibFile));
      }

      if (!isGroupingByAlbum)
        GroupedFilesList = [.. from file in _filesList
                               .OrderBy(t => t.Title)
                               .OrderBy(n => int.TryParse(n.TitleNumber, out int i) ? (int?)i : null)
                               .OrderBy(cd => int.TryParse(cd.CDNumber, out int i) ? (int?)i : null)
                               .OrderBy(a => a.Album)
                               .OrderBy(y => int.TryParse(y.Year, out int i) ? (int?)i : null)
                               group file by file.AlbumArtist into g
                               orderby g.Key
                               select new Group<MediaFile>(g.Key, [.. g])];
      else
        GroupedFilesList = [.. from file in _filesList
                               .OrderBy(t => t.Title)
                               .OrderBy(n => int.TryParse(n.TitleNumber, out int i) ? (int?)i : null)
                               .OrderBy(cd => int.TryParse(cd.CDNumber, out int i) ? (int?)i : null)
                               .OrderBy(y => int.TryParse(y.Year, out int i) ? (int?)i : null)
                               group file by file.Album into g
                               orderby g.Key
                               select new Group<MediaFile>(g.Key, [.. g])];
    }

    MediaFile ChangeFileMetaData(MediaFile file, MediaFile changes)
    {
      if (file.FileName != changes.FileName && changes.FileName != _diverse)
      {
        char p = '.';
        string oldName = file.FullFilePath[..(file.FullFilePath.LastIndexOf(p) + 1)];
        string newName = changes.FullFilePath[..(changes.FullFilePath.LastIndexOf("\\") + 1)] + changes.FileName + p;

        file.FullFilePath = Media.RenameFileAccurately(oldName, newName, changes.FullFilePath[(changes.FullFilePath.LastIndexOf(p) + 1)..]);
      }

      TagLib.File newFile = TagLib.File.Create(file.FullFilePath);
      TagLib.Id3v2.Tag.DefaultVersion = _tempMetadataVersion;
      TagLib.Id3v2.Tag.ForceDefaultVersion = true;

      if (changes.Cover == null && file.Cover != null)
        newFile.Tag.Pictures = null;
      else if (changes.Cover != null && !(changes.Cover is FileImageSource src && src.File == _diverseCover))
      {
        if (!string.IsNullOrWhiteSpace(changes.CoverPath))
          newFile.Tag.Pictures = [ConvertToTagLibPicture(changes.CoverPath)];
      }

      newFile.Tag.Title = changes?.Title == _diverse ? file.Title : changes?.Title.Trim();
      newFile.Tag.Performers = [changes?.Artist == _diverse ? file.Artist : changes?.Artist.Trim()];
      newFile.Tag.Album = changes?.Album == _diverse ? file.Album : changes?.Album.Trim();
      newFile.Tag.AlbumArtists = [changes?.AlbumArtist == _diverse ? file.AlbumArtist : changes?.AlbumArtist.Trim()];
      newFile.Tag.Genres = [changes?.Genre == _diverse ? file.Genre : changes?.Genre.Trim()];
      newFile.Tag.Year = uint.TryParse(changes?.Year == _diverse ? file.Year : changes?.Year.Trim(), out uint year) ? year : 0;
      newFile.Tag.Track = uint.TryParse(changes?.TitleNumber == _diverse ? file.TitleNumber : changes?.TitleNumber.Trim(), out uint track) ? track : 0;
      newFile.Tag.Disc = uint.TryParse(changes?.CDNumber == _diverse ? file.CDNumber : changes?.CDNumber.Trim(), out uint disc) ? disc : 0;

      newFile.Save();
      return ConvertToMediaFile(newFile);
    }

    MediaFile ConvertToMediaFile(TagLib.File tagLibFile)
    {
      return new MediaFile(
          tagLibFile.Tag.Title,
          tagLibFile.Tag.FirstPerformer,
          tagLibFile.Tag.Album,
          tagLibFile.Tag.FirstAlbumArtist,
          tagLibFile.Tag.FirstGenre,
          tagLibFile.Tag.Year == 0 ? null : (int)tagLibFile.Tag.Year,
          tagLibFile.Tag.Track == 0 ? null : (int)tagLibFile.Tag.Track,
          tagLibFile.Tag.Disc == 0 ? null : (int)tagLibFile.Tag.Disc,
          tagLibFile.Tag.Pictures.Length > 0 ? tagLibFile.Tag.Pictures[0] : null,
          tagLibFile.Name
          );
    }
    Picture? ConvertToTagLibPicture(string path)
    {
      if (System.IO.File.Exists(path))
      {
        byte[] bildDaten = System.IO.File.ReadAllBytes(path);
        var picture = new Picture(new ByteVector(bildDaten))
        {
          Type = PictureType.FrontCover,
          Description = "Cover",
          MimeType = $"image/{path[path.LastIndexOf(".")..]}"
        };
        return picture;
      }
      return null;
    }
    #endregion

    #endregion
  }
}