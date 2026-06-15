using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD de la partida: muestra el contador de huesos y los corazones (vidas).
/// Se suscribe a los eventos del GameManager para actualizarse solo.
/// </summary>
public class HUD : MonoBehaviour
{
    [Header("Huesos")]
    [Tooltip("Texto con el número de huesos recogidos.")]
    public TMP_Text bonesText;

    [Header("Vidas (corazones)")]
    [Tooltip("Contenedor donde se instancian los corazones. Opcional si usas livesText.")]
    public Transform heartsContainer;

    [Tooltip("Prefab/imagen de un corazón. Se clona una vez por vida.")]
    public GameObject heartPrefab;

    [Tooltip("Alternativa simple: muestra las vidas como texto (p.ej. 'x5').")]
    public TMP_Text livesText;

    private readonly List<GameObject> hearts = new List<GameObject>();

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBonesChanged += UpdateBones;
            GameManager.Instance.OnLivesChanged += UpdateLives;

            // Sincronizar con el estado actual al activarse.
            UpdateBones(GameManager.Instance.Bones);
            UpdateLives(GameManager.Instance.Lives);
        }
    }

    void Start()
    {
        // Por si el GameManager se creó después que el HUD en el orden de Awake.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBonesChanged -= UpdateBones;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnBonesChanged += UpdateBones;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            UpdateBones(GameManager.Instance.Bones);
            UpdateLives(GameManager.Instance.Lives);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBonesChanged -= UpdateBones;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
        }
    }

    void UpdateBones(int total)
    {
        if (bonesText != null) bonesText.text = total.ToString();
    }

    void UpdateLives(int total)
    {
        if (livesText != null) livesText.text = "x" + total;

        // Modo "corazones": reconstruir la fila de iconos.
        if (heartsContainer != null && heartPrefab != null)
        {
            // Asegurar que existan suficientes iconos instanciados.
            while (hearts.Count < total)
            {
                var h = Instantiate(heartPrefab, heartsContainer);
                hearts.Add(h);
            }
            // Mostrar/ocultar según las vidas actuales.
            for (int i = 0; i < hearts.Count; i++)
                hearts[i].SetActive(i < total);
        }
    }
}
