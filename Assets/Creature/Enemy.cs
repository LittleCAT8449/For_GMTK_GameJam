using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Wander")]
    [SerializeField] private float minWanderInterval = 1.5f;
    [SerializeField] private float maxWanderInterval = 4f;
    [SerializeField] private float obstacleCheckDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 1;

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float stateTimer;
    private float attackTimer;

    private enum State { Wander, Chase, Attack }
    private State currentState;
    private State previousState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
            currentState = State.Attack;
        else if (dist <= detectionRange)
            currentState = State.Chase;
        else
            currentState = State.Wander;

        if (currentState != previousState)
        {
            if ((currentState == State.Chase || currentState == State.Attack) && previousState == State.Wander)
                OneTimeTip.FindByTipId("enemy")?.Show();
            previousState = currentState;
        }

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Wander: UpdateWander(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
        }

        rb.velocity = new Vector2(moveDirection.x * GetCurrentSpeed(), rb.velocity.y);
    }

    private void UpdateWander()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            PickNewWanderDirection();
            stateTimer = Random.Range(minWanderInterval, maxWanderInterval);
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, obstacleCheckDistance, obstacleLayer);
        if (hit.collider != null)
        {
            moveDirection = Vector2.Reflect(moveDirection, hit.normal).normalized;
        }
    }

    private void UpdateChase()
    {
        moveDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
    }

    private void UpdateAttack()
    {
        moveDirection = Vector2.zero;

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    private void Attack()
    {
        Debug.Log($"Enemy attacked player for {attackDamage} damage");
    }

    private float GetCurrentSpeed()
    {
        return currentState == State.Wander ? wanderSpeed : chaseSpeed;
    }

    private void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
