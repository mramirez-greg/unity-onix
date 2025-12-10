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
   private int coins;
   public TMP_Text textCoins;



    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coins++;
            textCoins.text = coins.ToString();
        }

        if (collision.transform.CompareTag("Spikes"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
