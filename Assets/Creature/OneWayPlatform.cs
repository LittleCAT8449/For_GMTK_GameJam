using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private float dropDuration = 0.3f;

    private Collider2D platformCollider;
    private Collider2D playerCollider;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerCollider = collision.collider;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerCollider = null;
    }

    private void Update()
    {
        if (playerCollider != null && Input.GetKeyDown(KeyCode.S))
            StartCoroutine(DropPlayer(playerCollider));
    }

    private System.Collections.IEnumerator DropPlayer(Collider2D player)
    {
        Physics2D.IgnoreCollision(player, platformCollider, true);
        yield return new WaitForSeconds(dropDuration);
        Physics2D.IgnoreCollision(player, platformCollider, false);
    }
}
