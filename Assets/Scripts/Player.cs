using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
   public float speed = 5;

   private Rigidbody2D rb2D;

   private float move;

   public float jumpForce = 4;
   private bool isGrounded;
   public Transform groundCheck;
   public float groundRadius = 0.1f;
   public LayerMask groundLayer;

   private Animator animator;

   // Contador local de monedas/huesos: solo se usa como respaldo si no hay GameManager
   // (p.ej. abrir un nivel suelto en el editor). En partida normal manda el GameManager.
   private int coins;
   public TMP_Text textCoins;

   public AudioSource audioSource;
   public AudioClip coinClip;
   public AudioClip barrelClip;

   // Cuando es false, los diálogos/intro tienen "congelado" al jugador.
   private bool canMove = true;



    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Si no se asignó un AudioSource en el Inspector, intenta usar el del propio objeto.
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Mientras un diálogo/intro está activo, el jugador no responde a controles
        // y se queda quieto en el sitio.
        if (!canMove)
        {
            move = 0;
            if (rb2D != null)
                rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
                animator.SetFloat("VerticalVelocity", rb2D != null ? rb2D.linearVelocity.y : 0);
                animator.SetBool("IsGrounded", isGrounded);
            }
            return;
        }

        move = Input.GetAxisRaw("Horizontal");
        rb2D.linearVelocity = new Vector2(move*speed, rb2D.linearVelocity.y);

        if(move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move),1,1);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x,jumpForce);
        }
        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounded);
    }
    private void FixedUpdate()
    {
        // esto es para saber siestsi estoy en el piso
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    /// <summary>Permite a los diálogos/intro congelar y reanudar el control del jugador.</summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    /// <summary>Reaparece al jugador en una posición (lo usa el LevelManager al respawnear).</summary>
    public void ResetToPosition(Vector3 position)
    {
        transform.position = position;
        if (rb2D != null) rb2D.linearVelocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hueso/moneda: suma al marcador global (o al local si no hay GameManager).
        if(collision.transform.CompareTag("Coin") || collision.transform.CompareTag("Bone"))
        {
            if (audioSource != null && coinClip != null)
            {
                audioSource.PlayOneShot(coinClip);
            }
            Destroy(collision.gameObject);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddBone();
            }
            else
            {
                coins++;
                if (textCoins != null) textCoins.text = coins.ToString();
            }
        }

        if (collision.transform.CompareTag("Spikes"))
        {
            HandleDamage();
        }

        if(collision.transform.CompareTag("Barrel"))
        {
            if (audioSource != null && barrelClip != null)
            {
                audioSource.PlayOneShot(barrelClip);
            }
            Vector2 knockbackDir = (rb2D.position- (Vector2)collision.transform.position).normalized;
            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(knockbackDir*3,ForceMode2D.Impulse);

            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();

            foreach (BoxCollider2D col in colliders)
            {
                col.enabled = false;
            }

            collision.GetComponent<Animator>().enabled=true;
            Destroy(collision.gameObject, 0.5f);
        }
    }

    /// <summary>
    /// El jugador recibe daño mortal (pinchos). Con GameManager: pierde una vida y
    /// respawnea; si se queda sin vidas, el GameManager se encarga del Game Over.
    /// Sin GameManager: comportamiento antiguo (recargar la escena).
    /// </summary>
    private void HandleDamage()
    {
        if (GameManager.Instance != null)
        {
            bool survived = GameManager.Instance.TakeDamage();
            if (survived && LevelManager.Instance != null)
                LevelManager.Instance.RespawnPlayer();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
