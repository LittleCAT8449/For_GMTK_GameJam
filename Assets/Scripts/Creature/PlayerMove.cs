using UnityEngine;

namespace Creature
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerMove:MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed;
        [SerializeField, Min(0.01f)] private float timeToMaxSpeed = 0.08f;
        [SerializeField, Min(0.01f)] private float timeToStop = 0.06f;
        [SerializeField, Min(0.01f)] private float timeToReverseDirection = 0.08f;
        [SerializeField] private float jumpSpeed;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float groundCheckHorizontalInset = 0.05f;
        [SerializeField, Min(0f)] private float groundCheckIgnoreDuration = 0.08f;
        [SerializeField, Range(0f, 89f)] private float maxSlopeAngle = 45f;
        
        private Rigidbody2D rb;
        private Collider2D playerCollider;
        private float moveInput;
        private float targetMoveSpeed;
        private float currentSpeed;
        private bool jumpPressed;
        private bool jumpReleased;
        private bool isJumping;
        private bool isGrounded;
        private Vector2 groundNormal = Vector2.up;
        private float groundAngle;
        private float groundCheckEnabledTime;
        private int groundLayerMask;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            groundLayerMask = LayerMask.GetMask("Ground");
        }

        private void Update()
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        
            if (Input.GetButtonDown("Jump"))
            {
                jumpPressed = true;
            }
        
            if (Input.GetButtonUp("Jump"))
            {
                jumpReleased = true;
            }
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            Movement();
            Jump();
        }

        private void Movement()
        {
            targetMoveSpeed = moveInput * moveSpeed;

            bool canMoveOnSlope =
                isGrounded &&
                groundAngle > 0.01f &&
                groundAngle <= maxSlopeAngle;

            Vector2 slopeDirection = Vector2.right;
            float speedAlongMovement = rb.velocity.x;

            if (canMoveOnSlope)
            {
                // 与地面法线垂直，并始终以向右为正方向。
                slopeDirection = new Vector2(
                    groundNormal.y,
                    -groundNormal.x
                ).normalized;
                speedAlongMovement = Vector2.Dot(rb.velocity, slopeDirection);
            }

            float rate;
            bool hasMoveInput = Mathf.Abs(moveInput) > 0.01f;
            bool isChangingDirection =
                hasMoveInput &&
                Mathf.Abs(speedAlongMovement) > 0.01f &&
                Mathf.Sign(moveInput) != Mathf.Sign(speedAlongMovement);

            if (!hasMoveInput)
            {
                // 从最大速度减到 0 所需的时间。
                rate = moveSpeed / Mathf.Max(timeToStop, 0.01f);
            }
            else if (isChangingDirection)
            {
                // 从一个方向的最大速度切换到反方向最大速度。
                rate = moveSpeed * 2f /
                    Mathf.Max(timeToReverseDirection, 0.01f);
            }
            else
            {
                // 从静止加速到最大速度所需的时间。
                rate = moveSpeed / Mathf.Max(timeToMaxSpeed, 0.01f);
            }

            currentSpeed = Mathf.MoveTowards(
                speedAlongMovement,
                targetMoveSpeed,
                rate * Time.fixedDeltaTime
            );

            if (canMoveOnSlope)
            {
                rb.velocity = slopeDirection * currentSpeed;
            }
            else
            {
                rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
            }
        }

        private void CheckGrounded()
        {
            bool isGroundCheckTemporarilyDisabled =
                Time.fixedTime < groundCheckEnabledTime;
            bool isAscendingFromJump =
                isJumping && rb.velocity.y > 0.01f;

            if (isGroundCheckTemporarilyDisabled || isAscendingFromJump)
            {
                isGrounded = false;
                groundNormal = Vector2.up;
                groundAngle = 0f;
                return;
            }

            // 到达跳跃最高点或撞到天花板后，结束上升阶段。
            if (isJumping && rb.velocity.y <= 0.01f)
            {
                isJumping = false;
            }

            Bounds bounds = playerCollider.bounds;
            float inset = Mathf.Clamp(
                groundCheckHorizontalInset,
                0f,
                bounds.extents.x
            );

            Vector2 leftOrigin = new Vector2(
                bounds.min.x + inset,
                bounds.min.y
            );
            Vector2 centerOrigin = new Vector2(
                bounds.center.x,
                bounds.min.y
            );
            Vector2 rightOrigin = new Vector2(
                bounds.max.x - inset,
                bounds.min.y
            );

            RaycastHit2D leftHit = CastGroundRay(leftOrigin);
            RaycastHit2D centerHit = CastGroundRay(centerOrigin);
            RaycastHit2D rightHit = CastGroundRay(rightOrigin);
            RaycastHit2D hit = GetClosestGroundHit(
                leftHit,
                centerHit,
                rightHit
            );

            isGrounded = hit.collider != null;

            if (isGrounded)
            {
                groundNormal = hit.normal;
                groundAngle = Vector2.Angle(groundNormal, Vector2.up);
            }
            else
            {
                groundNormal = Vector2.up;
                groundAngle = 0f;
            }
        }

        private RaycastHit2D CastGroundRay(Vector2 origin)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                Vector2.down,
                groundCheckDistance,
                groundLayerMask
            );

            Debug.DrawRay(
                origin,
                Vector2.down * groundCheckDistance,
                hit.collider != null ? Color.green : Color.red
            );

            return hit;
        }

        private RaycastHit2D GetClosestGroundHit(
            RaycastHit2D leftHit,
            RaycastHit2D centerHit,
            RaycastHit2D rightHit
        )
        {
            RaycastHit2D closestHit = default;
            float closestDistance = float.MaxValue;

            if (leftHit.collider != null && leftHit.distance < closestDistance)
            {
                closestHit = leftHit;
                closestDistance = leftHit.distance;
            }

            if (centerHit.collider != null && centerHit.distance < closestDistance)
            {
                closestHit = centerHit;
                closestDistance = centerHit.distance;
            }

            if (rightHit.collider != null && rightHit.distance < closestDistance)
            {
                closestHit = rightHit;
            }

            return closestHit;
        }

        private void Jump()
        {
            // 刚按下：起跳一次
            if (jumpPressed && isGrounded)
            {
                isJumping = true;
                isGrounded = false;
                groundNormal = Vector2.up;
                groundAngle = 0f;
                groundCheckEnabledTime =
                    Time.fixedTime + groundCheckIgnoreDuration;

                rb.velocity = new Vector2(
                    rb.velocity.x,
                    jumpSpeed
                );
            }
            
            // 按下事件只消费一次，避免在空中按下后落地自动起跳
            jumpPressed = false;

            // 提前松开：截断当前上升速度
            if (jumpReleased)
            {
                if (rb.velocity.y > 0f)
                {
                    rb.velocity = new Vector2(
                        rb.velocity.x,
                        rb.velocity.y * jumpCutMultiplier
                    );
                }

                // 无论当前是否上升，都要消费松开事件
                jumpReleased = false;
            }
        }
    }
}
