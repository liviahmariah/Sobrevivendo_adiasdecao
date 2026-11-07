using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 7f;

    [Header("Configurações do pulo")]
    public float jumpForce = 12f; // Força do 1º pulo
    [Range(0.1f, 1f)] public float doubleJumpMultiplier = 0.5f; // 0.5 = metade da força no 2º pulo!
    public int maxJumps = 2; // Permite o pulo duplo (2 pulos)

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

    private float velocidadeBase;
    private bool poeiraAtiva;

    [Header("Carrocinha")]
    public ChefeCarrocinhaTutorial chefe;

    private Vector3 escalaOriginal;

    private void Awake()
    {
        velocidadeBase = speed;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        escalaOriginal = transform.localScale;
        jumpCount = 0;

        if (rb != null)
        {
            rb.gravityScale = 2.5f;
        }
        else
        {
            Debug.LogError("PlayerMovement: Rigidbody2D não encontrado na Sandy.", this);
        }

        if (poeiraCorrida != null)
        {
            poeiraCorrida.Stop();
        }
    }

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

        // PULO (FÍSICA DE PULO DUPLO COM FORÇA REDUZIDA)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumps)
            {
                ExecutarPulo();
            }
        }

        // LATIDO
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Bark();
        }

        // VIRAR PERSONAGEM
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

    private void ExecutarPulo()
    {
        if (rb == null) return;

        PararPoeira();
        jumpCount++;

        // Calcula a força baseada no contador: 1º pulo = cheia | 2º pulo = metade (50%)
        float forcaAtual = (jumpCount == 1) ? jumpForce : jumpForce * doubleJumpMultiplier;

        // Aplica a força de pulo
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaAtual);

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

    private void FixedUpdate()
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(
            moveInput * speed,
            rb.linearVelocity.y
        );
    }

    private void VerificarChao()
    {
        if (groundCheck == null) return;

        Collider2D colisao = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        isGrounded = colisao != null;

        // Reseta os pulos ao pousar
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            jumpCount = 0;
        }
    }

    private void AtualizarPoeira()
    {
        if (poeiraCorrida == null) return;

        bool deveMostrarPoeira = Mathf.Abs(moveInput) > 0.1f && isGrounded;

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

    private void PararPoeira()
    {
        if (poeiraCorrida == null) return;
        poeiraCorrida.Stop();
        poeiraAtiva = false;
    }

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

    private void Bark()
    {
        if (anim != null) anim.SetTrigger("Bark");
        if (audioSource != null && somLatido != null) audioSource.PlayOneShot(somLatido);

        if (TutorialManager.instance != null) TutorialManager.instance.RegistrarLatido();
        if (chefe != null) chefe.AssustarChefe();
        if (molhoChaves != null) molhoChaves.ReceberLatido();

        float direcaoSandy = Mathf.Sign(transform.localScale.x);
        if (direcaoSandy == 0f) direcaoSandy = 1f;

        NPCRecurso[] npcs = FindObjectsByType<NPCRecurso>(FindObjectsSortMode.None);
        foreach (NPCRecurso npc in npcs)
        {
            if (npc == null) continue;
            Vector2 diferenca = (Vector2)npc.transform.position - (Vector2)transform.position;
            if (diferenca.x * direcaoSandy <= 0f) continue;
            npc.ReceberLatido();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}