using UnityEngine;

public class Item : MonoBehaviour
{
    public int valor = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.Coletar(valor);
            Destroy(gameObject);
        }
    }
}