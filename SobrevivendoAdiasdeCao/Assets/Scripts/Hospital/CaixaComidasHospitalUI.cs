using UnityEngine;
using System.Collections.Generic;

public class CaixaComidasHospitalUI : MonoBehaviour
{
    public static CaixaComidasHospitalUI Instance { get; private set; }

    [Header("Onde os ícones vão aparecer")]
    public RectTransform conteudo;

    [Header("Modelo do ícone")]
    public GameObject itemComidaPrefab;

    [Header("Configuração da pilha")]
    public float margem = 10f;

    [Header("Variação dos itens")]
    public float variacaoRotacao = 12f;

    private class EntradaComida
    {
        public string idComida;
        public ItemComidaUI item;
    }

    private readonly List<EntradaComida> itens = new List<EntradaComida>();

    private void Awake()
    {
        // Instância única para a cena do Hospital
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    /// <summary>
    /// Adiciona o ícone do alimento/refeição coletado na caixa da UI do Hospital.
    /// </summary>
    public void AdicionarComida(string idComida, Sprite imagemComida)
    {
        if (itemComidaPrefab == null)
        {
            Debug.LogError("CaixaComidasHospitalUI: ItemComidaPrefab não configurado no Inspector do Hospital!");
            return;
        }

        if (conteudo == null)
        {
            Debug.LogError("CaixaComidasHospitalUI: Conteudo não configurado no Inspector do Hospital!");
            return;
        }

        GameObject novoObjeto = Instantiate(itemComidaPrefab, conteudo);

        ItemComidaUI item = novoObjeto.GetComponent<ItemComidaUI>();

        if (item == null)
        {
            Debug.LogError("CaixaComidasHospitalUI: ItemComidaUI não encontrado no prefab!");
            Destroy(novoObjeto);
            return;
        }

        RectTransform rect = novoObjeto.GetComponent<RectTransform>();

        if (rect != null)
        {
            // Força escala e rotação local corretas na UI
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            float largura = conteudo.rect.width;
            float altura = conteudo.rect.height;

            float metadeLargura = rect.rect.width / 2f;
            float metadeAltura = rect.rect.height / 2f;

            // Limites de posição dentro da caixa do Hospital
            float xMin = -largura / 2f + metadeLargura + margem;
            float xMax = largura / 2f - metadeLargura - margem;

            float yMin = -altura / 2f + metadeAltura + margem;
            float yMax = altura / 2f - metadeAltura - margem;

            // Proteção contra caixas de UI muito pequenas no Hospital
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

            float rotacao = Random.Range(-variacaoRotacao, variacaoRotacao);
            rect.localRotation = Quaternion.Euler(0f, 0f, rotacao);
        }

        if (imagemComida != null)
        {
            item.ColocarImagem(imagemComida);
        }

        EntradaComida entrada = new EntradaComida
        {
            idComida = idComida,
            item = item
        };

        itens.Add(entrada);

        Debug.Log("HOSPITAL CAIXA | Comida adicionada. Total de ícones: " + itens.Count);
    }

    /// <summary>
    /// Remove o último alimento coletado na caixa do Hospital.
    /// </summary>
    public bool RemoverUmaComida()
    {
        if (itens.Count == 0)
        {
            Debug.Log("HOSPITAL CAIXA | Não há comidas para remover.");
            return false;
        }

        int ultimoIndice = itens.Count - 1;
        EntradaComida entrada = itens[ultimoIndice];
        itens.RemoveAt(ultimoIndice);

        if (entrada.item != null)
        {
            Destroy(entrada.item.gameObject);
        }

        Debug.Log("HOSPITAL CAIXA | Uma comida removida. Total de ícones: " + itens.Count);
        return true;
    }

    public bool PossuiComida(string idComida)
    {
        foreach (EntradaComida entrada in itens)
        {
            if (entrada.idComida == idComida)
                return true;
        }
        return false;
    }

    public int ObterQuantidadeComidas()
    {
        return itens.Count;
    }
}