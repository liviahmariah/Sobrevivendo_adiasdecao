using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommunityManager : MonoBehaviour
{
    public static CommunityManager instance;

    [Header("Status da Comunidade")]
    [Range(0, 100)] public float alimentacao = 100f;
    [Range(0, 100)] public float saude = 100f;
    [Range(0, 100)] public float felicidade = 100f;

    [Header("Barras")]
    public Slider barraAlimentacao;
    public Slider barraSaude;
    public Slider barraFelicidade;

    [Header("Percentuais")]
    public TextMeshProUGUI textoAlimentacao;
    public TextMeshProUGUI textoSaude;
    public TextMeshProUGUI textoFelicidade;

    [Header("Consumo de Alimentação")]
    public float consumoAlimentacaoNormal = 0.8f;
    public float multiplicadorConsumoSaudeAtencao = 1.5f;
    public float multiplicadorConsumoSaudeCritica = 2f;

    [Header("Alimentação → Saúde")]
    public float perdaSaudeFomeAtencao = 1f;
    public float perdaSaudeFomeCritica = 2f;

    [Header("Influência na Felicidade")]
    public float perdaFelicidadeFome = 0.5f;
    public float perdaFelicidadeSaude = 0.5f;
    public float perdaFelicidadeSaudeCritica = 1f;


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ConfigurarBarras();
        AtualizarUI();
    }


    // =========================================================
    // ATUALIZAÇÃO
    // =========================================================

    private void Update()
    {
        // A comunidade só sofre o desgaste natural
        // enquanto o dia estiver correndo no MAPA.
        if (DayManager.Instance != null && DayManager.Instance.DiaEstaAtivo())
        {
            AtualizarComunidade();
        }

        AtualizarUI();
    }


    // =========================================================
    // CONFIGURAÇÃO DAS BARRAS
    // =========================================================

    private void ConfigurarBarras()
    {
        if (barraAlimentacao != null)
        {
            barraAlimentacao.minValue = 0;
            barraAlimentacao.maxValue = 100;
        }

        if (barraSaude != null)
        {
            barraSaude.minValue = 0;
            barraSaude.maxValue = 100;
        }

        if (barraFelicidade != null)
        {
            barraFelicidade.minValue = 0;
            barraFelicidade.maxValue = 100;
        }
    }


    // =========================================================
    // LÓGICA NATURAL DA COMUNIDADE
    // =========================================================

    private void AtualizarComunidade()
    {
        // -----------------------------------------
        // CONSUMO DE ALIMENTAÇÃO
        // -----------------------------------------

        float consumoAtual = consumoAlimentacaoNormal;

        if (SaudeCritica())
        {
            consumoAtual *= multiplicadorConsumoSaudeCritica;
        }
        else if (SaudeEmAtencao())
        {
            consumoAtual *= multiplicadorConsumoSaudeAtencao;
        }

        alimentacao -= consumoAtual * Time.deltaTime;


        // -----------------------------------------
        // FOME → PERDA DE SAÚDE
        // -----------------------------------------

        if (AlimentacaoCritica())
        {
            saude -= perdaSaudeFomeCritica * Time.deltaTime;
        }
        else if (AlimentacaoEmAtencao())
        {
            saude -= perdaSaudeFomeAtencao * Time.deltaTime;
        }


        // -----------------------------------------
        // FOME → PERDA DE FELICIDADE
        // -----------------------------------------

        if (AlimentacaoEmAtencao() || AlimentacaoCritica())
        {
            felicidade -= perdaFelicidadeFome * Time.deltaTime;
        }


        // -----------------------------------------
        // SAÚDE → PERDA DE FELICIDADE
        // -----------------------------------------

        if (SaudeCritica())
        {
            felicidade -= perdaFelicidadeSaudeCritica * Time.deltaTime;
        }
        else if (SaudeEmAtencao())
        {
            felicidade -= perdaFelicidadeSaude * Time.deltaTime;
        }


        // -----------------------------------------
        // LIMITAR VALORES ENTRE 0 E 100
        // -----------------------------------------

        alimentacao = Mathf.Clamp(alimentacao, 0f, 100f);
        saude = Mathf.Clamp(saude, 0f, 100f);
        felicidade = Mathf.Clamp(felicidade, 0f, 100f);
    }


    // =========================================================
    // ADICIONAR RECURSOS
    // =========================================================

    public void AdicionarComida(float valor)
    {
        alimentacao = Mathf.Clamp(alimentacao + valor, 0f, 100f);
        AtualizarUI();
    }

    public void AdicionarRemedios(float valor)
    {
        saude = Mathf.Clamp(saude + valor, 0f, 100f);
        AtualizarUI();
    }

    public void AdicionarDiversao(float valor)
    {
        felicidade = Mathf.Clamp(felicidade + valor, 0f, 100f);
        AtualizarUI();
    }


    // =========================================================
    // ALTERAR STATUS
    // =========================================================

    public void AlterarAlimentacao(float valor)
    {
        alimentacao = Mathf.Clamp(alimentacao + valor, 0f, 100f);
        AtualizarUI();
    }

    public void AlterarSaude(float valor)
    {
        saude = Mathf.Clamp(saude + valor, 0f, 100f);
        AtualizarUI();
    }

    public void AlterarFelicidade(float valor)
    {
        felicidade = Mathf.Clamp(felicidade + valor, 0f, 100f);
        AtualizarUI();
    }


    // =========================================================
    // FINALIZAÇÃO DO DIA
    // =========================================================

    public void FinalizarDia()
    {
        // A lógica dos dias pertence ao DayManager.
        // Este método permanece para compatibilidade
        // com outros sistemas que possam chamá-lo.
    }


    // =========================================================
    // ESTADOS DA COMUNIDADE
    // =========================================================

    public bool AlimentacaoCritica()
    {
        return alimentacao < 40f;
    }

    public bool SaudeCritica()
    {
        return saude < 40f;
    }

    public bool FelicidadeCritica()
    {
        return felicidade < 40f;
    }


    public bool AlimentacaoEmAtencao()
    {
        return alimentacao >= 40f && alimentacao < 70f;
    }

    public bool SaudeEmAtencao()
    {
        return saude >= 40f && saude < 70f;
    }

    public bool FelicidadeEmAtencao()
    {
        return felicidade >= 40f && felicidade < 70f;
    }


    public bool AlimentacaoEstavel()
    {
        return alimentacao >= 70f;
    }

    public bool SaudeEstavel()
    {
        return saude >= 70f;
    }

    public bool FelicidadeEstavel()
    {
        return felicidade >= 70f;
    }


    // =========================================================
    // ESTADOS SIMPLIFICADOS
    // =========================================================

    public bool ComunidadeComFome()
    {
        return alimentacao < 40f;
    }

    public bool ComunidadeDoente()
    {
        return saude < 40f;
    }

    public bool ComunidadeTriste()
    {
        return felicidade < 40f;
    }


    // =========================================================
    // INFLUÊNCIAS PARA AS FASES
    // =========================================================

    // Felicidade baixa → menos recursos aparecem.
    public float MultiplicadorSpawnRecursos()
    {
        if (FelicidadeCritica())
            return 0.5f;

        if (FelicidadeEmAtencao())
            return 0.75f;

        return 1f;
    }


    // Felicidade baixa → carrocinha chega mais rápido.
    public float MultiplicadorTempoCarrocinha()
    {
        if (FelicidadeCritica())
            return 0.7f;

        if (FelicidadeEmAtencao())
            return 0.85f;

        return 1f;
    }


    // Saúde abaixo de 70 → comunidade pode precisar
    // de medicamentos específicos.
    public bool PrecisaDeMedicamentoEspecifico()
    {
        return SaudeEmAtencao() || SaudeCritica();
    }


    // =========================================================
    // MÉDIA DA COMUNIDADE
    // =========================================================

    public float MediaComunidade()
    {
        return (alimentacao + saude + felicidade) / 3f;
    }


    // =========================================================
    // ATUALIZAÇÃO DA INTERFACE
    // =========================================================

    private void AtualizarUI()
    {
        if (barraAlimentacao != null)
            barraAlimentacao.value = alimentacao;

        if (barraSaude != null)
            barraSaude.value = saude;

        if (barraFelicidade != null)
            barraFelicidade.value = felicidade;


        if (textoAlimentacao != null)
            textoAlimentacao.text = Mathf.RoundToInt(alimentacao) + "%";

        if (textoSaude != null)
            textoSaude.text = Mathf.RoundToInt(saude) + "%";

        if (textoFelicidade != null)
            textoFelicidade.text = Mathf.RoundToInt(felicidade) + "%";
    }
}