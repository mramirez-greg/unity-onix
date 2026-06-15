using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Pantalla de victoria final. Muestra un resumen (huesos recogidos) y permite
/// volver al menú principal.
/// </summary>
public class VictoryUI : MonoBehaviour
{
    [Tooltip("Texto opcional para el resumen final (huesos recogidos, etc.).")]
    public TMP_Text summaryText;

    [Tooltip("Escena del menú principal a la que vuelve el botón.")]
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        if (summaryText != null && GameManager.Instance != null)
            summaryText.text = $"¡Onix reunió a toda su familia!\nHuesos recogidos: {GameManager.Instance.Bones}";
    }

    /// <summary>Asignar al OnClick del botón "Menú principal".</summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
