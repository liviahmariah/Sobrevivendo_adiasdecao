using UnityEngine;
using System.Collections.Generic;

public class CaixaBrinquedosUI : MonoBehaviour
{
    public static CaixaBrinquedosUI Instance { get; private set; }

    [Header("Onde os ícones vão aparecer")]
    public RectTransform conteudo;

    [Header("Modelo do ícone")]
    public GameObject itemBrinquedoPrefab;

    [Header("Configuração da pilha")]
    public float margem = 10f;

    [Header("Variação dos itens")]
    public float variacaoRotacao = 12f;

    private readonly List<GameObject> itens = new List<GameObject>();

    private void Awake()
    {
        // Garante a instância mais recente da cena da Praça
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public void AdicionarBrinquedo(Sprite imagemBrinquedo)
    {
        if (imagemBrinquedo == null)
        {
            Debug.LogWarning("CaixaBrinquedosUI: Imagem do brinquedo não configurada!");
            return;
        }

        if (itemBrinquedoPrefab == null || conteudo == null)
        {
            Debug.LogError("CaixaBrinquedosUI: Configure o prefab e o conteúdo no Inspector da Praça!");
            return;
        }

        GameObject novoObjeto = Instantiate(itemBrinquedoPrefab, conteudo);

        ItemComidaUI item = novoObjeto.GetComponent<ItemComidaUI>();

        if (item == null)
        {
            Debug.LogError("CaixaBrinquedosUI: ItemComidaUI não encontrado no prefab do brinquedo!");
            Destroy(novoObjeto);
            return;
        }

        RectTransform rect = novoObjeto.GetComponent<RectTransform>();

        if (rect != null)
        {
            // Reseta a escala local para garantir visibilidade na UI
            rect.localScale = Vector3.one;

            float largura = conteudo.rect.width;
            float altura = conteudo.rect.height;

            float metadeLargura = rect.rect.width / 2f;
            float metadeAltura = rect.rect.height / 2f;

            // CORREÇÃO DOS LIMITES: Subtração da margem adicionada
            float xMin = -largura / 2f + metadeLargura + margem;
            float xMax = largura / 2f - metadeLargura - margem;

            float yMin = -altura / 2f + metadeAltura + margem;
            float yMax = altura / 2f - metadeAltura - margem;

            // Proteção contra container pequeno
            if (xMin >= xMax)
            {
                xMin = -10f;
                xMax = 10f;
            }

            if (yMin >= yMax)
            {
                yMin = -10f;
                yMax = 10f;
            }

            rect.anchoredPosition = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );

            rect.localRotation = Quaternion.Euler(
                0f,
                0f,
                Random.Range(-variacaoRotacao, variacaoRotacao)
            );
        }

        item.ColocarImagem(imagemBrinquedo);
        itens.Add(novoObjeto);

        Debug.Log("PRAÇA | Brinquedo adicionado à caixa. Total: " + itens.Count);
    }

    public bool RemoverUmBrinquedo()
    {
        if (itens.Count == 0)
        {
            Debug.Log("PRAÇA | Não há brinquedos na caixa para remover.");
            return false;
        }

        int ultimoIndice = itens.Count - 1;
        GameObject objetoRemover = itens[ultimoIndice];
        itens.RemoveAt(ultimoIndice);

        if (objetoRemover != null)
        {
            Destroy(objetoRemover);
        }

        Debug.Log("PRAÇA | Brinquedo removido da caixa. Total restante: " + itens.Count);
        return true;
    }

    public int ObterQuantidadeBrinquedos()
    {
        return itens.Count;
    }
}