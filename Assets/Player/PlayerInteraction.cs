using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Ladder")]
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private float ladderHorizontalSpeed = 4f;

    [Header("Ceiling")]
    [SerializeField] private float ceilingMoveSpeed = 3f;

    private Rigidbody2D rb;
    private PlayerMove playerMove;
    private bool onLadder;
    private bool onCeiling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMove = GetComponent<PlayerMove>();
    }

    public void EnterLadder()
    {
        onLadder = true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        if (playerMove != null) playerMove.IsOnLadder = true;
    }

    public void ExitLadder()
    {
        onLadder = false;
        rb.gravityScale = 1f;
        if (playerMove != null) playerMove.IsOnLadder = false;
    }

    public void AttachCeiling()
    {
        onCeiling = true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        if (playerMove != null) playerMove.IsOnCeiling = true;
    }

    public void DetachCeiling()
    {
        onCeiling = false;
        rb.gravityScale = 1f;
        if (playerMove != null) playerMove.IsOnCeiling = false;
    }

    private void Update()
    {
        if (onCeiling && Input.GetKeyDown(KeyCode.LeftShift))
            DetachCeiling();
    }

    private void FixedUpdate()
    {
        if (onLadder)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            rb.velocity = new Vector2(h * ladderHorizontalSpeed, v * climbSpeed);
        }

        if (onCeiling)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            rb.velocity = new Vector2(h * ceilingMoveSpeed, v * ceilingMoveSpeed);
        }
    }

    private void OnDisable()
    {
        if (onLadder) ExitLadder();
        if (onCeiling) DetachCeiling();
    }
}
