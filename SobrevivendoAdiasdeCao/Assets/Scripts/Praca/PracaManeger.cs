using UnityEngine;

public class PracaManager : FaseManager
{
    public static new PracaManager Instance;

    [Header("Diversão conseguida nesta fase")]
    public float diversaoConseguida = 0f;

    [Header("Diversão boa para entrega")]
    public float diversaoBoaConseguida = 0f;

    [Header("Pontuação da Sandy")]
    public float pontosSandy = 0f;

    [Header("Estatísticas da coleta")]
    public int quantidadeItensBons = 0;
    public int quantidadeItensRuins = 0;

    public float pontosBons = 0f;
    public float pontosRuins = 0f;

    [Header("Penalidade por ser pega")]
    public float perdaFelicidadeAoSerPega = 10f;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Start()
    {
        tipoFase = TipoFase.Praca;

        diversaoConseguida = 0f;
        diversaoBoaConseguida = 0f;

        pontosSandy = 0f;

        quantidadeItensBons = 0;
        quantidadeItensRuins = 0;

        pontosBons = 0f;
        pontosRuins = 0f;

        Debug.Log("PRAÇA | Start iniciado.");
        Debug.Log("PRAÇA | CommunityManager: " +
            (CommunityManager.instance != null));

        base.Start();

        Debug.Log("PRAÇA | Fase ativa: " + faseAtiva);
        Debug.Log("PRAÇA | Coleta ativa: " + coletaAtiva);
        Debug.Log("PRAÇA | Tempo restante: " + ObterTempoRestante());
    }

    protected override float CalcularTempoDaFase()
    {
        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "PracaManager: CommunityManager não encontrado!"
            );

            return tempoMaximo;
        }

        float felicidade = Mathf.Clamp(
            CommunityManager.instance.felicidade,
            0f,
            100f
        );

        float percentual = felicidade / 100f;

        float tempo = Mathf.Lerp(
            tempoMinimo,
            tempoMaximo,
            percentual
        );

        Debug.Log(
            "PRAÇA | Felicidade: " + felicidade +
            " | Tempo calculado: " + tempo
        );

        return tempo;
    }

    // ==========================================
    // DIVERSÃO
    // ==========================================

    public void AdicionarDiversao(float valor)
    {
        if (faseFinalizada || !coletaAtiva)
            return;

        diversaoConseguida += valor;

        if (diversaoConseguida < 0f)
        {
            diversaoConseguida = 0f;
        }

        if (valor >= 0f)
        {
            quantidadeItensBons++;
            pontosBons += valor;
            diversaoBoaConseguida += valor;
            pontosSandy += valor;
        }
        else
        {
            quantidadeItensRuins++;
            float perda = Mathf.Abs(valor);
            pontosRuins += perda;
            pontosSandy -= perda;
        }

        Debug.Log(
            "PRAÇA | Diversão conseguida: " +
            diversaoConseguida
        );

        if (FindFirstObjectByType<PontuacaoSandyUI>() != null)
        {
            FindFirstObjectByType<PontuacaoSandyUI>()
                .AtualizarPontuacao();
        }
    }

    public float ObterDiversaoConseguida()
    {
        return diversaoConseguida;
    }

    // ==========================================
    // REGISTRO DA COLETA (PADRÃO HOSPITAL)
    // ==========================================

    public void RegistrarColetaDiversao(
        float valor,
        bool itemRuim,
        float multiplicadorBonusPontuacao = 1f)
    {
        if (faseFinalizada || !coletaAtiva)
            return;

        if (itemRuim)
        {
            quantidadeItensRuins++;

            pontosRuins += valor;
            pontosSandy -= valor;

            diversaoConseguida -= valor;

            if (diversaoConseguida < 0f)
                diversaoConseguida = 0f;
        }
        else
        {
            quantidadeItensBons++;

            float pontosRecebidos = valor * multiplicadorBonusPontuacao;

            pontosBons += pontosRecebidos;
            pontosSandy += pontosRecebidos;

            diversaoBoaConseguida += pontosRecebidos;
            diversaoConseguida += valor;
        }

        if (FindFirstObjectByType<PontuacaoSandyUI>() != null)
        {
            FindFirstObjectByType<PontuacaoSandyUI>()
                .AtualizarPontuacao();
        }
    }

    // ==========================================
    // VITÓRIA
    // ==========================================

    protected override void Vitoria()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("PRAÇA: VITÓRIA!");
        Debug.Log(
            "Diversão conseguida: " +
            diversaoBoaConseguida
        );
        Debug.Log("=================================");

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarFelicidade(
                diversaoBoaConseguida
            );
        }

        base.Vitoria();
    }

    // ==========================================
    // DERROTA
    // ==========================================

    public override void Derrota()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("PRAÇA: DERROTA!");
        Debug.Log(
            "Diversão perdida: " +
            diversaoConseguida
        );

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarFelicidade(
                -perdaFelicidadeAoSerPega
            );

            Debug.Log(
                "Felicidade perdida: " +
                perdaFelicidadeAoSerPega
            );
        }

        Debug.Log("Diversão entregue: 0");
        Debug.Log("=================================");

        base.Derrota();
    }

    // ==========================================
    // TELA DE RESULTADO
    // ==========================================

    protected override void AtualizarEstatisticasResultado()
    {
        base.AtualizarEstatisticasResultado();

        if (textoRecado != null)
        {
            if (resultadoFoiVitoria)
            {
                textoRecado.text =
                    "Voce conseguiu escapar! A diversao conseguida " +
                    "aumentou a felicidade da comunidade.";
            }
            else
            {
                textoRecado.text =
                    "A carrocinha pegou a Sandy! A diversao " +
                    "conseguida foi perdida e a comunidade " +
                    "ficou desanimada.";
            }

            textoRecado.gameObject.SetActive(true);
        }

        float diversaoEntregue = resultadoFoiVitoria
            ? diversaoBoaConseguida
            : 0f;

        if (textoItensBons != null)
        {
            textoItensBons.gameObject.SetActive(true);

            textoItensBons.text =
                "Atividades boas: " + quantidadeItensBons +
                " | +" + pontosBons.ToString("0.##") + " pontos";
        }

        if (textoItensRuins != null)
        {
            textoItensRuins.gameObject.SetActive(true);

            textoItensRuins.text =
                "Perigos/itens ruins: " + quantidadeItensRuins +
                " | -" + pontosRuins.ToString("0.##") + " pontos";
        }

        if (textoSaldoFinal != null)
        {
            textoSaldoFinal.gameObject.SetActive(true);

            textoSaldoFinal.text =
                "Saldo final: " + pontosSandy.ToString("0.##") +
                " pontos";
        }

        if (textoEntregaComunidade != null)
        {
            textoEntregaComunidade.gameObject.SetActive(true);

            textoEntregaComunidade.text =
                "Felicidade entregue: " +
                diversaoEntregue.ToString("0.##");
        }
    }
}