using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estado global del juego "Onix: El Rescate de la Familia".
/// Es un singleton persistente (DontDestroyOnLoad): conserva huesos, vidas y los
/// familiares rescatados entre escenas. Lanza eventos para que el HUD se actualice
/// sin tener que conocer al HUD directamente.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración inicial")]
    [Tooltip("Vidas con las que arranca cada partida.")]
    public int startingLives = 5;

    [Tooltip("Nombre de la escena que se carga al terminar el último nivel.")]
    public string victorySceneName = "Victory";

    [Tooltip("Nombre de la escena del menú principal (a donde vuelve un Game Over).")]
    public string mainMenuSceneName = "MainMenu";

    // --- Estado de la partida ---
    public int Bones { get; private set; }
    public int Lives { get; private set; }

    // Grego (0), Lilo (1), Mao (2). true = ya rescatado.
    public bool[] Rescued { get; private set; } = new bool[3];

    // --- Eventos para la UI ---
    public event Action<int> OnBonesChanged;          // nuevo total de huesos
    public event Action<int> OnLivesChanged;          // nuevo total de vidas
    public event Action<int> OnMemberRescued;         // índice del familiar rescatado
    public event Action OnGameOver;

    void Awake()
    {
        // Patrón singleton: si ya existe una instancia, este duplicado se destruye.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Si nunca se inicializó el estado (entrar directo a un nivel desde el editor),
        // arrancamos con valores por defecto para que el juego sea jugable igual.
        if (Lives <= 0)
            ResetGame();
    }

    /// <summary>Reinicia todo el estado para una nueva partida (lo llama el menú al pulsar Jugar).</summary>
    public void ResetGame()
    {
        Bones = 0;
        Lives = startingLives;
        Rescued = new bool[3];

        OnBonesChanged?.Invoke(Bones);
        OnLivesChanged?.Invoke(Lives);
    }

    /// <summary>Suma un hueso al marcador.</summary>
    public void AddBone(int amount = 1)
    {
        Bones += amount;
        OnBonesChanged?.Invoke(Bones);
    }

    /// <summary>
    /// El jugador recibe daño (pincho, barril mortal...). Pierde una vida.
    /// Devuelve true si todavía le quedan vidas (debe respawnear); false si fue Game Over.
    /// </summary>
    public bool TakeDamage(int amount = 1)
    {
        Lives = Mathf.Max(0, Lives - amount);
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            GameOver();
            return false;
        }
        return true;
    }

    /// <summary>Marca a un familiar como rescatado (0=Grego, 1=Lilo, 2=Mao).</summary>
    public void RescueMember(int memberIndex)
    {
        if (memberIndex < 0 || memberIndex >= Rescued.Length) return;
        if (Rescued[memberIndex]) return;

        Rescued[memberIndex] = true;
        OnMemberRescued?.Invoke(memberIndex);
    }

    public bool IsRescued(int memberIndex)
    {
        return memberIndex >= 0 && memberIndex < Rescued.Length && Rescued[memberIndex];
    }

    /// <summary>
    /// Carga la escena indicada. Si nextScene está vacío, va a la pantalla de victoria.
    /// Lo llama el LevelManager / FamilyMember tras el rescate.
    /// </summary>
    public void LoadNextLevel(string nextScene)
    {
        if (string.IsNullOrEmpty(nextScene))
            nextScene = victorySceneName;

        SceneManager.LoadScene(nextScene);
    }

    void GameOver()
    {
        OnGameOver?.Invoke();
        // Volvemos al menú principal. El menú llamará a ResetGame al pulsar Jugar otra vez.
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
