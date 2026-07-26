using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerSwim : MonoBehaviour
{
    [Header("Swim Movement")]
    [SerializeField] private float swimSpeed = 3f;
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float waterGravityScale = 0.3f;

    [Header("Light Decay")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private float decayDelay = 3f;
    [SerializeField] private float decayRate = 0.5f;

    private Rigidbody2D rb;
    private PlayerMove playerMove;
    private bool inWater;
    private float waterTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMove = GetComponent<PlayerMove>();
    }

    public void EnterWater()
    {
        inWater = true;
        waterTimer = 0f;
        rb.gravityScale = waterGravityScale;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        if (playerMove != null) playerMove.IsSwimming = true;
        OneTimeTip.FindByTipId("water")?.Show();
    }

    public void ExitWater()
    {
        inWater = false;
        rb.gravityScale = 1f;
        if (playerMove != null) playerMove.IsSwimming = false;
    }

    private void Update()
    {
        if (!inWater) return;
        waterTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!inWater) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        rb.velocity = new Vector2(h * swimSpeed, rb.velocity.y);

        if (v != 0f)
            rb.velocity = new Vector2(rb.velocity.x, v * verticalSpeed);

        if (waterTimer >= decayDelay && playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(decayRate * Time.fixedDeltaTime);
        }
    }

    private void OnDisable()
    {
        if (inWater) ExitWater();
    }
}
