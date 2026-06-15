using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lógica del menú principal. Conecta los botones Jugar / Salir.
/// Jugar resetea el estado del GameManager y carga el primer nivel.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Tooltip("Primer nivel a cargar al pulsar Jugar.")]
    public string firstLevelScene = "Level1_Playa";

    /// <summary>Asignar al OnClick del botón "Jugar".</summary>
    public void PlayGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();

        SceneManager.LoadScene(firstLevelScene);
    }

    /// <summary>Asignar al OnClick del botón "Salir".</summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
