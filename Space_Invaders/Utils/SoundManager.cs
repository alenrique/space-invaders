using System.Diagnostics;

namespace Space_Invaders.Utils;

public class SoundManager
{
    private string? _shootPath;
    private string? _explosionPath;
    private string? _hitPath;
    private bool _muted = false;
    private bool _audioReady = false;

    public SoundManager()
    {
        SetupAudio();
    }

    private void SetupAudio()
    {
        try
        {
            Debug.WriteLine("Configurando áudio para ambiente Linux...");

            var root = AppDomain.CurrentDomain.BaseDirectory;
            var soundsDir = Path.Combine(root, "Assets", "Sounds");

            _shootPath = Path.Combine(soundsDir, "shoot.wav");
            _explosionPath = Path.Combine(soundsDir, "explosion.wav");
            _hitPath = Path.Combine(soundsDir, "hit.wav");

            Debug.WriteLine($"Diretório dos sons: {soundsDir}");

            ValidateSoundFiles();

            _audioReady = true;
            Debug.WriteLine("Áudio configurado com sucesso!");
        }
        catch (Exception err)
        {
            Debug.WriteLine($"Falha ao configurar áudio: {err.Message}");
            _audioReady = false;
        }
    }

    private void ValidateSoundFiles()
    {
        var files = new Dictionary<string, string?>
        {
            { "shoot.wav", _shootPath },
            { "explosion.wav", _explosionPath },
            { "hit.wav", _hitPath }
        };

        foreach (var entry in files)
        {
            if (!string.IsNullOrWhiteSpace(entry.Value) && File.Exists(entry.Value))
                Debug.WriteLine($"Som localizado: {entry.Key}");
            else
                Debug.WriteLine($"Arquivo ausente: {entry.Key} em {entry.Value}");
        }

        try
        {
            var assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            if (Directory.Exists(assets))
            {
                Debug.WriteLine("Conteúdo da pasta Assets:");
                foreach (var item in Directory.GetFiles(assets, "*", SearchOption.AllDirectories))
                {
                    var relPath = Path.GetRelativePath(assets, item);
                    Debug.WriteLine($" - {relPath}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Erro ao listar diretório: {e.Message}");
        }
    }

    public void PlayPlayerShoot()
    {
        if (_muted)
        {
            Debug.WriteLine("Disparo do jogador (mutado)");
            return;
        }

        Debug.WriteLine("Jogador disparou");

        if (_audioReady)
            _ = Task.Run(() => Reproduce(_shootPath, "tiro jogador"));
    }

    public void PlayEnemyShoot()
    {
        if (_muted)
        {
            Debug.WriteLine("Disparo do inimigo (mutado)");
            return;
        }

        Debug.WriteLine("Inimigo disparou");

        if (_audioReady)
            _ = Task.Run(() => Reproduce(_shootPath, "tiro inimigo"));
    }

    public void PlayExplosion()
    {
        if (_muted) return;

        Debug.WriteLine("Explosão detectada!");

        if (_audioReady)
            _ = Task.Run(() => Reproduce(_explosionPath, "explosão"));
    }

    public void PlayHit()
    {
        if (_muted) return;

        Debug.WriteLine("Acerto!");

        if (_audioReady)
            _ = Task.Run(() => Reproduce(_hitPath, "impacto"));
    }

    private async Task Reproduce(string? file, string label)
    {
        try
        {
            Debug.WriteLine($"Tentando tocar: {label}");

            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                if (await UseAplay(file))
                    return;
            }

            Debug.WriteLine($"Som de {label} indisponível, tentando beep...");
            TriggerBeep();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Falha ao tocar {label}: {ex.Message}");
            TriggerBeep();
        }
    }

    private async Task<bool> UseAplay(string file)
    {
        try
        {
            Debug.WriteLine("Executando via aplay...");

            var info = new ProcessStartInfo
            {
                FileName = "aplay",
                Arguments = $"\"{file}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = Process.Start(info);
            if (proc != null)
            {
                await proc.WaitForExitAsync();
                if (proc.ExitCode == 0)
                {
                    Debug.WriteLine("aplay finalizou com sucesso");
                    return true;
                }
                else
                {
                    var errMsg = await proc.StandardError.ReadToEndAsync();
                    Debug.WriteLine($"Erro no aplay: {errMsg}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exceção no aplay: {e.Message}");
        }

        return false;
    }

    private void TriggerBeep()
    {
        try
        {
            Debug.WriteLine("Executando beep de fallback...");

            Console.Write("\a");

            _ = Task.Run(async () =>
            {
                try
                {
                    var beepInfo = new ProcessStartInfo
                    {
                        FileName = "beep",
                        Arguments = "-f 800 -l 100",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var proc = Process.Start(beepInfo);
                    if (proc != null)
                    {
                        await proc.WaitForExitAsync();
                        Debug.WriteLine("Beep executado com sucesso");
                        return;
                    }
                }
                catch { }

                try
                {
                    var fallback = new ProcessStartInfo
                    {
                        FileName = "speaker-test",
                        Arguments = "-t sine -f 800 -l 1",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var proc = Process.Start(fallback);
                    if (proc != null)
                    {
                        await Task.Delay(100);
                        try { proc.Kill(); } catch { }
                        Debug.WriteLine("Speaker-test finalizado");
                    }
                }
                catch
                {
                    Debug.WriteLine("Nenhuma alternativa de beep funcionou");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro no beep de fallback: {ex.Message}");
        }
    }

    public void SetVolume(double vol)
    {
        Debug.WriteLine($"Ajuste de volume solicitado: {Math.Clamp(vol, 0.0, 1.0)}");
    }

    public void MuteAll()
    {
        _muted = true;
        Debug.WriteLine("Som desativado");
    }

    public void UnmuteAll()
    {
        _muted = false;
        Debug.WriteLine("Som ativado");
    }

    public void Dispose()
    {
        Debug.WriteLine("Limpando recursos de áudio...");
    }
}
