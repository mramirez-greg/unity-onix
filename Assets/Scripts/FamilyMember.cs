using UnityEngine;

/// <summary>
/// Va en el familiar que espera al final del nivel (Grego/Lilo/Mao). Es un trigger:
/// cuando Onix lo toca, lo marca como rescatado, muestra el diálogo de reencuentro y
/// después carga el siguiente nivel.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FamilyMember : MonoBehaviour
{
    [Header("Identidad")]
    [Tooltip("Índice del familiar: 0=Grego, 1=Lilo, 2=Mao. Si es -1, usa el del LevelManager.")]
    public int memberIndex = -1;

    [Tooltip("Nombre que aparece en el panel de diálogo (p.ej. 'Grego').")]
    public string memberName = "";

    [Header("Diálogo de rescate")]
    [TextArea(2, 5)]
    public string[] rescueLines;

    [Header("Confrontación final (opcional, p.ej. Kiwi)")]
    [Tooltip("Quién habla en el diálogo final (vacío = sin diálogo final).")]
    public string postRescueSpeaker = "";

    [TextArea(2, 5)]
    [Tooltip("Líneas que se muestran tras el rescate, antes de cargar la siguiente escena. " +
             "Úsalo en el Nivel 3 para la confrontación con Kiwi (la gata mandona) antes de Victory.")]
    public string[] postRescueLines;

    [Header("Epílogo (opcional, p.ej. Emily castiga a Kiwi)")]
    [Tooltip("Quién habla en el epílogo (vacío = sin epílogo).")]
    public string epilogueSpeaker = "";

    [TextArea(2, 5)]
    [Tooltip("Cierre de la historia, se muestra después de la confrontación y antes de Victory.")]
    public string[] epilogueLines;

    [Header("Sprite al ser rescatado (opcional)")]
    [Tooltip("Si se asigna, cambia el sprite a la pose 'feliz/rescatado' al tocarlo.")]
    public Sprite happySprite;

    private bool rescued;

    void Reset()
    {
        // Asegurar que el collider sea trigger al añadir el componente en el editor.
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (rescued) return;
        // Detectamos al jugador por su componente (no dependemos de que el tag 'Player' exista).
        if (collision.GetComponentInParent<Player>() == null) return;

        rescued = true;

        int index = memberIndex;
        if (index < 0 && LevelManager.Instance != null)
            index = LevelManager.Instance.familyMemberIndex;

        if (GameManager.Instance != null)
            GameManager.Instance.RescueMember(index);

        // Pose feliz, si la hay.
        if (happySprite != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = happySprite;
        }

        // Mostrar diálogo de reencuentro y, al cerrarlo, la confrontación final (si la hay)
        // y luego cargar la siguiente escena.
        var dialogue = DialoguePanel.Instance;
        if (dialogue != null && rescueLines != null && rescueLines.Length > 0)
            dialogue.Show(rescueLines, ShowPostRescueOrFinish, string.IsNullOrEmpty(memberName) ? null : memberName);
        else
            ShowPostRescueOrFinish();
    }

    void ShowPostRescueOrFinish()
    {
        var dialogue = DialoguePanel.Instance;
        if (dialogue != null && postRescueLines != null && postRescueLines.Length > 0)
            dialogue.Show(postRescueLines, ShowEpilogueOrFinish,
                string.IsNullOrEmpty(postRescueSpeaker) ? null : postRescueSpeaker);
        else
            ShowEpilogueOrFinish();
    }

    void ShowEpilogueOrFinish()
    {
        var dialogue = DialoguePanel.Instance;
        if (dialogue != null && epilogueLines != null && epilogueLines.Length > 0)
            dialogue.Show(epilogueLines, GoToNextLevel,
                string.IsNullOrEmpty(epilogueSpeaker) ? null : epilogueSpeaker);
        else
            GoToNextLevel();
    }

    void GoToNextLevel()
    {
        string next = LevelManager.Instance != null ? LevelManager.Instance.nextSceneName : "";
        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextLevel(next);
    }
}
