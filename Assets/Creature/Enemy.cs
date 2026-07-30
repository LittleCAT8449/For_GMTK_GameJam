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

    [Header("Audio")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip attackClip;
    [Range(0, 1)] [SerializeField] private float moveVolume = 0.8f;
    [Range(0, 1)] [SerializeField] private float attackVolume = 1f;

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float stateTimer;
    private float attackTimer;

    private AudioSource moveAudio;
    private AudioSource attackAudio;

    private enum State { Wander, Chase, Attack }
    private State currentState;
    private State previousState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();

        moveAudio = gameObject.AddComponent<AudioSource>();
        moveAudio.playOnAwake = false;
        moveAudio.loop = true;
        moveAudio.spatialBlend = 1f;

        attackAudio = gameObject.AddComponent<AudioSource>();
        attackAudio.playOnAwake = false;
        attackAudio.loop = false;
        attackAudio.spatialBlend = 1f;
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

        if (currentState != State.Attack && rb.velocity.magnitude > 0.1f)
        {
            if (moveClip == null) { Debug.LogWarning("Enemy: moveClip 未拖入"); }
            else if (moveAudio.clip != moveClip)
            {
                moveAudio.clip = moveClip;
                moveAudio.volume = moveVolume;
            }
            if (!moveAudio.isPlaying && moveClip != null) moveAudio.Play();
        }
        else
            moveAudio.Stop();
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
        if (attackClip != null)
        {
            attackAudio.clip = attackClip;
            attackAudio.volume = attackVolume;
            attackAudio.Play();
        }
        else
            Debug.LogWarning("Enemy: attackClip 未拖入");
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
