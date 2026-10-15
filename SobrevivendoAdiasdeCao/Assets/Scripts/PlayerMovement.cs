
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 7f;

    [Header("Configurações do pulo")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.8f;
    public int maxJumps = 1;

    [Header("Detecção do chão")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Som do latido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somLatido;

    [Header("Poeira da corrida")]
    [SerializeField] private ParticleSystem poeiraCorrida;

    [Header("Molho de chaves")]
    public MolhoChavesTutorial molhoChaves;

    private Rigidbody2D rb;
    private Animator anim;

    private float moveInput;
    private int jumpCount;

    private bool isGrounded;
    private bool isJumping;

    private float jumpTime;
    private float startY;

    private float velocidadeBase;

    private bool poeiraAtiva;

    [Header("Carrocinha")]
    public ChefeCarrocinhaTutorial chefe;

    // Escala original do Player
    private Vector3 escalaOriginal;

    // =====================================================
    // AWAKE - INICIALIZA A VELOCIDADE-BASE ANTES DOS STARTS
    // =====================================================

    private void Awake()
    {
        velocidadeBase = speed;
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        escalaOriginal = transform.localScale;

        jumpCount = 0;

        if (rb != null)
        {
            rb.gravityScale = 1f;
        }
        else
        {
            Debug.LogError(
                "PlayerMovement: Rigidbody2D não encontrado na Sandy.",
                this
            );
        }

        if (poeiraCorrida != null)
        {
            poeiraCorrida.Stop();
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        VerificarChao();

        // ANIMAÇÃO DE MOVIMENTO
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
            anim.SetBool("Grounded", isGrounded);
        }

        // REGISTRO DE CORRIDA NO TUTORIAL
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.RegistrarCorrida();
            }
        }

        // POEIRA DA CORRIDA
        AtualizarPoeira();

        // PULO
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumps)
            {
                StartJump();
            }
        }

        // ATUALIZAÇÃO DO PULO
        if (isJumping)
        {
            UpdateJump();
        }

        // LATIDO
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Bark();
        }

        // VIRAR PERSONAGEM, MANTENDO A ESCALA ORIGINAL
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(escalaOriginal.x),
                escalaOriginal.y,
                escalaOriginal.z
            );
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(escalaOriginal.x),
                escalaOriginal.y,
                escalaOriginal.z
            );
        }
    }

    // =====================================================
    // POEIRA DA CORRIDA
    // =====================================================

    private void AtualizarPoeira()
    {
        if (poeiraCorrida == null)
            return;

        bool deveMostrarPoeira =
            Mathf.Abs(moveInput) > 0.1f &&
            isGrounded &&
            !isJumping;

        if (deveMostrarPoeira)
        {
            var shape = poeiraCorrida.shape;

            if (moveInput > 0)
            {
                shape.rotation = new Vector3(0f, 0f, 90f);
            }
            else if (moveInput < 0)
            {
                shape.rotation = new Vector3(0f, 0f, -90f);
            }

            if (!poeiraAtiva)
            {
                poeiraCorrida.Play();
                poeiraAtiva = true;
            }
        }
        else
        {
            if (poeiraAtiva)
            {
                poeiraCorrida.Stop();
                poeiraAtiva = false;
            }
        }
    }

    // =====================================================
    // FÍSICA
    // =====================================================

    private void FixedUpdate()
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

        Debug.Log(
            $"Velocidade-base: {velocidadeBase} | " +
            $"Multiplicador: {multiplicador} | " +
            $"Velocidade atual: {speed}",
            this
        );
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

    private void VerificarChao()
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

    private void StartJump()
    {
        if (rb == null)
            return;

        isJumping = true;

        PararPoeira();

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

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.RegistrarPulo();
        }
    }

    // =====================================================
    // PARAR POEIRA
    // =====================================================

    private void PararPoeira()
    {
        if (poeiraCorrida == null)
            return;

        poeiraCorrida.Stop();
        poeiraAtiva = false;
    }

    // =====================================================
    // ATUALIZAR PULO
    // =====================================================

    private void UpdateJump()
    {
        if (jumpDuration <= 0f)
            return;

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

            if (rb != null)
            {
                rb.gravityScale = 1f;
            }

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

    private void Bark()
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

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.RegistrarLatido();
        }

        if (chefe != null)
        {
            chefe.AssustarChefe();
        }

        if (molhoChaves != null)
        {
            molhoChaves.ReceberLatido();
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
                PararPoeira();

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
