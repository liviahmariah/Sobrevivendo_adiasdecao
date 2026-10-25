using UnityEngine;

public class RecursoColetavel : MonoBehaviour
{
    public enum TipoRecurso
    {
        Comida,
        Remedio,
        Diversao
    }

    public enum CategoriaComida
    {
        Comum,
        Valiosa
    }

    [Header("Tipo do Recurso")]
    public TipoRecurso tipoRecurso;

    [Header("Valor da coleta")]
    [Min(0f)]
    public float valor = 3f;

    [Header("Comida podre?")]
    public bool recursoRuim = false;

    [Header("Identificação da Comida")]
    public string idComida;
    public Sprite imagemComida;

    [Header("Categoria da comida")]
    public CategoriaComida categoriaComida =
        CategoriaComida.Comum;

    [Header("Sons de coleta")]
    public AudioClip somComidaComum;
    public AudioClip somComidaValiosa;
    public AudioClip somComidaPodre;

    [Range(0f, 1f)]
    public float volumeSom = 1f;

    [Header("Aparência do Item Podre")]
    public Color corRecursoRuim =
        new Color(0.55f, 0.85f, 0.55f, 1f);

    [Tooltip("Encontrado automaticamente nos filhos do prefab.")]
    [SerializeField]
    private ParticleSystem particulasMauCheiro;

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private bool coletado = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
            corOriginal = spriteRenderer.color;
        else
            Debug.LogWarning(
                "RecursoColetavel: SpriteRenderer não encontrado!",
                this
            );

        if (particulasMauCheiro == null)
        {
            particulasMauCheiro =
                GetComponentInChildren<ParticleSystem>(true);
        }

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado)
            return;

        if (!other.CompareTag("Player"))
            return;

        Coletar();
    }

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

        float valorFinal = recursoRuim ? -valor : valor;
        if (tipoRecurso == TipoRecurso.Comida &&
    MercadoManager.Instance != null)
        {
            MercadoManager.Instance.RegistrarColetaComida(
                valor,
                recursoRuim
            );

            PontuacaoSandyUI pontuacaoUI =
                FindFirstObjectByType<PontuacaoSandyUI>();

            if (pontuacaoUI != null)
            {
                pontuacaoUI.AtualizarPontuacao();
            }
        }

        // Toca o som correspondente à coleta.
        if (tipoRecurso == TipoRecurso.Comida)
            TocarSomColeta();

        // Aplica o efeito original na comunidade.
        AplicarEfeito();

        // Mostra o balão de comentário.
        if (tipoRecurso == TipoRecurso.Comida &&
            ComentarioColetaUI.Instance != null)
        {
            ComentarioColetaUI.Instance.MostrarComentario(
                idComida,
                valorFinal,
                recursoRuim
            );
        }

        Debug.Log(
            "RECURSO COLETADO | Tipo: " + tipoRecurso +
            " | Valor: " + valorFinal +
            " | Categoria: " + categoriaComida +
            " | Podre: " + recursoRuim +
            " | ID: " + idComida,
            this
        );

        Destroy(gameObject);
    }

    private void TocarSomColeta()
    {
        AudioClip somEscolhido;

        if (recursoRuim)
        {
            somEscolhido = somComidaPodre;
        }
        else if (categoriaComida == CategoriaComida.Valiosa)
        {
            somEscolhido = somComidaValiosa;
        }
        else
        {
            somEscolhido = somComidaComum;
        }

        if (somEscolhido != null)
        {
            AudioSource.PlayClipAtPoint(
                somEscolhido,
                transform.position,
                volumeSom
            );
        }
    }

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

                    if (recursoRuim)
                    {
                        if (CaixaComidasUI.Instance != null)
                        {
                            CaixaComidasUI.Instance.RemoverUmaComida();
                        }
                    }
                    else
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