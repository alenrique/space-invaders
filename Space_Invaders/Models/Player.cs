namespace Space_Invaders.Models;

public class Player
{
    public int CurrentScore { get; set; }
    public int RemainingLives { get; set; }
    public int HorizontalCoordinate { get; set; } // Coordenada X do jogador
    public int VerticalCoordinate { get; set; } // Coordenada Y do jogador
    
    public int PlayerWidth { get; set; } = 50; // Largura do sprite do jogador
    public int PlayerHeight { get; set; } = 50; // Altura do sprite
    
    public string AssetFilePath { get; set; } = "ms-appx:///Assets/Images/player.png"; // Localização do arquivo de imagem

    public Player(int initialX, int initialY)
    {
        this.HorizontalCoordinate = initialX; // Estabelece posição inicial X
        this.VerticalCoordinate = initialY; // Estabelece posição inicial Y
        CurrentScore = 0;
        RemainingLives = 3; // Quantidade inicial de vidas
    }

    public void IncreaseScore(int scorePoints)
    {
        CurrentScore += scorePoints;
    }

    public void GrantExtraLife()
    {
        if (RemainingLives < 6) // Limite superior de vidas
            RemainingLives++;
    }

    public void DecrementLife()
    {
        if (RemainingLives > 0)
            RemainingLives--;
    }

    public void UpdateHorizontalPosition(int movementDelta)
    {
        HorizontalCoordinate += movementDelta;
        // Restringir movimento para manter dentro da área de jogo
        if (HorizontalCoordinate < 0) HorizontalCoordinate = 0;
        if (HorizontalCoordinate > 550) HorizontalCoordinate = 550; // Considerando largura da tela
    }

    // Propriedades de compatibilidade para manter funcionamento
    public int Score 
    { 
        get => CurrentScore; 
        set => CurrentScore = value; 
    }
    
    public int Lives 
    { 
        get => RemainingLives; 
        set => RemainingLives = value; 
    }
    
    public int PosX 
    { 
        get => HorizontalCoordinate; 
        set => HorizontalCoordinate = value; 
    }
    
    public int PosY 
    { 
        get => VerticalCoordinate; 
        set => VerticalCoordinate = value; 
    }
    
    public int Width 
    { 
        get => PlayerWidth; 
        set => PlayerWidth = value; 
    }
    
    public int Height 
    { 
        get => PlayerHeight; 
        set => PlayerHeight = value; 
    }
    
    public string ImagePath 
    { 
        get => AssetFilePath; 
        set => AssetFilePath = value; 
    }

    // Métodos de compatibilidade
    public void AddScore(int points) => IncreaseScore(points);
    public void EarnLife() => GrantExtraLife();
    public void LoseLife() => DecrementLife();
    public void MoveX(int deltaX) => UpdateHorizontalPosition(deltaX);
}