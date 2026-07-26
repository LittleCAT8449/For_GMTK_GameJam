using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _acceleration = 60f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float _airControlMultiplier = 0.3f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private int maxJumps = 2;

    [Header("Gravity")]
    [SerializeField] private float _gravityMultiplier = 1f;
    [SerializeField] private float _fallGravityMultiplier = 2f;

    [Header("Crouch")]
    [SerializeField] private float _crouchHeight = 0.6f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Header("Wall Climb")]
    [SerializeField] private float _wallClimbSpeed = 3f;
    [SerializeField] private float wallSlideSpeed = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    public float maxSpeed => _maxSpeed;
    public float acceleration => _acceleration;
    public float airControlMultiplier => _airControlMultiplier;
    public float jumpForce => _jumpForce;
    public float gravityMultiplier => _gravityMultiplier;
    public float fallGravityMultiplier => _fallGravityMultiplier;
    public float crouchHeight => _crouchHeight;
    public float wallClimbSpeed => _wallClimbSpeed;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool isGrounded;
    private bool isCrouching;
    private bool isWallClimbing;
    private bool isTouchingWall;
    private bool isTouchingWallLeft;
    private Vector2 originalSize;
    private Vector2 originalOffset;
    private bool facingRight = true;
    private bool jumpPressed;
    private float wallClimbInput;
    private int jumpCount;

    public bool IsSwimming { get; set; }
    public bool IsOnLadder { get; set; }
    public bool IsOnCeiling { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (col is BoxCollider2D box)
        {
            originalSize = box.size;
            originalOffset = box.offset;
        }
        else if (col is CapsuleCollider2D capsule)
        {
            originalSize = capsule.size;
            originalOffset = capsule.offset;
        }
    }

    private void Update()
    {
        if (IsSwimming || IsOnLadder || IsOnCeiling) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        CheckGround();
        CheckWall();

        if (isGrounded)
            jumpCount = 0;

        HandleCrouch();
        HandleWallClimb();

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;

        FlipSprite();
    }

    private void FixedUpdate()
    {
        if (IsSwimming || IsOnLadder || IsOnCeiling) return;

        if (jumpPressed)
        {
            HandleJump();
            jumpPressed = false;
        }

        HandleWallClimbVelocity();
        HandleMovement();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        if (isWallClimbing) return;

        float targetSpeed = moveInput * _maxSpeed;
        if (isCrouching) targetSpeed *= crouchSpeedMultiplier;

        float accel = isGrounded ? _acceleration : _acceleration * _airControlMultiplier;
        float decel = isGrounded ? deceleration : deceleration * _airControlMultiplier;

        if (moveInput != 0)
        {
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, targetSpeed, accel * Time.fixedDeltaTime),
                rb.velocity.y
            );
        }
        else
        {
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, 0, decel * Time.fixedDeltaTime),
                rb.velocity.y
            );
        }
    }

    private void HandleJump()
    {
        if (isCrouching)
        {
            isCrouching = false;
            SetColliderSize(originalSize, originalOffset);
        }

        if (isWallClimbing)
        {
            float dir = isTouchingWallLeft ? 1f : -1f;
            rb.velocity = new Vector2(dir * _jumpForce * 0.6f, _jumpForce);
            isWallClimbing = false;
            jumpCount = 1;
            return;
        }

        if (isGrounded)
        {
            rb.velocity = new Vector2(moveInput * _maxSpeed, _jumpForce);
            jumpCount = 1;
        }
        else if (jumpCount < maxJumps)
        {
            rb.velocity = new Vector2(moveInput * _maxSpeed, _jumpForce);
            jumpCount++;
        }
    }

    private void HandleCrouch()
    {
        bool crouchInput = Input.GetAxisRaw("Vertical") < -0.5f;

        if (crouchInput && isGrounded && !isCrouching)
        {
            isCrouching = true;
            float deltaY = (originalSize.y - _crouchHeight) * 0.5f;
            Vector2 newOffset = new Vector2(originalOffset.x, originalOffset.y - deltaY);
            SetColliderSize(new Vector2(originalSize.x, _crouchHeight), newOffset);
        }
        else if (!crouchInput && isCrouching)
        {
            isCrouching = false;
            SetColliderSize(originalSize, originalOffset);
        }
    }

    private void HandleWallClimb()
    {
        if (isGrounded)
        {
            isWallClimbing = false;
            return;
        }

        if (isTouchingWall && Input.GetAxisRaw("Vertical") > 0.5f)
        {
            bool pressingTowardWall = (isTouchingWallLeft && moveInput < 0) || (!isTouchingWallLeft && moveInput > 0);
            if (pressingTowardWall || moveInput == 0)
                isWallClimbing = true;
        }

        if (isWallClimbing)
        {
            bool pressingAway = (isTouchingWallLeft && moveInput > 0) || (!isTouchingWallLeft && moveInput < 0);

            if (!isTouchingWall || pressingAway)
            {
                isWallClimbing = false;
                wallClimbInput = 0;
            }
            else
            {
                wallClimbInput = Input.GetAxisRaw("Vertical");
            }
        }
        else
        {
            wallClimbInput = 0;
        }
    }

    private void HandleWallClimbVelocity()
    {
        if (!isWallClimbing) return;

        if (wallClimbInput > 0.5f)
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, _wallClimbSpeed);
        else if (wallClimbInput < -0.5f)
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, -wallClimbSpeed);
        else
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, -wallSlideSpeed);
    }

    private void ApplyGravity()
    {
        if (isWallClimbing) return;

        float multiplier = rb.velocity.y < 0 ? _fallGravityMultiplier : _gravityMultiplier;
        rb.velocity += Vector2.up * (Physics2D.gravity.y * multiplier * Time.fixedDeltaTime);
    }

    private void CheckGround()
    {
        ContactPoint2D[] contacts = new ContactPoint2D[4];
        int count = rb.GetContacts(contacts);
        isGrounded = false;

        for (int i = 0; i < count; i++)
        {
            if (contacts[i].normal.y > 0.5f && ((1 << contacts[i].collider.gameObject.layer) & groundLayer.value) != 0)
            {
                isGrounded = true;
                break;
            }
        }

        if (!isGrounded && groundCheck != null)
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
        }
    }

    private void CheckWall()
    {
        if (isGrounded)
        {
            isTouchingWall = false;
            return;
        }

        ContactPoint2D[] contacts = new ContactPoint2D[4];
        int count = rb.GetContacts(contacts);
        isTouchingWall = false;
        isTouchingWallLeft = false;

        for (int i = 0; i < count; i++)
        {
            if (Mathf.Abs(contacts[i].normal.x) > 0.7f && Mathf.Abs(contacts[i].normal.y) < 0.7f)
            {
                if (((1 << contacts[i].collider.gameObject.layer) & groundLayer.value) != 0)
                {
                    isTouchingWall = true;
                    isTouchingWallLeft = contacts[i].normal.x > 0;
                    break;
                }
            }
        }
    }

    private void FlipSprite()
    {
        if (moveInput > 0 && !facingRight)
        {
            facingRight = true;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (moveInput < 0 && facingRight)
        {
            facingRight = false;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void SetColliderSize(Vector2 size, Vector2 offset)
    {
        if (col is BoxCollider2D box)
        {
            box.size = size;
            box.offset = offset;
        }
        else if (col is CapsuleCollider2D capsule)
        {
            capsule.size = size;
            capsule.offset = offset;
        }
    }
}
