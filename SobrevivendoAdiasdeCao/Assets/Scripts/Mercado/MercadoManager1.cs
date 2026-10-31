
using UnityEngine;
using System.Collections.Generic;

public class MercadoManager : FaseManager
{
    public static MercadoManager Instance;

    [Header("Comida conseguida nesta fase")]
    public float comidaConseguida = 0f;

    [Header("Pontuação da Sandy")]
    public float pontosSandy = 0f;

    [Header("Estatísticas da coleta")]
    public int quantidadeItensBons = 0;
    public int quantidadeItensRuins = 0;

    public float pontosBons = 0f;
    public float pontosRuins = 0f;

    [Header("Alimento bom disponível para entrega")]
    public float comidaBoaConseguida = 0f;

    [Header("Penalidade por ser pega")]
    public float perdaFelicidadeAoSerPega = 10f;

    private HashSet<string> tiposDeComidaConseguidos =
        new HashSet<string>();

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
        tipoFase = TipoFase.Mercado;

        comidaConseguida = 0f;
        pontosSandy = 0f;
        quantidadeItensBons = 0;
        quantidadeItensRuins = 0;
        pontosBons = 0f;
        pontosRuins = 0f;
        comidaBoaConseguida = 0f;

        tiposDeComidaConseguidos.Clear();

        base.Start();
    }

    protected override float CalcularTempoDaFase()
    {
        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "MercadoManager: CommunityManager não encontrado!"
            );

            return tempoMaximo;
        }

        float alimentacao = CommunityManager.instance.alimentacao;
        alimentacao = Mathf.Clamp(alimentacao, 0f, 100f);

        float percentual = alimentacao / 100f;

        float tempo = Mathf.Lerp(
            tempoMinimo,
            tempoMaximo,
            percentual
        );

        return Mathf.Clamp(tempo, tempoMinimo, tempoMaximo);
    }

    // ==========================================
    // COMIDA
    // ==========================================

    public void AdicionarComida(float valor)
    {
        if (faseFinalizada || !coletaAtiva)
            return;

        comidaConseguida += valor;

        if (comidaConseguida < 0f)
            comidaConseguida = 0f;

        Debug.Log(
            "MERCADO | Comida conseguida: " +
            comidaConseguida
        );
    }

    public bool RegistrarTipoComida(
        string idComida,
        Sprite imagemComida)
    {
        if (!coletaAtiva)
            return false;

        if (string.IsNullOrEmpty(idComida))
        {
            Debug.LogWarning(
                "MercadoManager: ID da comida está vazio!"
            );

            return false;
        }

        bool novoTipo = tiposDeComidaConseguidos.Add(idComida);

        if (CaixaComidasUI.Instance != null)
        {
            CaixaComidasUI.Instance.AdicionarComida(
                idComida,
                imagemComida
            );
        }

        return novoTipo;
    }

    public bool JaConseguiuComida(string idComida)
    {
        return tiposDeComidaConseguidos.Contains(idComida);
    }

    public float ObterComidaConseguida()
    {
        return comidaConseguida;
    }

    // ==========================================
    // VITÓRIA DO MERCADO
    // ==========================================

    protected override void Vitoria()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("MERCADO: VITÓRIA!");
        Debug.Log("Alimento bom para entrega: " + comidaBoaConseguida);
        Debug.Log("=================================");

        // Apenas alimentos bons são entregues.
        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarAlimentacao(
                comidaBoaConseguida
            );
        }

        base.Vitoria();
    }

    // ==========================================
    // DERROTA DO MERCADO
    // ==========================================

    public override void Derrota()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("MERCADO: DERROTA!");
        Debug.Log("Comida perdida: " + comidaConseguida);

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

        // Na derrota, nenhum alimento é entregue.
        Debug.Log("Alimento entregue: 0");
        Debug.Log("=================================");

        base.Derrota();
    }

    // ==========================================
    // REGISTRO DOS ITENS COLETADOS
    // ==========================================

    public void RegistrarColetaComida(
        float valor,
        bool comidaRuim)
    {
        if (faseFinalizada)
            return;

        if (comidaRuim)
        {
            quantidadeItensRuins++;
            pontosRuins += valor;
            pontosSandy -= valor;
        }
        else
        {
            quantidadeItensBons++;
            pontosBons += valor;
            pontosSandy += valor;

            comidaBoaConseguida += valor;
        }

        Debug.Log(
            "ESTATÍSTICAS DA COLETA\n" +
            "Itens bons: " + quantidadeItensBons +
            " | Pontos bons: " + pontosBons + "\n" +
            "Itens ruins: " + quantidadeItensRuins +
            " | Pontos ruins: " + pontosRuins + "\n" +
            "Saldo: " + pontosSandy + "\n" +
            "Alimento bom para entrega: " + comidaBoaConseguida
        );
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
                    "Voce conseguiu escapar! Os alimentos bons " +
                    "foram entregues para a comunidade.";
            }
            else
            {
                textoRecado.text =
                    "A carrocinha pegou a Sandy! Os alimentos " +
                    "coletados foram perdidos e a comunidade " +
                    "nao recebeu comida nesta missao.";
            }

            textoRecado.gameObject.SetActive(true);
        }

        float alimentoEntregue = resultadoFoiVitoria
            ? comidaBoaConseguida
            : 0f;

        if (textoItensBons != null)
        {
            textoItensBons.gameObject.SetActive(true);
            textoItensBons.text =
                "Itens bons: " + quantidadeItensBons +
                " | +" + pontosBons.ToString("0.##") + " pontos";
        }

        if (textoItensRuins != null)
        {
            textoItensRuins.gameObject.SetActive(true);
            textoItensRuins.text =
                "Itens ruins: " + quantidadeItensRuins +
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
                "Alimento entregue: " +
                alimentoEntregue.ToString("0.##");
        }
    }
}
