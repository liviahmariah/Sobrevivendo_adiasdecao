using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 7f;

    [Header("Configurações do pulo")]
    public float jumpHeight = 3f;
    public float jumpDuration = 0.8f;
    public int maxJumps = 2;

    [Header("Detecção do chão")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Som do latido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somLatido;

    private Rigidbody2D rb;
    private Animator anim;

    private float moveInput;
    private int jumpCount;

    private bool isGrounded;
    private bool isJumping;

    private float jumpTime;
    private float startY;

    private float velocidadeBase;

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        velocidadeBase = speed;

        jumpCount = 0;

        rb.gravityScale = 1f;
    }

    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        VerificarChao();

        // =================================================
        // ANIMAÇÃO DE MOVIMENTO
        // =================================================

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
            anim.SetBool("Grounded", isGrounded);
        }

        // =================================================
        // CORRIDA / ANDAR
        // =================================================

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.RegistrarCorrida();
            }
        }

        // =================================================
        // PULO
        // =================================================

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumps)
            {
                StartJump();
            }
        }

        // =================================================
        // ATUALIZA PULO
        // =================================================

        if (isJumping)
        {
            UpdateJump();
        }

        // =================================================
        // LATIDO
        // =================================================

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Bark();
        }

        // =================================================
        // VIRAR PERSONAGEM
        // =================================================

        if (moveInput > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // =====================================================
    // FÍSICA
    // =====================================================

    void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(
            moveInput * speed,
            rb.linearVelocity.y
        );
    }

    // =====================================================
    // VELOCIDADE
    // =====================================================

    public void DefinirVelocidade(float multiplicador)
    {
        speed = velocidadeBase * multiplicador;
    }

    public void RestaurarVelocidade()
    {
        speed = velocidadeBase;
    }

    public float GetVelocidadeBase()
    {
        return velocidadeBase;
    }

    // =====================================================
    // VERIFICAR CHÃO
    // =====================================================

    void VerificarChao()
    {
        if (groundCheck == null)
            return;

        Collider2D colisao = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        isGrounded = colisao != null;

        if (isGrounded && !isJumping)
        {
            jumpCount = 0;
        }
    }

    // =====================================================
    // COMEÇAR PULO
    // =====================================================

    void StartJump()
    {
        isJumping = true;

        jumpTime = 0f;

        startY = transform.position.y;

        jumpCount++;

        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            0f
        );

        isGrounded = false;

        if (anim != null)
        {
            anim.SetBool("Grounded", false);
        }

        // ================================================
        // AVISA O TUTORIAL
        // ================================================

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.RegistrarPulo();
        }
    }

    // =====================================================
    // ATUALIZAR PULO
    // =====================================================

    void UpdateJump()
    {
        jumpTime += Time.deltaTime / jumpDuration;

        float t = Mathf.Clamp01(jumpTime);

        float height =
            4f *
            jumpHeight *
            t *
            (1f - t);

        transform.position = new Vector3(
            transform.position.x,
            startY + height,
            transform.position.z
        );

        if (t >= 1f)
        {
            isJumping = false;

            transform.position = new Vector3(
                transform.position.x,
                startY,
                transform.position.z
            );

            rb.gravityScale = 1f;

            VerificarChao();

            if (anim != null)
            {
                anim.SetBool("Grounded", isGrounded);
            }
        }
    }

    // =====================================================
    // LATIDO
    // =====================================================

    void Bark()
    {
        if (anim != null)
        {
            anim.SetTrigger("Bark");
        }

        if (audioSource != null && somLatido != null)
        {
            audioSource.PlayOneShot(somLatido);
        }

        Debug.Log("Latido!");

        // ================================================
        // AVISA O TUTORIAL
        // ================================================

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.RegistrarLatido();
        }
    }

    // =====================================================
    // COLISÃO COM CHÃO
    // =====================================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isJumping)
            {
                isGrounded = true;

                jumpCount = 0;

                if (anim != null)
                {
                    anim.SetBool("Grounded", true);
                }
            }
        }
    }

    // =====================================================
    // SAIR DO CHÃO
    // =====================================================

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isJumping)
            {
                isGrounded = false;

                if (anim != null)
                {
                    anim.SetBool("Grounded", false);
                }
            }
        }
    }

    // =====================================================
    // GIZMO
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}