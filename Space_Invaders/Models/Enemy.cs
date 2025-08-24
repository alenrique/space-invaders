using System;

namespace Space_Invaders.Models;

public enum HostileUnitType
{
    Light = 1,
    Standard = 2,
    Heavy = 3,
    Elite = 4
}

public class Enemy
{
    public HostileUnitType UnitType { get; set; }
    public double HorizontalPosition { get; set; }
    public double VerticalPosition { get; set; }
    public int ScoreValue { get; set; }
    public string SpriteAssetPath { get; set; }
    public int EntityWidth { get; set; } = 40;
    public int EntityHeight { get; set; } = 40;
    public int MoveDirection { get; set; } = 0; // 1 for right, -1 for left, 0 for no movement

    public DateTime PreviousFireTime { get; set; } = DateTime.MinValue;
    public TimeSpan FireInterval { get; set; }
    public double FireProbability { get; set; }

    public Enemy(int horizontalPos, int verticalPos, HostileUnitType unitType)
    {
        UnitType = unitType;
        HorizontalPosition = horizontalPos;
        VerticalPosition = verticalPos;
        ScoreValue = CalculateScoreByType(unitType);
        ConfigureCombatParameters();
        ConfigureAssetPath(); // Correção: Garante a inicialização
    }

    private void ConfigureCombatParameters()
    {
        switch (UnitType)
        {
            case HostileUnitType.Heavy:
                FireInterval = TimeSpan.FromSeconds(6.0);
                FireProbability = 0.002;
                break;
            case HostileUnitType.Elite:
                FireInterval = TimeSpan.FromSeconds(1.5);
                FireProbability = 0.003;
                break;
        }
    }

    public Bullet? ExecuteFire()
    {
        DateTime currentTime = DateTime.Now;
        if (currentTime - PreviousFireTime < FireInterval)
            return null;
        
        Random randomGenerator = new Random();
        if (randomGenerator.NextDouble() < FireProbability)
        {
            PreviousFireTime = DateTime.Now;
            int projectileX = (int)HorizontalPosition + EntityWidth / 2;
            int projectileY = (int)VerticalPosition + EntityHeight;
            int projectileVelocity = 5;
            return new Bullet(projectileX, projectileY, projectileVelocity, ShotCategory.Enemy);
        }
        return null;
    }

    public void ConfigureAssetPath()
    {
        string basePath = "ms-appx:///Assets/Images/";
        SpriteAssetPath = UnitType switch
        {
            HostileUnitType.Light => basePath + "enemy1.png",
            HostileUnitType.Standard => basePath + "enemy2.png",
            HostileUnitType.Heavy => basePath + "enemy3.png",
            HostileUnitType.Elite => basePath + "enemy4.png",
            _ => basePath + "enemy1.png",
        };
    }

    private static int CalculateScoreByType(HostileUnitType unitType) => unitType switch
    {
        HostileUnitType.Light => 10,
        HostileUnitType.Standard => 20,
        HostileUnitType.Heavy => 30,
        HostileUnitType.Elite => new Random().Next(50, 301),
        _ => 10
    };

    public bool DetectCollisionWith(Bullet projectile)
    {
        return projectile.IsCollidingWith((int)HorizontalPosition, (int)VerticalPosition, EntityWidth, EntityHeight);
    }

    // Propriedades e métodos de compatibilidade
    public double PosX { get => HorizontalPosition; set => HorizontalPosition = value; }
    public double PosY { get => VerticalPosition; set => VerticalPosition = value; }
    public int Points { get => ScoreValue; set => ScoreValue = value; }
    public string ImagePath { get => SpriteAssetPath; set => SpriteAssetPath = value; }
    public int Width { get => EntityWidth; set => EntityWidth = value; }
    public int Height { get => EntityHeight; set => EntityHeight = value; }
    public Bullet? Shoot() => ExecuteFire();
    public bool IsCollidingWith(Bullet bullet) => DetectCollisionWith(bullet);
}