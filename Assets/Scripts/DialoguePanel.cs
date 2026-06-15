using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de diálogo / historia. Recibe una cola de líneas y las muestra una a una;
/// el jugador avanza con clic o Espacio. Mientras está visible bloquea el control del
/// jugador (Player.SetCanMove(false)). Al terminar invoca un callback opcional.
/// Se accede con DialoguePanel.Instance.
/// </summary>
public class DialoguePanel : MonoBehaviour
{
    public static DialoguePanel Instance { get; private set; }

    [Header("Referencias UI")]
    [Tooltip("El GameObject raíz del panel que se activa/desactiva.")]
    public GameObject panelRoot;

    [Tooltip("Texto donde se escribe la línea actual.")]
    public TMP_Text dialogueText;

    [Tooltip("Texto opcional del nombre de quien habla (puede quedar vacío).")]
    public TMP_Text speakerText;

    private readonly Queue<string> lines = new Queue<string>();
    private Action onComplete;
    private bool isShowing;

    public bool IsShowing => isShowing;

    void Awake()
    {
        Instance = this;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (!isShowing) return;

        // Avanzar con Espacio, Enter, clic izquierdo o Submit.
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0) ||
            Input.GetButtonDown("Submit"))
        {
            Advance();
        }
    }

    /// <summary>Muestra una secuencia de líneas; al cerrarse, llama onFinished (puede ser null).</summary>
    public void Show(IEnumerable<string> newLines, Action onFinished, string speaker = null)
    {
        lines.Clear();
        foreach (var l in newLines) lines.Enqueue(l);

        if (lines.Count == 0)
        {
            onFinished?.Invoke();
            return;
        }

        onComplete = onFinished;
        isShowing = true;

        if (speakerText != null)
        {
            speakerText.text = speaker ?? "";
            speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        }

        if (panelRoot != null) panelRoot.SetActive(true);
        SetPlayerControl(false);
        ShowNextLine();
    }

    void Advance()
    {
        if (lines.Count > 0)
            ShowNextLine();
        else
            Close();
    }

    void ShowNextLine()
    {
        if (lines.Count == 0)
        {
            Close();
            return;
        }
        if (dialogueText != null)
            dialogueText.text = lines.Dequeue();
    }

    void Close()
    {
        isShowing = false;
        if (panelRoot != null) panelRoot.SetActive(false);
        SetPlayerControl(true);

        var cb = onComplete;
        onComplete = null;
        cb?.Invoke();
    }

    static void SetPlayerControl(bool canMove)
    {
        var player = UnityEngine.Object.FindFirstObjectByType<Player>();
        if (player != null) player.SetCanMove(canMove);
    }
}
