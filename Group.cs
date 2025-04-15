using System.Collections.ObjectModel;

namespace MetadataManager
{
  public class Group<T> : ObservableCollection<T>
  {
    public Group(string title, IEnumerable<T> items) : base(items)
    {
      FirstTitle = title;
    }
    public string FirstTitle { get; }
  }
}
