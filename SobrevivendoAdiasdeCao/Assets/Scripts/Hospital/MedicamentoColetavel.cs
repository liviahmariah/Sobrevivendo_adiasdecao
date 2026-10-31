
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
            corOriginal = spriteRenderer.color;

        AtualizarAparencia();
    }

    public void AtualizarAparencia()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer == null)
            return;

        spriteRenderer.color = medicamentoRuim
            ? corMedicamentoRuim
            : corOriginal;
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

        if (ComentarioMedicamentoUI.Instance != null)
        {
            bool valioso =
                categoria == RecursoColetavel.CategoriaComida.Valiosa;

            float multiplicador = medicamentoRuim
                ? 1f
                : HospitalManager.Instance.ObterMultiplicadorMedicamento(
                    idMedicamento
                );

            float valorFinal = valor * multiplicador;

            ComentarioMedicamentoUI.Instance.MostrarComentario(
                idMedicamento,
                valorFinal,
                medicamentoRuim,
                valioso
            );
        }

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
