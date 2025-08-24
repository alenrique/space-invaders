using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Space_Invaders.Models;
using Space_Invaders.Utils;
using Space_Invaders.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.System;

namespace Space_Invaders;

public sealed partial class GamePage : Page
{
    private readonly GameManager _gameManager;
    private readonly DispatcherTimer _mainGameLoop;
    private readonly SoundManager? _audioSystem;

    private readonly Dictionary<object, FrameworkElement> _sprites = new();

    public GamePage()
    {
        this.InitializeComponent();
        _gameManager = new GameManager();
        this.DataContext = _gameManager;

        this.Loaded += OnPageLoaded;
        this.Unloaded += OnPageUnloaded;

        _audioSystem = new SoundManager();

        _gameManager.Enemies.CollectionChanged += Entities_CollectionChanged;
        _gameManager.Bullets.CollectionChanged += Entities_CollectionChanged;
        _gameManager.Barriers.CollectionChanged += Entities_CollectionChanged;

        InitializeVisuals();

        _mainGameLoop = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _mainGameLoop.Tick += OnGameLoopTick;
        _mainGameLoop.Start();
    }

    private void InitializeVisuals()
    {
        GameCanvas.Children.Clear();
        _sprites.Clear();
        
        GameCanvas.Children.Add(ScoreText);
        GameCanvas.Children.Add(LifesText);
        GameCanvas.Children.Add(GameOverText);

        CreateSprite(_gameManager.CurrentPlayer);
        foreach (var barrier in _gameManager.Barriers) CreateSprite(barrier);
        foreach (var enemy in _gameManager.Enemies) CreateSprite(enemy);
    }

    private void OnGameLoopTick(object? sender, object e)
    {
        if (_gameManager.IsGameOver)
        {
            GameOverText.Visibility = Visibility.Visible;
            _mainGameLoop.Stop();
            return;
        }

        _gameManager.UpdateGame();
        UpdateAllPositions();
    }

    private void UpdateAllPositions()
    {
        foreach (var (entity, sprite) in _sprites)
        {
            if (entity is Player p) Canvas.SetLeft(sprite, p.PosX);
            else if (entity is Enemy en) { Canvas.SetLeft(sprite, en.PosX); Canvas.SetTop(sprite, en.PosY); }
            else if (entity is Bullet b) { Canvas.SetLeft(sprite, b.PosX); Canvas.SetTop(sprite, b.PosY); }
            else if (entity is Wall w && !w.IsDestroyed && sprite is Rectangle r) { Canvas.SetLeft(sprite, w.PosX); Canvas.SetTop(sprite, w.PosY); r.Fill = w.GetWallColor(); }
        }
    }

    private void Entities_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                CreateSprite(item);
                if (item is Bullet bullet)
                {
                    if (bullet.Category == ShotCategory.Player) _audioSystem?.PlayPlayerShoot();
                    else _audioSystem?.PlayEnemyShoot();
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (_sprites.TryGetValue(item, out var sprite))
                {
                    GameCanvas.Children.Remove(sprite);
                    _sprites.Remove(item);
                    if (item is Enemy) _audioSystem?.PlayExplosion();
                }
            }
        }
    }

    private void CreateSprite(object entity)
    {
        FrameworkElement? sprite = null;
        if (entity is Player p)
        {
            sprite = new Image { Width = p.Width, Height = p.Height, Source = new BitmapImage(new Uri(p.ImagePath)) };
            Canvas.SetTop(sprite, p.PosY);
        }
        else if (entity is Enemy en)
        {
            sprite = new Image { Width = en.Width, Height = en.Height, Source = new BitmapImage(new Uri(en.ImagePath)) };
            Canvas.SetTop(sprite, en.PosY);
        }
        else if (entity is Bullet b)
        {
            sprite = new Rectangle
            {
                Width = 4, Height = 10,
                Fill = new SolidColorBrush(b.Category == ShotCategory.Player ? Colors.Yellow : Colors.White)
            };
            Canvas.SetTop(sprite, b.PosY);
        }
        else if (entity is Wall w)
        {
            sprite = new Rectangle { Width = w.Width, Height = w.Height, Fill = w.GetWallColor() };
            Canvas.SetTop(sprite, w.PosY);
            Canvas.SetLeft(sprite, w.PosX);
        }

        if (sprite != null)
        {
            _sprites[entity] = sprite;
            GameCanvas.Children.Add(sprite);
        }
    }

    private void OnPageLoaded(object? sender, RoutedEventArgs e)
    {
        this.Focus(FocusState.Programmatic);
        this.KeyDown += HandleKeyboardInput;
        _gameManager.GameWidth = GameCanvas.ActualWidth;
        _gameManager.GameHeight = GameCanvas.ActualHeight;
    }

    private void OnPageUnloaded(object? sender, RoutedEventArgs e)
    {
        _mainGameLoop.Stop();
        _audioSystem?.Dispose();
        this.KeyDown -= HandleKeyboardInput;
    }

    private void HandleKeyboardInput(object sender, KeyRoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Key Pressed: {e.Key}");
        switch (e.Key)
        {
            case VirtualKey.Left:
            case VirtualKey.A:
                _gameManager.MovePlayer(-1);
                break;
            case VirtualKey.Right:
            case VirtualKey.D:
                _gameManager.MovePlayer(1);
                break;
            case VirtualKey.Space:
                _gameManager.PlayerShoot();
                break;
        }
    }
}