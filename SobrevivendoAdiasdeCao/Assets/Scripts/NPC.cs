using UnityEngine;

public class NPC : MonoBehaviour
{
    public GameObject itemPrefab;

    private bool jaDropou = false;

    public void DropItem()
    {
        if (jaDropou) return;

        // posição um pouco acima do NPC
        Vector3 spawnPos = transform.position + new Vector3(0, 1f, 0);

        // cria o item
        GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

        // pega o Rigidbody do item
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // força aleatória para o lado + força pra cima
            float forcaX = Random.Range(-2f, 2f);
            float forcaY = 6f;

            rb.AddForce(new Vector2(forcaX, forcaY), ForceMode2D.Impulse);
        }

        jaDropou = true;
    }
}