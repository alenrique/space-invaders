using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Space_Invaders.Models;
using Space_Invaders.Utils;
using Windows.System;

namespace Space_Invaders;

public sealed partial class GamePage : Page
{
    public Player? currentPlayer;
    public Image? playerSprite;
    
    // Gerenciamento de projéteis
    private List<Bullet> activeBullets = new List<Bullet>();
    private List<Rectangle> bulletGraphics = new List<Rectangle>();
    
    // Gerenciamento de adversários
    private List<Enemy> hostileEntities = new List<Enemy>();
    private List<Image> enemySprites = new List<Image>();
    
    // Gerenciamento de obstáculos
    private List<Wall> barriers = new List<Wall>();
    private List<Rectangle> barrierGraphics = new List<Rectangle>();
    
    // Loop principal do jogo
    private DispatcherTimer mainGameLoop;
    
    // Sistema de áudio
    private SoundManager? audioSystem;

    public GamePage()
    {
        this.InitializeComponent();

        // Configuração inicial da página
        this.Loaded += OnPageLoaded;
        this.Unloaded += OnPageUnloaded;

        InitializePlayer();
        InitializeEnemyFormation();
        InitializeDefensiveBarriers();

        RefreshScoreDisplay();
        RefreshLifeDisplay();
        
        // Configurar loop do jogo
        SetupGameLoop();
    }

    private void SetupGameLoop()
    {
        mainGameLoop = new DispatcherTimer();
        mainGameLoop.Interval = TimeSpan.FromMilliseconds(16); // Aproximadamente 60 FPS
        mainGameLoop.Tick += OnGameLoopTick;
        mainGameLoop.Start();
    }

    private void OnGameLoopTick(object sender, object e)
    {
        ProcessBulletMovement();
        HandleEnemyFirePattern();
        ProcessCollisionDetection();
        RefreshBarrierAppearance();
    }

    private void HandleEnemyFirePattern()
    {
        // Sistema de disparo automático dos inimigos
        foreach (Enemy hostileUnit in hostileEntities)
        {
            Bullet? projectile = hostileUnit.Shoot();
            if (projectile != null)
            {
                activeBullets.Add(projectile);
                
                // Efeito sonoro para disparo inimigo
                audioSystem?.PlayEnemyShoot();
                
                // Renderização visual do projétil inimigo (cor vermelha)
                Rectangle projectileShape = new Rectangle
                {
                    Width = 4,
                    Height = 10,
                    Fill = new SolidColorBrush(Colors.White)
                };
                
                Canvas.SetLeft(projectileShape, projectile.PosX);
                Canvas.SetTop(projectileShape, projectile.PosY);
                
                GameCanvas.Children.Add(projectileShape);
                bulletGraphics.Add(projectileShape);
            }
        }
    }

    private void ProcessBulletMovement()
    {
        // Atualização da física dos projéteis
        for (int idx = activeBullets.Count - 1; idx >= 0; idx--)
        {
            activeBullets[idx].UpdatePosition();
            
            // Sincronização visual
            Canvas.SetTop(bulletGraphics[idx], activeBullets[idx].PosY);
            
            // Limpeza de projéteis fora dos limites
            if (activeBullets[idx].IsOffScreen(600)) // Limite vertical da tela
            {
                GameCanvas.Children.Remove(bulletGraphics[idx]);
                activeBullets.RemoveAt(idx);
                bulletGraphics.RemoveAt(idx);
            }
        }
    }

    private void ProcessCollisionDetection()
    {
        // Sistema de detecção de colisões
        for (int projIndex = activeBullets.Count - 1; projIndex >= 0; projIndex--)
        {
            Bullet projectile = activeBullets[projIndex];
            bool hasCollided = false;
            
            if (projectile.Category == ShotCategory.Player)
            {
                // Projétil do jogador contra inimigos
                hasCollided = ProcessPlayerProjectileHits(projectile, projIndex);
            }
            else
            {
                // Projétil inimigo contra jogador
                hasCollided = ProcessEnemyProjectileHits(projectile, projIndex);
            }
            
            // Verificar impacto em barreiras se não houve outras colisões
            if (!hasCollided)
            {
                ProcessBarrierCollisions(projectile, projIndex);
            }
        }
        
        // Verificação de vitória por eliminação total
        if (hostileEntities.Count == 0)
        {
            // Lógica para próxima fase pode ser implementada aqui
            // Exemplo: InitializeEnemyFormation(); para nova wave
        }
    }

