using UnityEngine;

public class MedicamentoColetavel : MonoBehaviour
{
    [Header("Identificação")]
    public string idMedicamento;

    [Header("Valor")]
    [Min(0f)]
    public float valor = 3f;

    [Header("Classificação")]
    public bool medicamentoRuim = false;

    [Header("Categoria")]
    public RecursoColetavel.CategoriaComida categoria =
        RecursoColetavel.CategoriaComida.Comum;

    [Header("Aparência do medicamento ruim")]
    public Color corMedicamentoRuim =
        new Color(0.55f, 0.85f, 0.55f, 1f);

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private bool coletado = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
        {
            corOriginal = spriteRenderer.color;

            if (medicamentoRuim)
                spriteRenderer.color = corMedicamentoRuim;
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

        if (FaseManager.Instance != null &&
            !FaseManager.Instance.PodeColetar())
            return;

        if (HospitalManager.Instance == null)
        {
            Debug.LogWarning(
                "MedicamentoColetavel: HospitalManager não encontrado!",
                this
            );
            return;
        }

        if (string.IsNullOrEmpty(idMedicamento))
        {
            Debug.LogWarning(
                "MedicamentoColetavel: ID do medicamento não definido!",
                this
            );
            return;
        }

        coletado = true;

        HospitalManager.Instance.RegistrarColetaMedicamento(
            idMedicamento,
            valor,
            medicamentoRuim
        );

        PontuacaoSandyUI pontuacaoUI =
            FindFirstObjectByType<PontuacaoSandyUI>();

        if (pontuacaoUI != null)
            pontuacaoUI.AtualizarPontuacao();

        Debug.Log(
            "MEDICAMENTO COLETADO | ID: " + idMedicamento +
            " | Valor: " + valor +
            " | Ruim: " + medicamentoRuim
        );

        Destroy(gameObject);
    }
}