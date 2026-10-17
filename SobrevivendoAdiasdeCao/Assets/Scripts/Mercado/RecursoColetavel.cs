
using UnityEngine;

public class RecursoColetavel : MonoBehaviour
{
    public enum TipoRecurso
    {
        Comida,
        Remedio,
        Diversao
    }

    [Header("Tipo do Recurso")]
    public TipoRecurso tipoRecurso;

    [Header("Valor")]
    public float valor = 10f;

    [Header("Recurso ruim?")]
    public bool recursoRuim = false;

    [Header("Identificação da Comida")]
    public string idComida;
    public Sprite imagemComida;

    [Header("Aparência do Item Ruim")]
    public Color corRecursoRuim =
        new Color(0.55f, 0.85f, 0.55f, 1f);

    [Tooltip("Encontrado automaticamente nos filhos do prefab.")]
    [SerializeField]
    private ParticleSystem particulasMauCheiro;

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private bool coletado = false;

    // =====================================================
    // INICIALIZAÇÃO
    // =====================================================

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
        {
            corOriginal = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning(
                "RecursoColetavel: SpriteRenderer não encontrado!",
                this
            );
        }

        // Procura o Particle System no próprio objeto
        // ou em qualquer um dos seus filhos.
        if (particulasMauCheiro == null)
        {
            particulasMauCheiro =
                GetComponentInChildren<ParticleSystem>(true);
        }

        // Não inicia o mau cheiro antes do sorteio do spawn.
        if (particulasMauCheiro != null)
        {
            particulasMauCheiro.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            particulasMauCheiro.gameObject.SetActive(false);
        }

        AtualizarAparencia();
    }

    // =====================================================
    // ATUALIZA A APARÊNCIA
    // =====================================================

    public void AtualizarAparencia()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = recursoRuim
                ? corRecursoRuim
                : corOriginal;
        }

        if (particulasMauCheiro == null)
            return;

        if (recursoRuim)
        {
            particulasMauCheiro.gameObject.SetActive(true);
            particulasMauCheiro.Play(true);
        }
        else
        {
            particulasMauCheiro.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            particulasMauCheiro.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // COLETA AUTOMÁTICA
    // =====================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado)
            return;

        if (!other.CompareTag("Player"))
            return;

        Coletar();
    }

    // =====================================================
    // COLETAR
    // =====================================================

    public void Coletar()
    {
        if (coletado)
            return;

        if (FaseManager.Instance != null &&
            !FaseManager.Instance.PodeColetar())
        {
            return;
        }

        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "RecursoColetavel: CommunityManager não encontrado!",
                this
            );

            return;
        }

        coletado = true;

        AplicarEfeito();

        Debug.Log(
            "RECURSO COLETADO | Tipo: " + tipoRecurso +
            " | Valor: " + valor +
            " | Ruim: " + recursoRuim +
            " | ID: " + idComida,
            this
        );

        Destroy(gameObject);
    }

    // =====================================================
    // APLICA EFEITO
    // =====================================================

    private void AplicarEfeito()
    {
        float valorFinal = recursoRuim ? -valor : valor;

        switch (tipoRecurso)
        {
            case TipoRecurso.Comida:

                if (MercadoManager.Instance != null)
                {
                    MercadoManager.Instance.AdicionarComida(
                        valorFinal
                    );

                    if (!recursoRuim)
                    {
                        MercadoManager.Instance.RegistrarTipoComida(
                            idComida,
                            imagemComida
                        );
                    }

                    return;
                }

                CommunityManager.instance.AlterarAlimentacao(
                    valorFinal
                );

                break;

            case TipoRecurso.Remedio:

                CommunityManager.instance.AlterarSaude(
                    valorFinal
                );

                break;

            case TipoRecurso.Diversao:

                CommunityManager.instance.AlterarFelicidade(
                    valorFinal
                );

                break;
        }
    }
}
