namespace Space_Invaders.Models;

public enum HostileUnitType
{
    Light = 1,     // 10 pontos
    Standard = 2,  // 20 pontos
    Heavy = 3,     // 30 pontos
    Elite = 4      // 50-300 pontos (variável)
}

public class Enemy
{
    public HostileUnitType UnitType { get; set; } // Classificação da unidade hostil
    public int HorizontalPosition { get; set; } // Coordenada X da entidade
    public int VerticalPosition { get; set; } // Coordenada Y da entidade
    public int ScoreValue { get; set; } // Valor em pontos da unidade
    public string SpriteAssetPath { get; set; } // Localização do arquivo de imagem
    public int EntityWidth { get; set; } = 50; // Dimensão horizontal
    public int EntityHeight { get; set; } = 50; // Dimensão vertical
    
    // Mecânica de combate das unidades inimigas
    public DateTime PreviousFireTime { get; set; } = DateTime.MinValue; // Registro do último disparo
    public TimeSpan FireInterval { get; set; } // Intervalo entre disparos consecutivos
    public double FireProbability { get; set; } // Probabilidade de disparo por ciclo (0.0 a 1.0)

    public Enemy(int horizontalPos, int verticalPos, HostileUnitType unitType)
    {
        UnitType = unitType; // Estabelece a classificação da unidade
        HorizontalPosition = horizontalPos; // Define posição inicial X
        VerticalPosition = verticalPos; // Define posição inicial Y
        ScoreValue = CalculateScoreByType(unitType); // Determina pontuação baseada no tipo
        
        // Estabelecer parâmetros de combate conforme classificação
        ConfigureCombatParameters();
    }

    private void ConfigureCombatParameters()
    {
        switch (UnitType)
        {
            case HostileUnitType.Heavy:
                FireInterval = TimeSpan.FromSeconds(6.0); // Dispara a cada 6 segundos
                FireProbability = 0.002; // 0.2% de probabilidade por frame
                break;
            case HostileUnitType.Elite:
                FireInterval = TimeSpan.FromSeconds(1.5); // Dispara a cada 1.5 segundos
                FireProbability = 0.003; // 0.3% de probabilidade por frame
                break;
        }
    }

    public void AdjustHorizontalPosition(int displacement)
    {
        HorizontalPosition += displacement;
        // Manter dentro dos limites horizontais da área de jogo
        if (HorizontalPosition < 0) HorizontalPosition = 0;
        if (HorizontalPosition > 800) HorizontalPosition = 800; // Considerando largura da tela de 800 pixels
    }

    public void AdjustVerticalPosition(int displacement)
    {
        VerticalPosition += displacement;
        // Manter dentro dos limites verticais da área de jogo
        if (VerticalPosition < 0) VerticalPosition = 0;
        if (VerticalPosition > 600) VerticalPosition = 600; // Considerando altura da tela de 600 pixels
    }

    // Verificação de capacidade de disparar
    public bool IsAbleToFire()
    {
        DateTime currentTime = DateTime.Now;
        
        // Validar se o intervalo de recarga foi respeitado
        if (currentTime - PreviousFireTime < FireInterval)
            return false;
            
        // Avaliar chance probabilística de disparo
        Random randomGenerator = new Random();
        return randomGenerator.NextDouble() < FireProbability;
    }

    // Execução de disparo pela unidade inimiga
    public Bullet? ExecuteFire()
    {
        if (!IsAbleToFire())
            return null;
            
        PreviousFireTime = DateTime.Now;
        
        // Gerar projétil na posição da unidade (centro da base)
        int projectileX = HorizontalPosition + EntityWidth / 2;
        int projectileY = VerticalPosition + EntityHeight;
        int projectileVelocity = 5; // Velocidade do projétil hostil
        
        return new Bullet(projectileX, projectileY, projectileVelocity, ShotCategory.Enemy);
    }

    public void ConfigureAssetPath()
    {
        string basePath = "ms-appx:///Assets/Images/";
        switch (UnitType)
        {
            case HostileUnitType.Light:
                SpriteAssetPath = basePath + "enemy1.png";
                break;
            case HostileUnitType.Standard:
                SpriteAssetPath = basePath + "enemy2.png";
                break;
            case HostileUnitType.Heavy:
                SpriteAssetPath = basePath + "enemy3.png";
                break;
            case HostileUnitType.Elite:
                SpriteAssetPath = basePath + "enemy4.png";
                break;
            default:
                SpriteAssetPath = basePath + "enemy.png";
                break;
        }
    }

    private static int CalculateScoreByType(HostileUnitType unitType)
    {
        return unitType switch
        {
            HostileUnitType.Light => 10,
            HostileUnitType.Standard => 20,
            HostileUnitType.Heavy => 30,
            HostileUnitType.Elite => new Random().Next(50, 301), // Pontuação aleatória entre 50-300
            _ => 10
        };
    }

    // Verificação de colisão com projétil
    public bool DetectCollisionWith(Bullet projectile)
    {
        return projectile.IsCollidingWith(HorizontalPosition, VerticalPosition, EntityWidth, EntityHeight);
    }

    // Propriedades de compatibilidade para manter funcionamento
    public int PosX 
    { 
        get => HorizontalPosition; 
        set => HorizontalPosition = value; 
    }
    
    public int PosY 
    { 
        get => VerticalPosition; 
        set => VerticalPosition = value; 
    }
    
    public int Points 
    { 
        get => ScoreValue; 
        set => ScoreValue = value; 
    }
    
    public string ImagePath 
    { 
        get => SpriteAssetPath; 
        set => SpriteAssetPath = value; 
    }
    
    public int Width 
    { 
        get => EntityWidth; 
        set => EntityWidth = value; 
    }
    
    public int Height 
    { 
        get => EntityHeight; 
        set => EntityHeight = value; 
    }
    
    public HostileUnitType Type 
    { 
        get => (HostileUnitType)UnitType; 
        set => UnitType = (HostileUnitType)value; 
    }

    // Métodos de compatibilidade
    public void MoveX(int deltaX) => AdjustHorizontalPosition(deltaX);
    public void MoveY(int deltaY) => AdjustVerticalPosition(deltaY);
    public void SetImagePath() => ConfigureAssetPath();
    public Bullet? Shoot() => ExecuteFire();
    public bool IsCollidingWith(Bullet bullet) => DetectCollisionWith(bullet);
}