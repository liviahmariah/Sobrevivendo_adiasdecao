using UnityEngine;

public class HospitalManager : FaseManager
{
    public static HospitalManager Instance;

    [Header("Medicamentos conseguidos")]
    public float medicamentosConseguidos = 0f;

    [Header("Medicamentos bons para entrega")]
    public float medicamentosBonsConseguidos = 0f;

    [Header("Pontuação da Sandy")]
    public float pontosSandy = 0f;

    [Header("Estatísticas da coleta")]
    public int quantidadeItensBons = 0;
    public int quantidadeItensRuins = 0;

    public float pontosBons = 0f;
    public float pontosRuins = 0f;

    [Header("Penalidade por ser pega")]
    public float perdaAnimoAoSerPega = 10f;

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
        tipoFase = TipoFase.Hospital;

        medicamentosConseguidos = 0f;
        medicamentosBonsConseguidos = 0f;

        pontosSandy = 0f;

        quantidadeItensBons = 0;
        quantidadeItensRuins = 0;

        pontosBons = 0f;
        pontosRuins = 0f;

        base.Start();
    }

    protected override float CalcularTempoDaFase()
    {
        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "HospitalManager: CommunityManager não encontrado!"
            );

            return tempoMaximo;
        }

        float saude = CommunityManager.instance.saude;
        saude = Mathf.Clamp(saude, 0f, 100f);

        float percentual = saude / 100f;

        float tempo = Mathf.Lerp(
            tempoMinimo,
            tempoMaximo,
            percentual
        );

        return Mathf.Clamp(tempo, tempoMinimo, tempoMaximo);
    }

    // ==========================================
    // MEDICAMENTOS
    // ==========================================

    public void AdicionarMedicamento(float valor)
    {
        if (faseFinalizada || !coletaAtiva)
            return;

        medicamentosConseguidos += valor;

        if (medicamentosConseguidos < 0f)
            medicamentosConseguidos = 0f;

        Debug.Log(
            "HOSPITAL | Medicamentos conseguidos: " +
            medicamentosConseguidos
        );
    }

    public float ObterMedicamentosConseguidos()
    {
        return medicamentosConseguidos;
    }

    // ==========================================
    // REGISTRO DA COLETA
    // ==========================================

    public void RegistrarColetaMedicamento(
        float valor,
        bool medicamentoRuim,
        float multiplicadorBonusPontuacao = 1f)
    {
        if (faseFinalizada || !coletaAtiva)
            return;

        if (medicamentoRuim)
        {
            quantidadeItensRuins++;

            pontosRuins += valor;
            pontosSandy -= valor;

            medicamentosConseguidos -= valor;

            if (medicamentosConseguidos < 0f)
                medicamentosConseguidos = 0f;
        }
        else
        {
            quantidadeItensBons++;

            float pontosRecebidos = valor * multiplicadorBonusPontuacao;

            pontosBons += pontosRecebidos;
            pontosSandy += pontosRecebidos;

            medicamentosBonsConseguidos += pontosRecebidos;
            medicamentosConseguidos += valor;

            Debug.Log(
                "Medicamento bom coletado | Valor: " + valor +
                " | Bônus: " + multiplicadorBonusPontuacao +
                " | Pontuação recebida: " + pontosRecebidos
            );
        }

        Debug.Log(
            "ESTATÍSTICAS DO HOSPITAL\n" +
            "Itens bons: " + quantidadeItensBons +
            " | Pontos bons: " + pontosBons + "\n" +
            "Itens ruins: " + quantidadeItensRuins +
            " | Pontos ruins: " + pontosRuins + "\n" +
            "Saldo: " + pontosSandy + "\n" +
            "Medicamentos para entrega: " +
            medicamentosBonsConseguidos
        );
    }

    // ==========================================
    // VITÓRIA
    // ==========================================

    protected override void Vitoria()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("HOSPITAL: VITÓRIA!");
        Debug.Log(
            "Medicamentos para entrega: " +
            medicamentosBonsConseguidos
        );
        Debug.Log("=================================");

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarSaude(
                medicamentosBonsConseguidos
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
        Debug.Log("HOSPITAL: DERROTA!");
        Debug.Log(
            "Medicamentos perdidos: " +
            medicamentosConseguidos
        );

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarFelicidade(
                -perdaAnimoAoSerPega
            );
        }

        Debug.Log("Medicamentos entregues: 0");
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
                    "Voce conseguiu escapar! Os medicamentos bons " +
                    "foram entregues para a comunidade.";
            }
            else
            {
                textoRecado.text =
                    "A carrocinha pegou a Sandy! Os medicamentos " +
                    "coletados foram perdidos e a comunidade " +
                    "nao recebeu os recursos.";
            }

            textoRecado.gameObject.SetActive(true);
        }

        float medicamentosEntregues = resultadoFoiVitoria
            ? medicamentosBonsConseguidos
            : 0f;

        if (textoItensBons != null)
        {
            textoItensBons.gameObject.SetActive(true);

            textoItensBons.text =
                "Medicamentos bons: " + quantidadeItensBons +
                " | +" + pontosBons.ToString("0.##") + " pontos";
        }

        if (textoItensRuins != null)
        {
            textoItensRuins.gameObject.SetActive(true);

            textoItensRuins.text =
                "Medicamentos ruins: " + quantidadeItensRuins +
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
                "Medicamentos entregues: " +
                medicamentosEntregues.ToString("0.##");
        }
    }
}