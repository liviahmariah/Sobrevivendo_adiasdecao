
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class OndulacaoCamera : MonoBehaviour
{
    [Header("Shader")]
    public Shader shaderOndulacao;

    [Header("Configuração")]
    public float duracao = 5f;
    public float intensidade = 0.015f;
    public float velocidade = 3f;

    private Material materialOndulacao;
    private bool efeitoAtivo;
    private Coroutine rotinaEfeito;

    private void Start()
    {
        if (shaderOndulacao == null)
            shaderOndulacao = Shader.Find("Custom/ShaderOndulacaoVisao");

        if (shaderOndulacao == null)
        {
            Debug.LogError("Shader de ondulação não encontrado!");
            return;
        }

        materialOndulacao = new Material(shaderOndulacao);

        materialOndulacao.SetFloat("_Intensidade", intensidade);
        materialOndulacao.SetFloat("_Velocidade", velocidade);
    }

    private void Update()
    {
        // Tecla temporária para testar.
        if (Input.GetKeyDown(KeyCode.K))
            AtivarOndulacao();
    }

    public void AtivarOndulacao()
    {
        if (materialOndulacao == null)
            return;

        if (rotinaEfeito != null)
            StopCoroutine(rotinaEfeito);

        rotinaEfeito = StartCoroutine(ExecutarOndulacao());
    }

    private IEnumerator ExecutarOndulacao()
    {
        efeitoAtivo = true;

        yield return new WaitForSeconds(duracao);

        efeitoAtivo = false;
        rotinaEfeito = null;
    }

    private void OnRenderImage(RenderTexture origem, RenderTexture destino)
    {
        if (materialOndulacao != null && efeitoAtivo)
            Graphics.Blit(origem, destino, materialOndulacao);
        else
            Graphics.Blit(origem, destino);
    }

    private void OnDestroy()
    {
        if (materialOndulacao != null)
            Destroy(materialOndulacao);
    }
}
