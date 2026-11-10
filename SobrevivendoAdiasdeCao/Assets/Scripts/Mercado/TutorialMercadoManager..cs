using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialMercadoManager : MonoBehaviour
{
    public static TutorialMercadoManager Instance { get; private set; }

    [Header("UI do Tutorial")]
    public GameObject painelDica;
    public TMP_Text textoDica;
    public TMP_Text textoContinuar; // Opcional: ex: "Pressione ESPAÇO para continuar"

    [Header("Configurações")]
    public bool congelarJogo = true;
    public KeyCode teclaParaAvançar = KeyCode.Space;

    [Header("Progresso do Tutorial")]
    public bool andouOuPulou;
    public bool latiuEmNPC;
    public bool coletouItemBom;

    private int etapaAtual = 0;
    private bool aguardandoConfirmacao = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(IniciarTutorial());
    }

    private IEnumerator IniciarTutorial()
    {
        // Breve espera inicial antes da primeira instrução
        yield return new WaitForSecondsRealtime(0.2f);
        ExibirEtapa(1);
    }

    private void Update()
    {
        // Se a tela estiver congelada aguardando confirmação do jogador
        if (aguardandoConfirmacao)
        {
            if (Input.GetKeyDown(teclaParaAvançar) || Input.GetMouseButtonDown(0))
            {
                ConcluirMensagemAtual();
            }
            return;
        }

        // Validação da Etapa 1 (Movimentação livre sem congelar)
        if (etapaAtual == 1 && !andouOuPulou)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                andouOuPulou = true;
                // Exibe a próxima etapa instruindo sobre o latido
                ExibirEtapa(2);
            }
        }
    }

    /// <summary>
    /// Exibe a mensagem da etapa atual e congela o tempo do jogo.
    /// </summary>
    public void ExibirEtapa(int numeroEtapa)
    {
        etapaAtual = numeroEtapa;
        string mensagem = "";

        switch (etapaAtual)
        {
            case 1:
                mensagem = "BEM-VINDO AO MERCADO!\nUse os controles para andar e pular pela feira.";
                break;
            case 2:
                mensagem = "MECÂNICA DE LATIDO:\nLata nos clientes para assustá-los! Eles soltam itens valiosos de suas mãos.";
                break;
            case 3:
                mensagem = "CUIDADO COM OS FEIRANTES!\nFique atento à área de visão e alcance deles. Se chegar muito perto, eles atacam!";
                break;
            case 4:
                mensagem = "PONTUAÇÃO DE ITENS:\nAlimentos bons SOMAM pontos para a comunidade, mas itens estragados DIMINUEM sua pontuação!";
                break;
            case 5:
                mensagem = "TUTORIAL CONCLUÍDO!\nExplore a feira, colete mantimentos e tome cuidado com as ameaças.";
                break;
        }

        // 1. Exibe a UI
        if (painelDica != null) painelDica.SetActive(true);
        if (textoDica != null) textoDica.text = mensagem;
        if (textoContinuar != null) textoContinuar.text = "[Pressione ESPAÇO ou Clique para continuar]";

        // 2. CONGELA O TEMPO E ÁUDIOS DO JOGO
        if (congelarJogo)
        {
            Time.timeScale = 0f;
        }

        aguardandoConfirmacao = true;
    }

    private void ConcluirMensagemAtual()
    {
        aguardandoConfirmacao = false;

        // Esconde o painel
        if (painelDica != null) painelDica.SetActive(false);

        // DESCONGELA O JOGO
        Time.timeScale = 1f;

        // Se for o encerramento do tutorial, desativa o script
        if (etapaAtual == 5)
        {
            enabled = false;
        }
    }

    // Callbacks acionados pelas mecânicas do jogo:

    public void NotificarLatidoNPC()
    {
        if (etapaAtual == 2 && !latiuEmNPC)
        {
            latiuEmNPC = true;
            ExibirEtapa(3);
        }
    }

    public void NotificarColetaItem(bool ehBom)
    {
        if (ehBom && !coletouItemBom)
        {
            coletouItemBom = true;
        }

        if (etapaAtual == 3 || etapaAtual == 4)
        {
            ExibirEtapa(5);
        }
    }
}