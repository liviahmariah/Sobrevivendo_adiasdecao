using UnityEngine;

public class MedicamentoColetavel : MonoBehaviour
{
    [Header("Identificação")]
    public string idMedicamento = "medicamento";

    [Header("Valor")]
    [Min(0f)]
    public float valor = 3f;

    [Header("Classificação")]
    public bool medicamentoRuim = false;

    [Header("Categoria")]
    public RecursoColetavel.CategoriaComida categoria = RecursoColetavel.CategoriaComida.Comum;

    [Header("Aparência do medicamento ruim")]
    public Color corMedicamentoRuim = new Color(0.55f, 0.85f, 0.55f, 1f);

    [Tooltip("Encontrado automaticamente nos filhos do prefab.")]
    [SerializeField]
    private ParticleSystem particulasMedicamentoRuim;

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private bool coletado = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
            corOriginal = spriteRenderer.color;

        if (particulasMedicamentoRuim == null)
        {
            particulasMedicamentoRuim = GetComponentInChildren<ParticleSystem>(true);
        }

        if (particulasMedicamentoRuim != null)
        {
            particulasMedicamentoRuim.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            particulasMedicamentoRuim.gameObject.SetActive(false);
        }

        AtualizarAparencia();
    }

    public void AtualizarAparencia()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = medicamentoRuim ? corMedicamentoRuim : corOriginal;
        }

        if (particulasMedicamentoRuim == null)
        {
            particulasMedicamentoRuim = GetComponentInChildren<ParticleSystem>(true);
        }

        if (particulasMedicamentoRuim == null)
            return;

        if (medicamentoRuim)
        {
            particulasMedicamentoRuim.gameObject.SetActive(true);
            particulasMedicamentoRuim.Play(true);
        }
        else
        {
            particulasMedicamentoRuim.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            particulasMedicamentoRuim.gameObject.SetActive(false);
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

        if (FaseManager.Instance != null && !FaseManager.Instance.PodeColetar())
            return;

        if (HospitalManager.Instance == null)
        {
            Debug.LogWarning("MedicamentoColetavel: HospitalManager não encontrado!", this);
            return;
        }

        if (string.IsNullOrEmpty(idMedicamento))
        {
            idMedicamento = "medicamento";
        }

        coletado = true;
        float multiplicadorPontuacao = 1f;

        ItemSoltoPorLatido itemDoLatido = GetComponent<ItemSoltoPorLatido>();
        DetectorPuloSandy detector = FindFirstObjectByType<DetectorPuloSandy>();

        if (itemDoLatido != null && detector != null && detector.EstaNoAr())
        {
            multiplicadorPontuacao = 1.5f;
            Debug.Log("BÔNUS DE LATIDO + PULO: 1,5x!");
        }

        // 1. Registra a coleta e calcula os pontos no HospitalManager
        HospitalManager.Instance.RegistrarColetaMedicamento(
            valor,
            medicamentoRuim,
            multiplicadorPontuacao
        );

        // 2. Exibe os comentários da UI (balão de texto)
        if (ComentarioMedicamentoUI.Instance != null)
        {
            bool valioso = categoria == RecursoColetavel.CategoriaComida.Valiosa;

            ComentarioMedicamentoUI.Instance.MostrarComentario(
                idMedicamento,
                valor,
                medicamentoRuim,
                valioso
            );
        }

        // 3. Gerencia a Caixa de Ícones na UI (Remover se Ruim / Adicionar se Bom)
        if (CaixaComidasHospitalUI.Instance != null)
        {
            if (medicamentoRuim)
            {
                CaixaComidasHospitalUI.Instance.RemoverUmaComida();
                Debug.Log("MEDICAMENTO RUIM: Item bom removido da caixa!");
            }
            else
            {
                SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>(true);
                Sprite spriteParaUI = (sr != null) ? sr.sprite : null;
                CaixaComidasHospitalUI.Instance.AdicionarComida(idMedicamento, spriteParaUI);
                Debug.Log("MEDICAMENTO BOM: Adicionado à caixa!");
            }
        }
        else
        {
            Debug.LogWarning("CaixaComidasHospitalUI.Instance não foi encontrada na cena!");
        }

        // 4. Atualiza o texto da pontuação na UI
        if (PontuacaoHospitalUI.Instance != null)
        {
            PontuacaoHospitalUI.Instance.AtualizarPontuacao();
        }
        else
        {
            PontuacaoSandyUI pontuacaoGenerica = FindFirstObjectByType<PontuacaoSandyUI>();
            if (pontuacaoGenerica != null)
            {
                pontuacaoGenerica.AtualizarPontuacao();
            }
        }

        Debug.Log("MEDICAMENTO COLETADO | ID: " + idMedicamento + " | Valor: " + valor + " | Ruim: " + medicamentoRuim);

        Destroy(gameObject);
    }
}