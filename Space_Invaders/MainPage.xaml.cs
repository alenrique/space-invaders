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

        // Imagem principal
        var mainImage = new Image
        {
            Width = 400,
            Height = 200,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/space_invaders.png"))
        };
        Canvas.SetLeft(mainImage, 100);
        Canvas.SetTop(mainImage, 10);
        GameCanvas.Children.Add(mainImage);

        // Ícone e texto 1
        var img1 = new Image
        {
            Width = 40,
            Height = 40,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/enemy1.png"))
        };
        Canvas.SetLeft(img1, 200);
        Canvas.SetTop(img1, 250);
        GameCanvas.Children.Add(img1);

        var txt1 = new TextBlock
        {
            Text = " = 10 pts",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 30,
            FontFamily = new FontFamily("ms-appx:///Assets/Fonts/PixelifySans-VariableFont_wght.ttf"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Canvas.SetLeft(txt1, 250);
        Canvas.SetTop(txt1, 250 + (40 - 40) / 2);
        GameCanvas.Children.Add(txt1);

        // Ícone e texto 2
        var img2 = new Image
        {
            Width = 40,
            Height = 40,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/enemy2.png"))
        };
        Canvas.SetLeft(img2, 200);
        Canvas.SetTop(img2, 300);
        GameCanvas.Children.Add(img2);

        var txt2 = new TextBlock
        {
            Text = " = 20 pts",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 30,
            FontFamily = new FontFamily("ms-appx:///Assets/Fonts/PixelifySans-VariableFont_wght.ttf"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Canvas.SetLeft(txt2, 250);
        Canvas.SetTop(txt2, 300 + (40 - 40) / 2);
        GameCanvas.Children.Add(txt2);

        // Ícone e texto 3
        var img3 = new Image
        {
            Width = 40,
            Height = 40,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/enemy3.png"))
        };
        Canvas.SetLeft(img3, 200);
        Canvas.SetTop(img3, 350);
        GameCanvas.Children.Add(img3);

        var txt3 = new TextBlock
        {
            Text = " = 40 pts",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 30,
            FontFamily = new FontFamily("ms-appx:///Assets/Fonts/PixelifySans-VariableFont_wght.ttf"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Canvas.SetLeft(txt3, 250);
        Canvas.SetTop(txt3, 350 + (40 - 40) / 2);
        GameCanvas.Children.Add(txt3);

        // Ícone e texto 4 (especial)
        var img4 = new Image
        {
            Width = 40,
            Height = 40,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/enemy4.png"))
        };
        Canvas.SetLeft(img4, 200);
        Canvas.SetTop(img4, 400);
        GameCanvas.Children.Add(img4);

        var txt4 = new TextBlock
        {
            Text = " = ??? Pontos",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 30,
            FontFamily = new FontFamily("ms-appx:///Assets/Fonts/PixelifySans-VariableFont_wght.ttf"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Canvas.SetLeft(txt4, 250);
        Canvas.SetTop(txt4, 400 + (40 - 40) / 2);
        GameCanvas.Children.Add(txt4);

        // Cria o TextBlock que vai dentro do botão
        var contentText = new TextBlock
        {
            Text = "Iniciar Jogo",
            FontSize = 28,
            FontFamily = new FontFamily("ms-appx:///Assets/Fonts/PixelifySans-VariableFont_wght.ttf"),
            Foreground = new SolidColorBrush(Colors.White)
        };

        // Cria o botão e coloca o TextBlock dentro
        var startButton = new Button
        {
            Background = null,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Content = contentText
        };

        // Posicionamento no canvas
        Canvas.SetLeft(startButton, 220);
        Canvas.SetTop(startButton, 450);

        // Evento de clique
        startButton.Click += StartButton_Click;

        // Evento de hover (muda cor do texto, não do botão)
        startButton.PointerEntered += (s, e) => contentText.Foreground = new SolidColorBrush(Colors.Lime);

        startButton.PointerExited += (s, e) => contentText.Foreground = new SolidColorBrush(Colors.White);

        GameCanvas.Children.Add(startButton);
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(GamePage));
        var dialog = new Windows.UI.Popups.MessageDialog("Jogo iniciado!");
        _ = dialog.ShowAsync();
    }
}
