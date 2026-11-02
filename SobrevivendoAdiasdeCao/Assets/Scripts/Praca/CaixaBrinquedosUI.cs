
using UnityEngine;
using System.Collections.Generic;

public class CaixaBrinquedosUI : MonoBehaviour
{
    public static CaixaBrinquedosUI Instance;

    [Header("Onde os ícones vão aparecer")]
    public RectTransform conteudo;

    [Header("Modelo do ícone")]
    public GameObject itemBrinquedoPrefab;

    [Header("Configuração da pilha")]
    public float margem = 10f;

    [Header("Variação dos itens")]
    public float variacaoRotacao = 12f;

    private readonly List<GameObject> itens =
        new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    public void AdicionarBrinquedo(Sprite imagemBrinquedo)
    {
        if (imagemBrinquedo == null)
        {
            Debug.LogWarning(
                "CaixaBrinquedosUI: imagem do brinquedo não configurada!"
            );
            return;
        }

        if (itemBrinquedoPrefab == null || conteudo == null)
        {
            Debug.LogError(
                "CaixaBrinquedosUI: configure o prefab e o conteúdo!"
            );
            return;
        }

        GameObject novoObjeto = Instantiate(
            itemBrinquedoPrefab,
            conteudo
        );

        ItemComidaUI item =
            novoObjeto.GetComponent<ItemComidaUI>();

        if (item == null)
        {
            Debug.LogError(
                "CaixaBrinquedosUI: ItemComidaUI não encontrado no prefab!"
            );

            Destroy(novoObjeto);
            return;
        }

        RectTransform rect =
            novoObjeto.GetComponent<RectTransform>();

        if (rect != null)
        {
            float largura = conteudo.rect.width;
            float altura = conteudo.rect.height;

            float metadeLargura = rect.rect.width / 2f;
            float metadeAltura = rect.rect.height / 2f;

            float xMin = -largura / 2f + metadeLargura + margem;
            float xMax = largura / 2f - metadeLargura - margem;

            float yMin = -altura / 2f + metadeAltura + margem;
            float yMax = altura / 2f - metadeAltura - margem;

            if (xMin > xMax)
                xMin = xMax = 0f;

            if (yMin > yMax)
                yMin = yMax = 0f;

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

        Debug.Log(
            "PRAÇA | Brinquedo adicionado à caixa. Total: " +
            itens.Count
        );
    }

    public int ObterQuantidadeBrinquedos()
    {
        return itens.Count;
    }
}
