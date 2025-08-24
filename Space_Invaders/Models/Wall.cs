using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Space_Invaders.Models;

public class Wall
{
    public int PosX { get; set; } // Coordenada X da parede
    public int PosY { get; set; } // Coordenada Y da parede
    public int Lives { get; set; } = 6; // Quantidade atual de vidas
    public int MaxLives { get; set; } = 6; // Total de vidas possíveis
    public int Width { get; set; } = 100; // Tamanho horizontal
    public int Height { get; set; } = 50; // Tamanho vertical
    public bool IsDestroyed => Lives <= 0; // Verifica se a parede foi eliminada

    public Wall(int posX, int posY)
    {
        PosX = posX;
        PosY = posY;
    }

    public void MoveX(int delta)
    {
        PosX += delta;

        // Impede que a parede ultrapasse os limites horizontais
        if (PosX < 0)
            PosX = 0;
        else if (PosX > 800 - Width)
            PosX = 800 - Width;
    }

    // Reduz a vida da parede e retorna verdadeiro se ela for destruída
    public bool TakeDamage(int dmg = 1)
    {
        Lives -= dmg;
        if (Lives < 0)
            Lives = 0;

        return IsDestroyed;
    }

    // Retorna a cor visual da parede com base na proporção de vida restante
    public SolidColorBrush GetWallColor()
    {
        float ratio = (float)Lives / MaxLives;
        byte alphaChannel;

        if (ratio > 0.75f)
            alphaChannel = 255; // Mais opaco
        else if (ratio > 0.5f)
            alphaChannel = 192;
        else if (ratio > 0.25f)
            alphaChannel = 128;
        else if (ratio > 0)
            alphaChannel = 64;
        else
            alphaChannel = 0; // Invisível

        return new SolidColorBrush(Color.FromArgb(alphaChannel, 255, 255, 255));
    }

    // Detecta colisão com uma bala
    public bool IsCollidingWith(Bullet bullet)
    {
        return bullet.IsCollidingWith(PosX, PosY, Width, Height);
    }
}
