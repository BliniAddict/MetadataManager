using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.RegularExpressions;

namespace MetadataManager
{
  public partial class MediaFile : ObservableObject
  {
    #region Constructors
    public MediaFile(MediaFile file)
    {
      Title = file.Title;
      Artist = file.Artist;
      Album = file.Album;
      AlbumArtist = file.AlbumArtist;
      Genre = file.Genre;
      Year = file.Year;
      TitleNumber = file.TitleNumber;
      CDNumber = file.CDNumber;
      FileName = file.FileName;
      FullFilePath = file.FullFilePath;
      IsVideo = file.IsVideo;
      Cover = file.Cover;
      CoverPath = file.CoverPath;
    }

    public MediaFile(string title,
                string artist,
                string album,
                string albumArtist,
                string genre,
                int? year,
                int? titleNumber,
                int? cDNumber,
                TagLib.IPicture cover,
                string filePath)
    {
      Title = title ?? string.Empty;
      Artist = artist ?? string.Empty;
      Album = album ?? string.Empty;
      AlbumArtist = albumArtist ?? string.Empty;
      Genre = genre ?? string.Empty;
      Year = year.ToString() ?? string.Empty;
      TitleNumber = titleNumber.ToString() ?? string.Empty;
      CDNumber = cDNumber.ToString() ?? string.Empty;
      Cover = cover?.Data.Data != null ? ImageSource.FromStream(() => new MemoryStream(cover.Data.Data)) : null;

      FileName = filePath.Remove(filePath.LastIndexOf(".")).Substring(filePath.LastIndexOf("\\") + 1);
      FullFilePath = filePath;
      IsVideo = filePath.EndsWith(".mp4") || filePath.EndsWith(".mkv") || filePath.EndsWith(".webm");
    }
    #endregion

    #region Properties
    [ObservableProperty]
    string title;

    [ObservableProperty]
    string artist;

    [ObservableProperty]
    string album;

    [ObservableProperty]
    string albumArtist;

    [ObservableProperty]
    string genre;

    [ObservableProperty]
    string year;
    partial void OnYearChanged(string value)
    {
      if (value != "diverse")
        Year = Regex.Replace(value, @"[^0-9-]", "");
    }

    [ObservableProperty]
    string titleNumber;
    partial void OnTitleNumberChanged(string value)
    {
      if (value != "diverse")
        TitleNumber = Regex.Replace(value, @"[^0-9-]", "");
    }

    [ObservableProperty]
    string cDNumber;
    partial void OnCDNumberChanged(string value)
    {
      if (value != "diverse")
        CDNumber = Regex.Replace(value, @"[^0-9-]", "");
    }

    [ObservableProperty]
    string fileName;

    [ObservableProperty]
    string fullFilePath;

    [ObservableProperty]
    ImageSource? cover;

    public string? CoverPath { get; set; }
    public bool IsVideo { get; set; }
    #endregion

    public MediaFile Clone() => new(this);
  }
}