    private bool ProcessPlayerProjectileHits(Bullet projectile, int projIndex)
    {
        // Verificação de acertos em adversários
        for (int enemyIdx = hostileEntities.Count - 1; enemyIdx >= 0; enemyIdx--)
        {
            Enemy target = hostileEntities[enemyIdx];
            
            if (target.IsCollidingWith(projectile))
            {
                // Sistema de pontuação
                if (currentPlayer != null)
                {
                    currentPlayer.AddScore(target.Points);
                    RefreshScoreDisplay();
                }
                
                // Efeito sonoro de destruição
                audioSystem?.PlayExplosion();
                
                // Remoção do projétil
                DestroyBullet(projIndex);
                
                // Remoção do inimigo
                GameCanvas.Children.Remove(enemySprites[enemyIdx]);
                hostileEntities.RemoveAt(enemyIdx);
                enemySprites.RemoveAt(enemyIdx);
                
                return true; // Confirmação de acerto
            }
        }
        return false;
    }

    private bool ProcessEnemyProjectileHits(Bullet projectile, int projIndex)
    {
        // Verificação de dano ao jogador
        if (currentPlayer != null && projectile.IsCollidingWith(currentPlayer.PosX, currentPlayer.PosY, currentPlayer.Width, currentPlayer.Height))
        {
            // Redução de vida do jogador
            currentPlayer.LoseLife();
            RefreshLifeDisplay();
            
            // Efeito sonoro de dano
            audioSystem?.PlayHit();
            
            // Remoção do projétil
            DestroyBullet(projIndex);
            
            // Verificação de fim de jogo
            if (currentPlayer.Lives <= 0)
            {
                // Parada do jogo - Game Over
                mainGameLoop.Stop();
                // Interface de game over pode ser implementada aqui
            }
            
            return true; // Confirmação de acerto
        }
        return false;
    }

    private void ProcessBarrierCollisions(Bullet projectile, int projIndex)
    {
        // Sistema de dano em barreiras defensivas
        for (int barrierIdx = barriers.Count - 1; barrierIdx >= 0; barrierIdx--)
        {
            Wall defensiveWall = barriers[barrierIdx];
            
            if (!defensiveWall.IsDestroyed && defensiveWall.IsCollidingWith(projectile))
            {
                // Aplicação de dano estrutural
                bool isCompletelyDestroyed = defensiveWall.TakeDamage();
                
                // Efeito sonoro de impacto
                audioSystem?.PlayHit();
                
                // Remoção do projétil
                DestroyBullet(projIndex);
                
                // Limpeza de barreira destruída
                if (isCompletelyDestroyed)
                {
                    GameCanvas.Children.Remove(barrierGraphics[barrierIdx]);
                    barriers.RemoveAt(barrierIdx);
                    barrierGraphics.RemoveAt(barrierIdx);
                }
                
                break; // Um projétil só pode atingir uma barreira
            }
        }
    }

    private void DestroyBullet(int bulletIndex)
    {
        if (bulletIndex >= 0 && bulletIndex < activeBullets.Count)
        {
            GameCanvas.Children.Remove(bulletGraphics[bulletIndex]);
            activeBullets.RemoveAt(bulletIndex);
            bulletGraphics.RemoveAt(bulletIndex);
        }
    }

    private void RefreshBarrierAppearance()
    {
        // Atualização visual baseada na integridade estrutural
        for (int i = 0; i < barriers.Count; i++)
        {
            if (!barriers[i].IsDestroyed)
            {
                barrierGraphics[i].Fill = barriers[i].GetWallColor();
            }
        }
    }

    private void InitializeDefensiveBarriers()
    {
        // Construção de estruturas defensivas
        int barrierVerticalPos = 400; // Altura das barreiras
        int[] barrierHorizontalPositions = { 100 }; // Posições horizontais
        
        foreach (int barrierX in barrierHorizontalPositions)
        {
            Wall defensiveStructure = new Wall(barrierX, barrierVerticalPos);
            barriers.Add(defensiveStructure);
            
            Rectangle barrierVisual = new Rectangle
            {
                Width = defensiveStructure.Width,
                Height = defensiveStructure.Height,
                Fill = defensiveStructure.GetWallColor()
            };
            
            Canvas.SetLeft(barrierVisual, defensiveStructure.PosX);
            Canvas.SetTop(barrierVisual, defensiveStructure.PosY);
            
            GameCanvas.Children.Add(barrierVisual);
            barrierGraphics.Add(barrierVisual);
        }
    }

