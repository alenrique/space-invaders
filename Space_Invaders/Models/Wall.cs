using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System.ComponentModel; // Added for INotifyPropertyChanged

namespace Space_Invaders.Models;

public class Wall : INotifyPropertyChanged // Implemented INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int PosX { get; set; } // Coordenada X da parede
    public int PosY { get; set; } // Coordenada Y da parede
    private int _lives = 6; // Quantidade atual de vidas
    public int Lives
    {
        get => _lives;
        set
        {
            if (_lives != value)
            {
                _lives = value;
                OnPropertyChanged(nameof(Lives));
                OnPropertyChanged(nameof(WallColor)); // Notify UI when Lives changes
                OnPropertyChanged(nameof(IsDestroyed)); // Notify UI when IsDestroyed changes
            }
        }
    }
    public int MaxLives { get; set; } = 6; // Total de vidas possíveis
    public int Width { get; set; } = 81; // Tamanho horizontal
    public int Height { get; set; } = 60; // Tamanho vertical
    public bool IsDestroyed => Lives <= 0; // Verifica se a parede foi eliminada

    public SolidColorBrush WallColor => GetWallColor(); // New property for UI binding

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
    private SolidColorBrush GetWallColor() // Changed to private as it's now accessed via WallColor property
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

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
