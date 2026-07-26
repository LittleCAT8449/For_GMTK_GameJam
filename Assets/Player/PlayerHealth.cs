using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Light2D playerLight;
    [SerializeField] private float maxRadius = 5f;
    [SerializeField] private float respawnRadius = 5f;
    [SerializeField] private Transform defaultSpawnPoint;

    public bool IsDead { get; private set; }

    private Rigidbody2D rb;
    private Vector3 respawnPosition;

    private void Awake()
    {
        if (playerLight == null) playerLight = GetComponent<Light2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        playerLight.pointLightOuterRadius = maxRadius;
        if (defaultSpawnPoint != null)
            respawnPosition = defaultSpawnPoint.position;
    }

    private void Update()
    {
        if (IsDead || playerLight == null) return;
        if (playerLight.pointLightOuterRadius <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (IsDead || playerLight == null) return;
        playerLight.pointLightOuterRadius = Mathf.Min(maxRadius, playerLight.pointLightOuterRadius + amount);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || playerLight == null) return;
        playerLight.pointLightOuterRadius = Mathf.Max(0f, playerLight.pointLightOuterRadius - amount);
    }

    public void SetRespawnPosition(Vector3 pos)
    {
        respawnPosition = pos;
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("Player died!");

        transform.position = respawnPosition;
        if (rb != null) rb.velocity = Vector2.zero;
        Respawn();
    }

    public void Kill()
    {
        Die();
    }

    public void Respawn()
    {
        IsDead = false;
        if (playerLight != null)
            playerLight.pointLightOuterRadius = respawnRadius;
    }
}
