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

    private bool coletado = false;

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

        // ==========================================
        // BLOQUEIA A COLETA QUANDO O TEMPO DA FASE ACABOU
        // ==========================================

        if (FaseManager.Instance != null &&
            !FaseManager.Instance.PodeColetar())
        {
            return;
        }

        // ==========================================
        // VERIFICA COMMUNITY MANAGER
        // ==========================================

        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "RecursoColetavel: CommunityManager não encontrado!"
            );
            return;
        }

        coletado = true;

        AplicarEfeito();

        Debug.Log(
            "RECURSO COLETADO | Tipo: " +
            tipoRecurso +
            " | Valor: " +
            valor +
            " | Ruim: " +
            recursoRuim +
            " | ID: " +
            idComida
        );

        Destroy(gameObject);
    }

    private void AplicarEfeito()
    {
        float valorFinal = recursoRuim ? -valor : valor;

        // ==========================================
        // COMIDA
        // ==========================================

        if (tipoRecurso == TipoRecurso.Comida)
        {
            if (MercadoManager.Instance != null)
            {
                MercadoManager.Instance.AdicionarComida(
                    valorFinal
                );

                // Só registra comidas boas.
                if (!recursoRuim)
                {
                    MercadoManager.Instance.RegistrarTipoComida(
                        idComida,
                        imagemComida
                    );
                }

                return;
            }

            // Fora do Mercado.
            CommunityManager.instance.AlterarAlimentacao(
                valorFinal
            );

            return;
        }

        // ==========================================
        // REMÉDIO
        // ==========================================

        if (tipoRecurso == TipoRecurso.Remedio)
        {
            CommunityManager.instance.AlterarSaude(
                valorFinal
            );

            return;
        }

        // ==========================================
        // DIVERSÃO
        // ==========================================

        if (tipoRecurso == TipoRecurso.Diversao)
        {
            CommunityManager.instance.AlterarFelicidade(
                valorFinal
            );

            return;
        }
    }
}