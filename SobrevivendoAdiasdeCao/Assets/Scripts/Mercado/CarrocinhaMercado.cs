using UnityEngine;

public class CarrocinhaMercado : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3f;

    [Header("Alvo")]
    public Transform player;

    private Rigidbody2D rb;

    private bool perseguindo = false;
    private bool capturou = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // A carrocinha começa parada.
        Desativar();

        // Procura Sandy automaticamente.
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
        if (!perseguindo)
            return;

        if (player == null)
            return;
    }

    private void FixedUpdate()
    {
        if (!perseguindo)
            return;

        if (player == null)
            return;

        PerseguirSandy();
    }

    public void Ativar()
    {
        perseguindo = true;
        capturou = false;

        Debug.Log(
            "CARROCINHA DO MERCADO ATIVADA!"
        );
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
        Vector2 direcao =
            (player.position - transform.position).normalized;

        rb.linearVelocity =
            direcao * velocidade;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!perseguindo)
            return;

        if (capturou)
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

        // A derrota agora é controlada pelo FaseManager/MercadoManager.
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