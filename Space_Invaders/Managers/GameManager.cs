using System.ComponentModel;
using System.Collections.ObjectModel;
using Space_Invaders.Models;
using Microsoft.UI;
using System.Linq;

namespace Space_Invaders.Managers;

public class GameManager : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Propriedades para Data Binding na UI
    private string _scoreText = "Score: 0";
    public string ScoreText
    {
        get => _scoreText;
        set {
            if (_scoreText != value)
            {
                _scoreText = value;
                OnPropertyChanged(nameof(ScoreText));
            }
        }
    }

    private string _livesText = "Lives: 3";
    public string LivesText
    {
        get => _livesText;
        set {
            if (_livesText != value)
            {
                _livesText = value;
                OnPropertyChanged(nameof(LivesText));
            }
        }
    }

    private bool _isGameOver = false;
    public bool IsGameOver
    {
        get => _isGameOver;
        set {
            if (_isGameOver != value)
            {
                _isGameOver = value;
                OnPropertyChanged(nameof(IsGameOver));
            }
        }
    }

    // Coleções para gerenciar as entidades do jogo
    public Player CurrentPlayer { get; private set; }
    public ObservableCollection<Enemy> Enemies { get; } = new();
    public ObservableCollection<Bullet> Bullets { get; } = new();
    public ObservableCollection<Wall> Barriers { get; } = new();

    public GameManager()
    {
        CurrentPlayer = new Player(300, 500);
        InitializeGame();
    }

    private void InitializeGame()
    {
        InitializePlayer();
        InitializeEnemyFormation();
        InitializeDefensiveBarriers();
        UpdateUI();
    }

    public void InitializePlayer()
    {
        CurrentPlayer = new Player(300, 500);
    }

    public void InitializeEnemyFormation()
    {
        Enemies.Clear();
        for (int row = 0; row < 5; row++)
        {
            HostileUnitType unitType;
            if (row == 0) unitType = HostileUnitType.Heavy;
            else if (row == 1 || row == 2) unitType = HostileUnitType.Standard;
            else unitType = HostileUnitType.Light;

            for (int col = 0; col < 3; col++)
            {
                var enemy = new Enemy(col * 70 + 300, row * 50 + 50, unitType);
                Enemies.Add(enemy);
            }
        }
    }

    public void InitializeDefensiveBarriers()
    {
        Barriers.Clear();
        int barrierVerticalPos = 400;
        int gapBarrier = 80;
        int[] barrierHorizontalPositions = { gapBarrier, gapBarrier * 2 + 100, gapBarrier * 3 + 200, gapBarrier * 4 + 300 };

        foreach (int barrierX in barrierHorizontalPositions)
        {
            Barriers.Add(new Wall(barrierX, barrierVerticalPos));
        }
    }

    public void MovePlayer(int direction) // -1 para esquerda, 1 para direita
    {
        if (IsGameOver) return;
        CurrentPlayer.MoveX(10 * direction);
    }

    public void PlayerShoot()
    {
        if (IsGameOver) return;
        if (Bullets.Count(b => b.Category == ShotCategory.Player) < 3)
        {
            var newProjectile = new Bullet(CurrentPlayer.PosX + CurrentPlayer.Width / 2, CurrentPlayer.PosY, 10, ShotCategory.Player);
            Bullets.Add(newProjectile);
        }
    }

    public void UpdateGame()
    {
        if (IsGameOver) return;

        ProcessBulletMovement();
        HandleEnemyFirePattern();
        ProcessCollisionDetection();

        if (Enemies.Count == 0)
        {
            // Lógica para próxima fase
            InitializeEnemyFormation();
        }
    }

    private void ProcessBulletMovement()
    {
        for (int i = Bullets.Count - 1; i >= 0; i--)
        {
            Bullets[i].UpdatePosition();
            if (Bullets[i].IsOffScreen(600))
            {
                Bullets.RemoveAt(i);
            }
        }
    }

    private void HandleEnemyFirePattern()
    {
        foreach (var enemy in Enemies)
        {
            var projectile = enemy.Shoot();
            if (projectile != null)
            {
                Bullets.Add(projectile);
            }
        }
    }

    private void ProcessCollisionDetection()
    {
        for (int i = Bullets.Count - 1; i >= 0; i--)
        {
            var bullet = Bullets[i];
            bool hit = false;

            foreach (var wall in Barriers)
            {
                if (!wall.IsDestroyed && wall.IsCollidingWith(bullet))
                {
                    wall.TakeDamage();
                    Bullets.RemoveAt(i);
                    hit = true;
                    break;
                }
            }
            if (hit) continue;

            if (bullet.Category == ShotCategory.Player)
            {
                for (int j = Enemies.Count - 1; j >= 0; j--)
                {
                    if (Enemies[j].IsCollidingWith(bullet))
                    {
                        CurrentPlayer.AddScore(Enemies[j].Points);
                        Enemies.RemoveAt(j);
                        Bullets.RemoveAt(i);
                        hit = true;
                        break;
                    }
                }
            }
            else if (bullet.Category == ShotCategory.Enemy)
            {
                if (bullet.IsCollidingWith(CurrentPlayer.PosX, CurrentPlayer.PosY, CurrentPlayer.Width, CurrentPlayer.Height))
                {
                    CurrentPlayer.LoseLife();
                    Bullets.RemoveAt(i);
                    if (CurrentPlayer.Lives <= 0)
                    {
                        IsGameOver = true;
                    }
                    hit = true;
                }
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        ScoreText = $"Score: {CurrentPlayer.Score}";
        LivesText = $"Lives: {CurrentPlayer.Lives}";
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}