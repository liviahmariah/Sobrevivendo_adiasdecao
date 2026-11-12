using UnityEngine;

public class BrinquedoPraca : MonoBehaviour
{
    public enum TipoBrinquedo
    {
        BolaDePraia,
        Urso,
        Sandalia,
        Graveto,
        Pato,
        Bolinha,
        Galinha,
        Polvo,
        Disco
    }

    [Header("Identificação")]
    public TipoBrinquedo tipoBrinquedo;

    [Header("Pontuação")]
    [Min(0f)]
    public float valor = 3f;

    public bool brinquedoRuim = false;

    [Header("Aparência dos brinquedos ruins")]
    public Color corBrinquedoRuim = new Color(1f, 0.75f, 0.75f, 1f);

    private SpriteRenderer[] sprites;
    private Color[] coresOriginais;

    [Header("Sons dos brinquedos bons")]
    public AudioClip somBolaDePraia;
    public AudioClip somUrso;
    public AudioClip somSandalia;
    public AudioClip somGraveto;
    public AudioClip somPato;
    public AudioClip somBolinha;
    public AudioClip somGalinha;
    public AudioClip somPolvo;
    public AudioClip somDisco;

    [Header("Som dos brinquedos ruins")]
    public AudioClip somBrinquedoRuim;

    [Range(0f, 1f)]
    public float volumeSom = 1f;

    private bool coletado = false;

    private void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>(true);

        if (sprites != null && sprites.Length > 0)
        {
            coresOriginais = new Color[sprites.Length];

            for (int i = 0; i < sprites.Length; i++)
            {
                coresOriginais[i] = sprites[i].color;
            }
        }
    }

    public void AtualizarAparencia()
    {
        if (sprites == null) return;

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].color = brinquedoRuim
                ? corBrinquedoRuim
                : coresOriginais[i];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado || !other.CompareTag("Player"))
            return;

        Coletar();
    }

    public void Coletar()
    {
        if (coletado)
            return;

        if (PracaManager.Instance == null || !PracaManager.Instance.PodeColetar())
            return;

        coletado = true;

        float pontuacao = brinquedoRuim ? -valor : valor;

        // 1. Atualiza pontuação da Praça
        PracaManager.Instance.AdicionarDiversao(pontuacao);

        // 2. Exibe balão de comentário
        if (ComentarioBrinquedoPracaUI.Instance != null)
        {
            ComentarioBrinquedoPracaUI.Instance.MostrarComentario(
                tipoBrinquedo,
                valor,
                brinquedoRuim
            );
        }

        // 3. INTEGRAÇÃO COM A CAIXA DE BRINQUEDOS DA UI (PRAÇA)
        if (CaixaBrinquedosUI.Instance != null)
        {
            if (brinquedoRuim)
            {
                // Se o brinquedo for ruim, remove um item bom previamente guardado
                CaixaBrinquedosUI.Instance.RemoverUmBrinquedo();
                Debug.Log("PRAÇA | Brinquedo RUIM: Item removido da caixa!");
            }
            else
            {
                // Resgata o Sprite principal do brinquedo
                SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>(true);
                Sprite spriteParaUI = (sr != null) ? sr.sprite : null;

                if (spriteParaUI != null)
                {
                    CaixaBrinquedosUI.Instance.AdicionarBrinquedo(spriteParaUI);
                    Debug.Log("PRAÇA | Brinquedo BOM: Adicionado à caixa!");
                }
                else
                {
                    Debug.LogWarning("PRAÇA | Nenhum SpriteRenderer encontrado no brinquedo!");
                }
            }
        }

        // 4. Efeitos sonoros
        TocarSom();

        Debug.Log(
            "PRAÇA | Brinquedo: " + tipoBrinquedo +
            " | Pontuação: " + pontuacao +
            " | Ruim: " + brinquedoRuim
        );

        Destroy(gameObject);
    }

    private void TocarSom()
    {
        AudioClip somEscolhido = brinquedoRuim
            ? somBrinquedoRuim
            : ObterSomBrinquedo();

        if (somEscolhido != null)
        {
            AudioSource.PlayClipAtPoint(
                somEscolhido,
                transform.position,
                volumeSom
            );
        }
    }

    private AudioClip ObterSomBrinquedo()
    {
        switch (tipoBrinquedo)
        {
            case TipoBrinquedo.BolaDePraia:
                return somBolaDePraia;

            case TipoBrinquedo.Urso:
                return somUrso;

            case TipoBrinquedo.Sandalia:
                return somSandalia;

            case TipoBrinquedo.Graveto:
                return somGraveto;

            case TipoBrinquedo.Pato:
                return somPato;

            case TipoBrinquedo.Bolinha:
                return somBolinha;

            case TipoBrinquedo.Galinha:
                return somGalinha;

            case TipoBrinquedo.Polvo:
                return somPolvo;

            case TipoBrinquedo.Disco:
                return somDisco;

            default:
                return null;
        }
    }
}