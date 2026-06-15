using UnityEngine;

/// <summary>
/// Coordina un nivel concreto: muestra el panel de introducción al entrar, conoce el
/// punto de aparición (spawnPoint) para respawnear al jugador tras perder una vida, y
/// sabe cuál es la escena siguiente y qué familiar se rescata aquí.
/// Hay uno por escena de nivel; se accede a él con LevelManager.Instance.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Identidad del nivel")]
    [Tooltip("Nombre de la zona, p.ej. 'Playa Soleada'.")]
    public string zoneName = "Playa";

    [Tooltip("Familiar que se rescata en este nivel: 0=Grego, 1=Lilo, 2=Mao.")]
    public int familyMemberIndex = 0;

    [Tooltip("Escena que se carga tras rescatar al familiar. Vacío = pantalla de victoria.")]
    public string nextSceneName = "";

    [Header("Introducción")]
    [Tooltip("Quién narra/aparece en la intro (p.ej. 'Kiwi'). Vacío = sin retrato ni nombre.")]
    public string introSpeaker = "";

    [TextArea(2, 5)]
    [Tooltip("Líneas del panel de historia que se muestran al empezar el nivel.")]
    public string[] introLines;

    [Header("Referencias de escena")]
    [Tooltip("Dónde reaparece el jugador al perder una vida. Si está vacío, usa su posición inicial.")]
    public Transform spawnPoint;

    private Player player;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = Object.FindFirstObjectByType<Player>();

        // Recordamos la posición inicial por si no se asignó un spawnPoint en el Inspector.
        if (spawnPoint == null && player != null)
        {
            var sp = new GameObject("SpawnPoint_Auto").transform;
            sp.position = player.transform.position;
            sp.SetParent(transform);
            spawnPoint = sp;
        }

        ShowIntro();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Muestra el panel de intro y bloquea al jugador hasta que se cierre.</summary>
    public void ShowIntro()
    {
        if (introLines == null || introLines.Length == 0) return;

        var dialogue = DialoguePanel.Instance;
        if (dialogue != null)
            dialogue.Show(introLines, null, string.IsNullOrEmpty(introSpeaker) ? null : introSpeaker);
        else
            Debug.LogWarning("[LevelManager] No hay DialoguePanel en la escena; se omite la intro.");
    }

    /// <summary>Reaparece al jugador en el spawnPoint (tras perder una vida sin morir del todo).</summary>
    public void RespawnPlayer()
    {
        if (player == null) player = Object.FindFirstObjectByType<Player>();
        if (player == null || spawnPoint == null) return;

        player.ResetToPosition(spawnPoint.position);
    }
}
