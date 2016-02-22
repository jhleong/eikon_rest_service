namespace DailyIntervalDemo
{
    using System.Windows;
    using ThomsonReuters.Desktop.SDK.UI.Tools;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            UIBootstrapper.Initialize();
            base.OnStartup(e);
        }
    }
}