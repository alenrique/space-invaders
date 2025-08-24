using System.ComponentModel;
using System.Collections.ObjectModel;
using Space_Invaders.Models;
using Microsoft.UI;
using System.Linq;
using Space_Invaders.Utils;
using System.IO;
using System.Threading.Tasks;

namespace Space_Invaders.Managers;

public class GameManager : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Action<int>? GameOver; // Evento para notificar o GamePage sobre o fim do jogo

    public double GameWidth { get; set; } = 1000;
    public double GameHeight { get; set; } = 600;

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
                if (_isGameOver)
                {
                    GameOver?.Invoke(CurrentPlayer.Score); // Invoca o evento GameOver
                }
            }
        }
    }

    // Coleções para gerenciar as entidades do jogo
    public Player CurrentPlayer { get; private set; }
    public ObservableCollection<Enemy> Enemies { get; } = new();
    public ObservableCollection<Bullet> Bullets { get; } = new();
    public ObservableCollection<Wall> Barriers { get; } = new();

    private SoundManager _soundManager;

    private int _enemyMoveDirection = 1; // 1 for right, -1 for left
    private double _enemySpeed = 1.0;
    private double _enemyDropAmount = 20.0;
    private double _eliteEnemySpeed = 3.0; // Speed for Elite enemy

    private Random _random = new Random(); // New Random instance

    public GameManager()
    {
        CurrentPlayer = new Player(300, 500);
        _soundManager = new SoundManager();
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
        CurrentPlayer = new Player((int)(GameWidth / 2 - 25), (int)(GameHeight - 70));
    }

    public void InitializeEnemyFormation()
    {
        Enemies.Clear();
        // Calculate starting X to center the enemy formation
        // Assuming each enemy is 50 wide and there are 3 columns, total width is 11 * 70 = 770
        double formationWidth = 11 * 70; 
        double startX = (GameWidth - formationWidth) / 2;

        for (int row = 0; row < 5; row++)
        {
            HostileUnitType unitType;
            if (row == 0) unitType = HostileUnitType.Heavy;
            else if (row == 1 || row == 2) unitType = HostileUnitType.Standard;
            else unitType = HostileUnitType.Light;

            for (int col = 0; col < 11; col++)
            {
                var enemy = new Enemy((int)(startX + col * 50), row * 45 + 80, unitType);
                Enemies.Add(enemy);
            }
        }
    }

    public void InitializeDefensiveBarriers()
    {
        Barriers.Clear();
        // Calculate vertical position relative to GameHeight
        double barrierVerticalPos = GameHeight - 200; // Example: 200 units from the bottom

        // Calculate horizontal positions to distribute barriers evenly
        int numberOfBarriers = 4;
        double barrierWidth = 81; // Assuming a fixed width for each barrier
        double totalBarriersWidth = numberOfBarriers * barrierWidth;
        double spacing = (GameWidth - totalBarriersWidth) / (numberOfBarriers + 1);

        for (int i = 0; i < numberOfBarriers; i++)
        {
            double barrierX = spacing * (i + 1) + i * barrierWidth;
            Barriers.Add(new Wall((int)barrierX, (int)barrierVerticalPos));
        }
    }

    public void MovePlayer(int direction) // -1 para esquerda, 1 para direita
    {
        if (IsGameOver) return;
        double newPosX = CurrentPlayer.PosX + (10 * direction);

        // Boundary check for player movement
        if (newPosX >= 0 && newPosX <= GameWidth - CurrentPlayer.Width)
        {
            CurrentPlayer.MoveX(10 * direction);
        }
    }

    public void PlayerShoot()
    {
        if (IsGameOver) return;
        if (Bullets.Count(b => b.Category == ShotCategory.Player) < 1)
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
        UpdateEnemyMovement(); // Added this line
        UpdateEliteEnemyMovement(); // Update Elite enemy movement
        SpawnEliteEnemy(); // Attempt to spawn Elite enemy

        if (Enemies.Count == 0)
        {
            // Lógica para próxima fase
            InitializeEnemyFormation();
            CurrentPlayer.Lives += 1; // Bônus de vida
        }
        CleanUpDestroyedBarriers(); // Call the cleanup method
    }

    private void SpawnEliteEnemy()
    {
        // Only one Elite enemy at a time
        if (Enemies.Any(b => b.UnitType == HostileUnitType.Elite)) return;

        // 1 in 500 chance to spawn an Elite enemy per update cycle
        if (_random.Next(0, 500) == 0) 
        {
            int startX;
            int direction; // 1 for right, -1 for left
            if (_random.Next(0, 2) == 0) // Randomly choose left or right side
            {
                // Spawn from left, move right
                startX = -50; // Just off-screen to the left
                direction = 1;
            }
            else
            {
                // Spawn from right, move left
                startX = (int)GameWidth + 50; // Just off-screen to the right
                direction = -1;
            }

            var eliteEnemy = new Enemy(startX, 50, HostileUnitType.Elite); // Fixed Y position near top
            eliteEnemy.MoveDirection = direction; // Set the movement direction
            eliteEnemy.PosX = startX; // Set initial position
            eliteEnemy.PosY = 40; // Set initial Y position
            eliteEnemy.Width = 50; // Elite enemy might be larger
            eliteEnemy.Height = 40;
            eliteEnemy.ScoreValue = _random.Next(50, 301); // Random score for Elite enemy
            eliteEnemy.FireInterval = TimeSpan.FromSeconds(1.5);
            eliteEnemy.FireProbability = 0.003;
            eliteEnemy.ConfigureAssetPath(); // Ensure asset path is set

            // Store direction in a way that can be accessed during movement
            // For simplicity, let's add a custom property or use a dictionary if Enemy class cannot be modified
            // For now, we'll assume we can add a property to Enemy or pass it around.
            // Let's add a temporary property to Enemy for movement direction.
            // This will require modifying the Enemy class.
            // For now, I'll store it in a tuple or a custom class if Enemy cannot be modified.
            // Let's assume for now that we can add a property to Enemy.
            // If not, I'll need to create a wrapper class for EliteEnemy.

            // For now, let's just add it and assume we'll handle its movement separately.
            Enemies.Add(eliteEnemy);
        }
    }

    private void UpdateEliteEnemyMovement()
    {
        if (!Enemies.Any(b => b.UnitType == HostileUnitType.Elite)) return;
        var eliteEnemy = Enemies.First(b => b.UnitType == HostileUnitType.Elite);
        eliteEnemy.PosX += _eliteEnemySpeed * eliteEnemy.MoveDirection;

        // Remove if off-screen
        if ((eliteEnemy.MoveDirection == 1 && eliteEnemy.PosX > GameWidth) || (eliteEnemy.MoveDirection == -1 && eliteEnemy.PosX + eliteEnemy.Width < 0))
        {
            Enemies.RemoveAt(Enemies.IndexOf(eliteEnemy));
        }
    }

    private void CleanUpDestroyedBarriers()
    {
        for (int i = Barriers.Count - 1; i >= 0; i--)
        {
            if (Barriers[i].IsDestroyed)
            {
                Barriers.RemoveAt(i);
            }
        }
    }

    private void UpdateEnemyMovement() // New method
    {
        if (!Enemies.Any()) return;

        bool hitEdge = false;
        foreach (var enemy in Enemies)
        {
            if (enemy.UnitType == HostileUnitType.Elite) continue; // Skip Elite enemy here
            enemy.PosX += _enemySpeed * _enemyMoveDirection;

            // Check for boundary collision
            if (_enemyMoveDirection == 1 && enemy.PosX + enemy.Width > GameWidth)
            {
                hitEdge = true;
            }
            else if (_enemyMoveDirection == -1 && enemy.PosX < 0)
            {
                hitEdge = true;
            }
        }

        if (hitEdge)
        {
            _enemyMoveDirection *= -1; // Reverse direction
            foreach (var enemy in Enemies)
            {
                if (enemy.UnitType == HostileUnitType.Elite) continue; // Skip Elite enemy here
                enemy.PosY += _enemyDropAmount; // Move down
                _enemySpeed += 0.003; // Increase speed slightly
            }

            // Check if any enemy has reached the bottom
            if (Enemies.Any(enemy => enemy.PosY + enemy.Height >= GameHeight - 150))
            {
                IsGameOver = true;
            }
        }
    }

    private void ProcessBulletMovement()
    {
        for (int i = Bullets.Count - 1; i >= 0; i--)
        {
            Bullets[i].UpdatePosition();
            if (Bullets[i].IsOffScreen((int)GameHeight))
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
                    _soundManager.PlayExplosion();
                    Bullets.RemoveAt(i);
                    hit = true;
                    break;
                }
            }
            if (hit) continue;

            if (bullet.Category == ShotCategory.Player)
            {
                // Collision with regular enemies
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
                if (hit) continue;
            }
            else if (bullet.Category == ShotCategory.Enemy)
            {
                if (bullet.IsCollidingWith(CurrentPlayer.PosX, CurrentPlayer.PosY, CurrentPlayer.Width, CurrentPlayer.Height))
                {
                    CurrentPlayer.LoseLife();
                    _soundManager.PlayExplosion();
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

    public async Task SaveScore(string playerName, int score)
    {
        string scoresFilePath = "score.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(scoresFilePath, append: true))
            {
                string scoreEntry = $"{DateTime.Now}: {playerName} - {score} points";
                await writer.WriteLineAsync(scoreEntry);
            }
            Console.WriteLine($"Score saved to {scoresFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving score: {ex.Message}");
        }
    }

    public async Task<List<ScoreEntry>> LoadScores()
    {
        List<ScoreEntry> scores = new List<ScoreEntry>();
        string scoresFilePath = "score.txt";

        if (!File.Exists(scoresFilePath))
        {
            return scores; // Return empty list if file doesn't exist
        }

        try
        {
            using (StreamReader reader = new StreamReader(scoresFilePath))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // Parse the line: "DateTime: PlayerName - Score points"
                    // Example: "8/24/2025 10:30:00 AM: Player1 - 1234 points"
                    try
                    {
                        int pointsKeywordIndex = line.IndexOf(" points");
                        if (pointsKeywordIndex == -1) continue; // Skip if " points" not found

                        string lineWithoutPoints = line.Substring(0, pointsKeywordIndex).Trim(); // "24/08/2025 19:04:28: Joanna - 200"

                        int lastDashIndex = lineWithoutPoints.LastIndexOf(" - ");
                        if (lastDashIndex == -1) continue; // Skip if " - " not found

                        string scoreString = lineWithoutPoints.Substring(lastDashIndex + 3).Trim(); // "200"
                        string dateTimeAndNamePart = lineWithoutPoints.Substring(0, lastDashIndex).Trim(); // "24/08/2025 19:04:28: Joanna"

                        int lastColonIndex = dateTimeAndNamePart.LastIndexOf(':');
                        // This is tricky. If the time format is HH:mm:ss, there will be colons in the time.
                        // We need the colon that separates the date/time from the name.
                        // Let's assume the date format is consistent and the last colon before the name is the separator.
                        // If the date format is "dd/MM/yyyy HH:mm:ss", then the last colon of the time is the one we want.
                        // The example "24/08/2025 19:04:28: Joanna" shows the colon after "28".
                        // So, `LastIndexOf(':')` should work here.

                        if (lastColonIndex == -1) continue; // Skip if no colon found

                        string datePart = dateTimeAndNamePart.Substring(0, lastColonIndex).Trim();
                        string playerName = dateTimeAndNamePart.Substring(lastColonIndex + 1).Trim();

                        if (DateTime.TryParse(datePart, out DateTime date) && int.TryParse(scoreString, out int score))
                        {
                            Console.WriteLine($"Parsed Entry - Date: {date}, Player: {playerName}, Score: {score}");
                            scores.Add(new ScoreEntry { Date = date, PlayerName = playerName, Score = score });
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse Date or Score: Date='{datePart}', Score='{scoreString}'");
                        }
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"Error parsing score line: {line} - {parseEx.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading scores: {ex.Message}");
        }

        // Sort scores in descending order
        Console.WriteLine($"Number of scores loaded: {scores.Count}");
        foreach (var score in scores)
        {
            Console.WriteLine($"{score.Date}: {score.PlayerName} - {score.Score} points");
        }
        return scores.OrderByDescending(s => s.Score).ToList();
    }
}