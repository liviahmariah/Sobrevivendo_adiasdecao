using UnityEngine;

public class CarrocinhaHospital : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3.5f;
    public bool virarParaSandy = true;

    [Header("Alvo")]
    public Transform player;

    [Header("Altura do chão")]
    public float alturaDoChao;

    private Rigidbody2D rb;
    private Vector3 escalaInicial;

    private bool perseguindo = false;
    private bool capturou = false;
    private bool alturaDefinida = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        escalaInicial = transform.localScale;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Start()
    {
        InicializarReferencias();
    }

    private void InicializarReferencias()
    {
        if (!alturaDefinida)
        {
            alturaDoChao = rb != null ? rb.position.y : transform.position.y;
            alturaDefinida = true;
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
                Debug.LogError("[CarrocinhaHospital] Nenhum objeto com a Tag 'Player' foi encontrado!");
            }
        }
    }

    public void Ativar()
    {
        gameObject.SetActive(true);
        InicializarReferencias();

        perseguindo = true;
        capturou = false;

        Debug.Log("[CarrocinhaHospital] ATIVADA! Perseguindo: " + (player != null ? player.name : "NULO"));
    }

    public void Desativar()
    {
        perseguindo = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (!perseguindo || capturou)
            return;

        if (player == null)
        {
            InicializarReferencias();
            if (player == null) return;
        }

        PerseguirSandy();
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
        else if (virarParaSandy)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(escalaInicial.x) * direcaoX,
                escalaInicial.y,
                escalaInicial.z
            );
        }

        // Move diretamente pela posição (funciona mesmo se o Rigidbody estiver Kinematic ou travado por colisor)
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
        VerificarCaptura(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VerificarCaptura(other.gameObject);
    }

    private void VerificarCaptura(GameObject objeto)
    {
        if (!perseguindo || capturou)
            return;

        if (!objeto.CompareTag("Player"))
            return;

        CapturarSandy();
    }

    private void CapturarSandy()
    {
        capturou = true;
        perseguindo = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("[CarrocinhaHospital] SANDY FOI CAPTURADA!");

        if (HospitalManager.Instance != null)
        {
            HospitalManager.Instance.Derrota();
        }
        else
        {
            Debug.LogError("[CarrocinhaHospital] HospitalManager.Instance é NULO!");
        }
    }
}