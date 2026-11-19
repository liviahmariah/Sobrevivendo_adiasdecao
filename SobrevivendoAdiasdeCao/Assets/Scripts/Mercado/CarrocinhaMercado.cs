using UnityEngine;

public class CarrocinhaMercado : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3.5f;
    public bool virarParaSandy = true;

    [Header("Alvo")]
    public Transform player;

    [Header("Animação")]
    public Animator animator;
    [Tooltip("Nome do parâmetro Bool no Animator (ex: Andando ou Perseguindo)")]
    public string paramAndando = "Andando";

    [Header("Altura do chão")]
    [Tooltip("Altura fixa que a carrocinha deve manter.")]
    public float alturaDoChao;

    private Rigidbody2D rb;
    private Vector3 escalaInicial;

    private bool perseguindo = false;
    private bool capturou = false;
    private bool alturaInicializada = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        escalaInicial = transform.localScale;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Start()
    {
        Inicializar();
    }

    private void Inicializar()
    {
        if (!alturaInicializada)
        {
            alturaDoChao = rb != null ? rb.position.y : transform.position.y;
            alturaInicializada = true;
        }

        if (player == null)
        {
            GameObject objetoPlayer = GameObject.FindGameObjectWithTag("Player");
            if (objetoPlayer != null)
                player = objetoPlayer.transform;
        }
    }

    private void FixedUpdate()
    {
        if (!perseguindo || capturou)
            return;

        if (player == null)
        {
            Inicializar();
            if (player == null) return;
        }

        PerseguirSandy();
    }

    public void Ativar()
    {
        gameObject.SetActive(true);
        Inicializar();

        perseguindo = true;
        capturou = false;

        AtualizarAnimacao(true);
    }

    public void Desativar()
    {
        perseguindo = false;
        AtualizarAnimacao(false);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void PerseguirSandy()
    {
        float posAtualX = rb != null ? rb.position.x : transform.position.x;
        float distanciaX = player.position.x - posAtualX;
        float direcaoX = Mathf.Abs(distanciaX) < 0.05f ? 0f : Mathf.Sign(distanciaX);

        AtualizarAnimacao(direcaoX != 0f);

        if (direcaoX != 0f && virarParaSandy)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(escalaInicial.x) * direcaoX,
                escalaInicial.y,
                escalaInicial.z
            );
        }

        float novoX = posAtualX + (direcaoX * velocidade * Time.fixedDeltaTime);
        Vector2 novaPosicao = new Vector2(novoX, alturaDoChao);

        if (rb != null)
            rb.MovePosition(novaPosicao);
        else
            transform.position = novaPosicao;
    }

    private void AtualizarAnimacao(bool estaAndando)
    {
        if (animator != null && !string.IsNullOrEmpty(paramAndando))
            animator.SetBool(paramAndando, estaAndando);
    }

    private void OnCollisionEnter2D(Collision2D collision) => VerificarColisao(collision.gameObject);
    private void OnTriggerEnter2D(Collider2D other) => VerificarColisao(other.gameObject);

    private void VerificarColisao(GameObject objeto)
    {
        if (!perseguindo || capturou || !objeto.CompareTag("Player"))
            return;

        CapturarSandy();
    }

    private void CapturarSandy()
    {
        if (capturou)
            return;

        capturou = true;
        perseguindo = false;
        AtualizarAnimacao(false);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (HospitalManager.Instance != null)
            HospitalManager.Instance.Derrota();
        else if (MercadoManager.Instance != null)
            MercadoManager.Instance.Derrota();
    }
}