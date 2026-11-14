using UnityEngine;

public class CarrocinhaMercado : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3.5f;

    [Header("Alvo")]
    public Transform player;

    [Header("Altura do chão")]
    [Tooltip("Altura fixa que a carrocinha deve manter.")]
    public float alturaDoChao;

    private Rigidbody2D rb;

    private bool perseguindo = false;
    private bool capturou = false;
    private bool alturaInicializada = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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
            {
                player = objetoPlayer.transform;
            }
            else
            {
                Debug.LogWarning("CarrocinhaMercado: Player não encontrado!");
            }
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

        Debug.Log("CARROCINHA ATIVADA! Perseguindo Sandy.");
    }

    public void Desativar()
    {
        perseguindo = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void PerseguirSandy()
    {
        float posAtualX = rb != null ? rb.position.x : transform.position.x;
        float distanciaX = player.position.x - posAtualX;
        float direcaoX = Mathf.Sign(distanciaX);

        if (Mathf.Abs(distanciaX) < 0.05f)
        {
            direcaoX = 0f;
        }

        float novoX = posAtualX + (direcaoX * velocidade * Time.fixedDeltaTime);
        Vector2 novaPosicao = new Vector2(novoX, alturaDoChao);

        if (rb != null)
        {
            rb.MovePosition(novaPosicao);
        }
        else
        {
            transform.position = novaPosicao;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        VerificarColisao(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VerificarColisao(other.gameObject);
    }

    private void VerificarColisao(GameObject objeto)
    {
        if (!perseguindo || capturou)
            return;

        if (!objeto.CompareTag("Player"))
            return;

        CapturarSandy();
    }

    private void CapturarSandy()
    {
        if (capturou)
            return;

        capturou = true;
        perseguindo = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("CARROCINHA ENCOSTOU NA SANDY!");

        // Envia a derrota para o Manager da fase atual (Hospital ou Mercado)
        if (HospitalManager.Instance != null)
        {
            HospitalManager.Instance.Derrota();
        }
        else if (MercadoManager.Instance != null)
        {
            MercadoManager.Instance.Derrota();
        }
        else
        {
            Debug.LogWarning("CarrocinhaMercado: Nenhum Manager (Hospital ou Mercado) encontrado!");
        }
    }
}