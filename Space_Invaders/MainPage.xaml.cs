using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;

namespace Space_Invaders;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(GamePage));
        var dialog = new Windows.UI.Popups.MessageDialog("Jogo iniciado!");
        _ = dialog.ShowAsync();
    }

    private void StartButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        contentText.Foreground = new SolidColorBrush(Colors.Lime);
    }

    private void StartButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        contentText.Foreground = new SolidColorBrush(Colors.White);
    }

    private void RankingButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(RankingPage));
    }

    private void RankingButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Button button && button.Content is TextBlock textBlock)
        {
            textBlock.Foreground = new SolidColorBrush(Colors.Lime);
        }
    }

    private void RankingButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Button button && button.Content is TextBlock textBlock)
        {
            textBlock.Foreground = new SolidColorBrush(Colors.White);
        }
    }
}