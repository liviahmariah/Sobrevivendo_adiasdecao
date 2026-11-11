using UnityEngine;

public class ProjetilFeirante : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 4.5f;
    public float tempoDeVida = 3f;
    public float velocidadeRotacao = 360f;

    [Header("Colisão e Camadas")]
    public LayerMask camadasObstaculos;

    [Header("Configuração do Rastro (TrailRenderer)")]
    public TrailRenderer rastroTrail;
    public Color corDoRastro = new Color(1f, 1f, 1f, 0.8f);
    public float tempoVidaRastro = 0.25f;
    public float larguraRastro = 0.15f;

    private Vector2 direcaoFixa;
    private bool configurado;

    private void Start()
    {
        ConfigurarTrail();
        Destroy(gameObject, tempoDeVida);
    }

    private void ConfigurarTrail()
    {
        if (rastroTrail == null)
            rastroTrail = GetComponent<TrailRenderer>();

        // Se ainda assim não existir um TrailRenderer no prefab, cria um via código
        if (rastroTrail == null)
        {
            rastroTrail = gameObject.AddComponent<TrailRenderer>();
        }

        // Configurações visuais do Rastro
        rastroTrail.time = tempoVidaRastro;
        rastroTrail.startWidth = larguraRastro;
        rastroTrail.endWidth = 0f; // Afunila no final
        rastroTrail.startColor = corDoRastro;
        rastroTrail.endColor = new Color(corDoRastro.r, corDoRastro.g, corDoRastro.b, 0f);

        // Garante que o rastro renderize por cima do cenário
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            rastroTrail.sortingLayerID = sr.sortingLayerID;
            rastroTrail.sortingOrder = sr.sortingOrder - 1;
        }
        else
        {
            rastroTrail.sortingOrder = 10;
        }
    }

    public void Configurar(Vector3 posicaoAlvo)
    {
        direcaoFixa = ((Vector2)posicaoAlvo - (Vector2)transform.position).normalized;
        configurado = true;

        if (rastroTrail != null)
            rastroTrail.Clear();
    }

    private void Update()
    {
        if (!configurado)
            return;

        // 1. Move em linha reta
        transform.position += (Vector3)(direcaoFixa * velocidade * Time.deltaTime);

        // 2. Rotação do projétil no ar
        transform.Rotate(Vector3.forward, velocidadeRotacao * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Untagged") || other.GetComponent<AtaqueFeirante>() != null)
            return;

        if (other.CompareTag("Player"))
        {
            EnergiaSandy energia = other.GetComponentInParent<EnergiaSandy>();
            if (energia == null)
                energia = other.GetComponentInChildren<EnergiaSandy>();

            if (energia != null)
            {
                energia.PerderEnergia();
            }

            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & camadasObstaculos.value) != 0)
        {
            Destroy(gameObject);
        }
    }
}