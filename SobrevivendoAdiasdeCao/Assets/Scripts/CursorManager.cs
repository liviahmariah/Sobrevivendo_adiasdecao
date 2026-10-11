
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    [Header("Texturas do cursor")]
    public Texture2D cursorTexture;
    public Texture2D cursorCliqueTexture;

    [Header("Configuração")]
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    private bool clicando = false;

    void Start()
    {
        AtualizarCursor(cursorTexture);
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        // Botão esquerdo pressionado
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            clicando = true;
            AtualizarCursor(cursorCliqueTexture);
        }

        // Botão esquerdo solto
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            clicando = false;
            AtualizarCursor(cursorTexture);
        }
    }

    void AtualizarCursor(Texture2D textura)
    {
        if (textura == null)
            return;

        Cursor.SetCursor(
            textura,
            hotspot,
            cursorMode
        );
    }

    private void OnApplicationFocus(bool foco)
    {
        if (foco)
        {
            AtualizarCursor(
                clicando ? cursorCliqueTexture : cursorTexture
            );
        }
    }
}
