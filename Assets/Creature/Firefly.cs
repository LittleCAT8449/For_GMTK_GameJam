using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Firefly : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float breathSpeed = 1f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float detectionDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Random Direction")]
    [SerializeField] private float minChangeInterval = 2f;
    [SerializeField] private float maxChangeInterval = 5f;
    [SerializeField] private float maxTurnAngle = 60f;

    private Vector2 direction;
    private float breathTimer;
    private float changeTimer;

    private void Awake()
    {
        if (light2D == null) light2D = GetComponent<Light2D>();
        if (obstacleLayer == 0) obstacleLayer = LayerMask.GetMask("Ground");

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        changeTimer = Random.Range(minChangeInterval, maxChangeInterval);
    }

    private void Update()
    {
        UpdateBreathing();
        CheckCollision();
        UpdateRandomDirection();
        Move();
    }

    private void UpdateBreathing()
    {
        breathTimer += Time.deltaTime * breathSpeed;
        float t = (Mathf.Sin(breathTimer) + 1f) * 0.5f;
        light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }

    private void CheckCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectionDistance, obstacleLayer);
        if (hit.collider != null)
        {
            direction = Vector2.Reflect(direction, hit.normal).normalized;
        }
    }

    private void UpdateRandomDirection()
    {
        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f)
        {
            float angle = Random.Range(-maxTurnAngle, maxTurnAngle) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            direction = new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos
            ).normalized;
            changeTimer = Random.Range(minChangeInterval, maxChangeInterval);
        }
    }

    private void Move()
    {
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
    }
}
