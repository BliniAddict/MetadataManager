namespace MetadataManager
{
  public partial class MainPage : ContentPage
  {
    private readonly MainViewModel _viewModel;

    public MainPage()
    {
      InitializeComponent();

      _viewModel = new MainViewModel(new MediaProcessor());
      BindingContext = _viewModel;

      _viewModel.ValidateSelectedFilesRequested += ValidateSelectedFiles;
      _viewModel.ScrollToItemRequested += ScrollToItem;
    }

    void ValidateSelectedFiles(List<MediaFile> selectedFiles)
    {
      ClearSelectionButton.IsEnabled = selectedFiles.Count >= 1;

      if (selectedFiles.Distinct().Count() == 1)
      {
        CoverImageButton.Source = _viewModel.ChangableFile.Cover;

        FileNameEntry.IsEnabled = true;
        MediaPlayer.IsVisible = true;
      }
      else
      {
        if (selectedFiles.Any(noCover => noCover.Cover != null))
          CoverImageButton.Source = ImageSource.FromFile(_viewModel._diverseCover);

        FileNameEntry.IsEnabled = false;
        MediaPlayer.IsVisible = selectedFiles.Count == 0;
      }
    }

    void ScrollToItem(Group<MediaFile> grp, MediaFile title)
    {
      FilesCollection.ScrollTo(title, grp, animate: false, position: ScrollToPosition.MakeVisible);
    }
  }
}