    private void FireProjectile()
    {
        if (currentPlayer != null)
        {
            // Efeito sonoro de disparo
            audioSystem?.PlayPlayerShoot();
            
            // Criação de novo projétil na posição do jogador
            Bullet newProjectile = new Bullet(currentPlayer.PosX + currentPlayer.Width / 2, currentPlayer.PosY, 10, ShotCategory.Player);
            activeBullets.Add(newProjectile);
            
            // Renderização visual do projétil do jogador (amarelo)
            Rectangle projectileVisual = new Rectangle
            {
                Width = 4,
                Height = 10,
                Fill = new SolidColorBrush(Colors.White)
            };
            
            Canvas.SetLeft(projectileVisual, newProjectile.PosX);
            Canvas.SetTop(projectileVisual, newProjectile.PosY);
            
            GameCanvas.Children.Add(projectileVisual);
            bulletGraphics.Add(projectileVisual);
        }
    }

    private void RefreshScoreDisplay()
    {
        if (currentPlayer != null)
            ScoreText.Text = $"Score: {currentPlayer.Score}";
    }

    private void RefreshLifeDisplay()
    {
        if (currentPlayer != null)
            LifesText.Text = $"Lifes: {currentPlayer.Lives}";
    }

    public void InitializeEnemyFormation()
    {
        // Limpeza de formações anteriores
        hostileEntities.Clear();
        enemySprites.Clear();

        for (int row = 0; row < 5; row++)
        {
            HostileUnitType unitType;

            if (row == 0)
                unitType = HostileUnitType.Heavy;   // Primeira fileira - unidades pesadas
            else if (row == 1 || row == 2)
                unitType = HostileUnitType.Standard;   // Segunda e terceira fileiras - unidades médias
            else
                unitType = HostileUnitType.Light;   // Quarta e quinta fileiras - unidades leves

            for (int col = 0; col < 3; col++)
            {
                Enemy hostileUnit = new Enemy(col * 70 + 300, row * 50 + 50, unitType);
                hostileUnit.SetImagePath();
                hostileEntities.Add(hostileUnit);

                Image enemyVisual = new Image
                {
                    Width = 50,
                    Height = 50,
                    Source = new BitmapImage(new Uri(hostileUnit.ImagePath))
                };

                Canvas.SetLeft(enemyVisual, hostileUnit.PosX);
                Canvas.SetTop(enemyVisual, hostileUnit.PosY);

                GameCanvas.Children.Add(enemyVisual);
                enemySprites.Add(enemyVisual);
            }
        }
    }

    public void InitializePlayer()
    {
        currentPlayer = new Player(300, 500); // Coordenadas iniciais do jogador

        playerSprite = new Image
        {
            Width = currentPlayer.Width,
            Height = currentPlayer.Height,
            Source = new BitmapImage(new Uri(currentPlayer.ImagePath))
        };

        Canvas.SetLeft(playerSprite, currentPlayer.PosX);
        Canvas.SetTop(playerSprite, currentPlayer.PosY);

        GameCanvas.Children.Add(playerSprite);
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Configuração de foco para captura de entrada
        MainScrollViewer.Focus(FocusState.Programmatic);

        // Vinculação de eventos de teclado
        MainScrollViewer.KeyDown += HandleKeyboardInput;

        // Posicionamento de elementos de interface
        Canvas.SetLeft(ScoreText, 10);
        Canvas.SetTop(ScoreText, 10);

        Canvas.SetLeft(LifesText, 500);
        Canvas.SetTop(LifesText, 10);
        
        // Inicialização do sistema de áudio
        audioSystem = new SoundManager();
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        // Finalização do loop principal
        mainGameLoop?.Stop();
        
        // Liberação de recursos de áudio
        audioSystem?.Dispose();
    }

    public void HandleKeyboardInput(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Left:
                if (currentPlayer != null)
                    currentPlayer.MoveX(-10); // Movimento para esquerda
                break;
            case VirtualKey.Right:
                if (currentPlayer != null)
                    currentPlayer.MoveX(10); // Movimento para direita
                break;
            case VirtualKey.Space:
                FireProjectile(); // Ativação de disparo
                break;
        }

        // Sincronização visual da posição do jogador
        if (currentPlayer != null)
            Canvas.SetLeft(playerSprite, currentPlayer.PosX);
    }
}