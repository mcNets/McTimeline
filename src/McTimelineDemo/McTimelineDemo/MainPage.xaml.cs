namespace McTimelineDemo;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = new MainViewModel();

    public MainPage()
    {
        this.InitializeComponent();
        Task.Run(async () => await ViewModel.GenerarNovaCollecio());
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        var theme = App.RootFrame.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        var srv = App.RootFrame.RequestedTheme = theme;
    }
}
