using System.Globalization;

namespace MetadataManager
{
  public class FileNameConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is string fileName)
      {
        fileName = fileName.Replace("<", "");
        fileName = fileName.Replace(">", "");
        fileName = fileName.Replace(":", "");
        fileName = fileName.Replace("\"", "");
        fileName = fileName.Replace("/", "");
        fileName = fileName.Replace("\\", "");
        fileName = fileName.Replace("|", "");
        fileName = fileName.Replace("?", "");
        fileName = fileName.Replace("*", "");
        return fileName;
      }
      return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      return value;
    }
  }

  public class DiverseDataConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is string str)
      {
        if (str == "diverse")
          return true;
      }
      return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      return false;
    }
  }
}