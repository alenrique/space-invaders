namespace Space_Invaders.Models;

public enum ShotCategory
{
    Player,   // Disparo do jogador (movimenta para cima)
    Enemy     // Disparo do inimigo (movimenta para baixo)
}

public class Bullet
{
    public int PosX { get; set; } // Coordenada X
    public int PosY { get; set; } // Coordenada Y
    public int Velocity { get; set; } // Velocidade do disparo
    public int SizeX { get; set; } = 4; // Largura
    public int SizeY { get; set; } = 10; // Altura
    public ShotCategory Category { get; set; } // Tipo (herói ou inimigo)

    public Bullet(int x, int y, int velocity, ShotCategory category)
    {
        PosX = x;
        PosY = y;
        Velocity = velocity;
        Category = category;
    }

    public void UpdatePosition()
    {
        if (Category == ShotCategory.Player)
            PosY -= Velocity; // Vai para cima
        else
            PosY += Velocity; // Vai para baixo
    }

    public bool IsOffScreen(int screenHeight)
    {
        return PosY < 0 || PosY > screenHeight;
    }

    public bool IsCollidingWith(int targetX, int targetY, int targetW, int targetH)
    {
        return PosX >= targetX &&
               PosX <= targetX + targetW &&
               PosY >= targetY &&
               PosY <= targetY + targetH;
    }
}
