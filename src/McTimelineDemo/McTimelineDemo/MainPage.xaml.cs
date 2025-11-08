namespace McTimelineDemo;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        var theme = App.RootFrame.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        var srv = App.RootFrame.RequestedTheme = theme;
    }
}
