using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace MetadataManager
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder();
      builder = SetDependencies(builder);
      builder
          .UseMauiCommunityToolkit(options =>
            {
              options.SetShouldEnableSnackbarOnWindows(true);
            })
          .UseMauiCommunityToolkitMediaElement()
          .UseMauiApp<App>()
          .ConfigureFonts(fonts =>
          {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
          });

#if DEBUG
      builder.Logging.AddDebug();
#endif



      var app = builder.Build();
      var mediaProcessor = app.Services.GetRequiredService<MediaProcessor>();
      mediaProcessor.LoadSettings();

      return app;
    }


    static MauiAppBuilder SetDependencies(MauiAppBuilder builder)
    {
      builder.Services.AddSingleton<MediaProcessor>();

      builder.Services.AddSingleton<MainViewModel>();
      builder.Services.AddSingleton<SettingsViewModel>();

      builder.Services.AddSingleton<MainPage>();
      builder.Services.AddSingleton<SettingsPage>();

      return builder;
    }
  }
}
