using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector2 direction = Vector2.zero;
    public float speed = 1;

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.gameObject.GetComponent<PlayerController>().Damage = collision.gameObject.GetComponent<PlayerController>().Damage + 1;

        Destroy(gameObject);
    }
}
