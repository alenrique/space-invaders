using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Space_Invaders.Managers;
using Space_Invaders.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Space_Invaders
{
    public sealed partial class RankingPage : Page
    {
        private readonly GameManager _gameManager;

        public RankingPage()
        {
            this.InitializeComponent();
            _gameManager = new GameManager(); // Re-use or pass existing instance if possible
            this.Loaded += RankingPage_Loaded;
        }

        private async void RankingPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRanking();
        }

        private async Task LoadRanking()
        {
            List<ScoreEntry> scores = await _gameManager.LoadScores();
            RankingListView.ItemsSource = scores;
        }

        private void OnBackToMainMenuClick(object sender, RoutedEventArgs e)
        {
            // Navigate back to the main page
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}