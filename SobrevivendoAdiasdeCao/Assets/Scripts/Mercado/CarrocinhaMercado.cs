
using UnityEngine;

public class CarrocinhaMercado : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3f;

    [Header("Alvo")]
    public Transform player;

    [Header("Altura do chão")]
    [Tooltip("Altura fixa que a carrocinha deve manter.")]
    public float alturaDoChao;

    private Rigidbody2D rb;

    private bool perseguindo = false;
    private bool capturou = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Impede a rotação causada pelas colisões.
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Start()
    {
        // Guarda a altura inicial como referência do chão.
        alturaDoChao = rb != null
            ? rb.position.y
            : transform.position.y;

        Desativar();

        if (player == null)
        {
            GameObject objetoPlayer =
                GameObject.FindGameObjectWithTag("Player");

            if (objetoPlayer != null)
            {
                player = objetoPlayer.transform;
            }
            else
            {
                Debug.LogWarning(
                    "CarrocinhaMercado: Player não encontrado!"
                );
            }
        }
    }

    private void Update()
    {
        if (!perseguindo || player == null)
            return;
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (!perseguindo || player == null)
        {
            return;
        }

        PerseguirSandy();
    }

    public void Ativar()
    {
        perseguindo = true;
        capturou = false;

        Debug.Log("CARROCINHA DO MERCADO ATIVADA!");
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
        // Persegue somente na horizontal.
        float direcaoX =
            Mathf.Sign(player.position.x - rb.position.x);

        // Se Sandy estiver exatamente na mesma posição X,
        // não aplica movimento horizontal.
        if (Mathf.Abs(player.position.x - rb.position.x) < 0.05f)
        {
            direcaoX = 0f;
        }

        rb.linearVelocity = new Vector2(
            direcaoX * velocidade,
            0f
        );

        // Mantém a carrocinha na altura inicial.
        rb.position = new Vector2(
            rb.position.x,
            alturaDoChao
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!perseguindo || capturou)
            return;

        if (!collision.gameObject.CompareTag("Player"))
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

        Debug.Log("=================================");
        Debug.Log("CARROCINHA ENCOSTOU NA SANDY!");
        Debug.Log("SANDY FOI PEGA!");
        Debug.Log("=================================");

        if (MercadoManager.Instance != null)
        {
            MercadoManager.Instance.Derrota();
        }
        else
        {
            Debug.LogWarning(
                "CarrocinhaMercado: MercadoManager não encontrado!"
            );
        }
    }
}
