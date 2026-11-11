using UnityEngine;

public class ProjetilMedicamento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 6f;
    public float tempoDeVida = 4f;
    public float velocidadeRotacao = 360f;

    [Header("Configuração do Rastro (TrailRenderer)")]
    public TrailRenderer rastroTrail;
    public Color corDoRastro = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Ajuste a cor do rastro aqui
    public float tempoVidaRastro = 0.25f;
    public float larguraRastro = 0.15f;

    private Vector2 direcao;
    private EnergiaSandy energia;
    private OndulacaoCamera efeitoOndulacao;
    private bool jaAtingiu = false;

    private void Start()
    {
        ConfigurarTrail();
        Destroy(gameObject, tempoDeVida);
    }

    private void ConfigurarTrail()
    {
        if (rastroTrail == null)
            rastroTrail = GetComponent<TrailRenderer>();

        // Se não houver TrailRenderer no objeto, adiciona um dinamicamente
        if (rastroTrail == null)
            rastroTrail = gameObject.AddComponent<TrailRenderer>();

        // Evita a cor rosa aplicando o Shader 2D padrão da Unity por código
        if (rastroTrail.material == null || rastroTrail.material.name.Contains("Default"))
        {
            rastroTrail.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Configurações visuais do Rastro
        rastroTrail.time = tempoVidaRastro;
        rastroTrail.startWidth = larguraRastro;
        rastroTrail.endWidth = 0f; // Afunila no final
        rastroTrail.startColor = corDoRastro;
        rastroTrail.endColor = new Color(corDoRastro.r, corDoRastro.g, corDoRastro.b, 0f);

        // Ajusta a renderização para ficar logo atrás do sprite do medicamento
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

    public void Configurar(
        Vector2 direcaoAlvo,
        EnergiaSandy energiaAlvo,
        OndulacaoCamera efeitoAlvo)
    {
        direcao = direcaoAlvo.normalized;
        energia = energiaAlvo;
        efeitoOndulacao = efeitoAlvo;

        if (rastroTrail != null)
            rastroTrail.Clear();
    }

    private void Update()
    {
        // 1. Movimento continuo na direção configurada
        transform.Translate(
            direcao * velocidade * Time.deltaTime,
            Space.World
        );

        // 2. Rotação do medicamento girando no próprio eixo durante o voo
        transform.Rotate(Vector3.forward, velocidadeRotacao * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (jaAtingiu) return;

        if (colisao.GetComponentInParent<PlayerMovement>() != null)
        {
            jaAtingiu = true;

            if (energia != null)
                energia.PerderEnergia();

            // CHAMA O EFEITO SIMPLES DE TELA RUIM VIA SINGLETON:
            if (EfeitoVisaoRuimUI.Instance != null)
                EfeitoVisaoRuimUI.Instance.AtivarVisaoRuim();

            Destroy(gameObject);
            return;
        }

        if (!colisao.isTrigger)
            Destroy(gameObject);
    }
}