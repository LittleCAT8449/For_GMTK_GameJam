using UnityEngine;

public class MeasurementTool : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerMove playerMove;
    public Collider2D playerCollider;

    [Header("Grid")]
    public float tileSize = 1f;
    public Color gridColor = new Color(1, 1, 1, 0.12f);

    [Header("References - colors")]
    public Color jumpColor = Color.green;
    public Color jumpArcColor = Color.cyan;
    public Color wallClimbColor = new Color(1, 0.5f, 0, 1);
    public Color crouchColor = Color.yellow;
    public Color playerColor = Color.magenta;

    [Header("Ruler")]
    public Transform rulerA;
    public Transform rulerB;
    public Color rulerColor = Color.red;

    private float EffectiveGravity
    {
        get
        {
            if (playerMove == null) return 9.81f;
            return Mathf.Abs(Physics2D.gravity.y * playerMove.gravityMultiplier);
        }
    }

    private float FallGravity
    {
        get
        {
            if (playerMove == null) return 19.62f;
            return Mathf.Abs(Physics2D.gravity.y * playerMove.fallGravityMultiplier);
        }
    }

    private float JumpForce
    {
        get
        {
            if (playerMove == null) return 12f;
            return playerMove.jumpForce;
        }
    }

    private float MaxSpeed
    {
        get
        {
            if (playerMove == null) return 8f;
            return playerMove.maxSpeed;
        }
    }

    private float AirAccel
    {
        get
        {
            if (playerMove == null) return 18f;
            return playerMove.acceleration * playerMove.airControlMultiplier;
        }
    }

    private float PlayerHeight
    {
        get
        {
            if (playerCollider != null)
            {
                if (playerCollider is BoxCollider2D box) return box.size.y;
                if (playerCollider is CapsuleCollider2D cap) return cap.size.y;
            }
            return 0.96f;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            DrawAll();
    }

    private void DrawAll()
    {
        Vector3 origin = transform.position;

        Camera cam = Camera.current;
        if (cam == null) return;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        float margin = 20f;
        float minX = camPos.x - camWidth / 2f - margin;
        float maxX = camPos.x + camWidth / 2f + margin;
        float minY = camPos.y - camHeight / 2f - margin;
        float maxY = camPos.y + camHeight / 2f + margin;

        DrawGrid(minX, maxX, minY, maxY);
        DrawReferences(origin);
        DrawRuler();
    }

    private void DrawGrid(float minX, float maxX, float minY, float maxY)
    {
        Gizmos.color = gridColor;

        int startX = Mathf.FloorToInt(minX / tileSize) * (int)tileSize;
        int endX = Mathf.CeilToInt(maxX / tileSize) * (int)tileSize;
        int startY = Mathf.FloorToInt(minY / tileSize) * (int)tileSize;
        int endY = Mathf.CeilToInt(maxY / tileSize) * (int)tileSize;

        for (int x = startX; x <= endX; x += Mathf.Max(1, (int)tileSize))
            Gizmos.DrawLine(new Vector3(x, minY, 0), new Vector3(x, maxY, 0));
        for (int y = startY; y <= endY; y += Mathf.Max(1, (int)tileSize))
            Gizmos.DrawLine(new Vector3(minX, y, 0), new Vector3(maxX, y, 0));
    }

    private void DrawReferences(Vector3 origin)
    {
        float gUp = EffectiveGravity;
        float gDown = FallGravity;
        float vJ = JumpForce;
        float speed = MaxSpeed;
        float airAccel = AirAccel;
        float pHeight = PlayerHeight;

        float jumpH = vJ * vJ / (2f * gUp);
        float tUp = vJ / gUp;
        float tDown = Mathf.Sqrt(2f * jumpH / gDown);
        float airTime = tUp + tDown;

        float timeToMaxSpeed = speed / airAccel;
        float distAccel = 0.5f * airAccel * timeToMaxSpeed * timeToMaxSpeed;
        float distCruise = speed * (airTime - timeToMaxSpeed);
        float jumpDist = distAccel + distCruise;

        float secondJumpH = vJ * vJ / (2f * gDown);
        float totalDoubleJumpH = jumpH + secondJumpH;

        Gizmos.color = jumpColor;
        Vector3 jumpTop = origin + Vector3.up * jumpH;
        Gizmos.DrawLine(origin, jumpTop);
        DrawLabel(jumpTop, $"Jump {jumpH:F1}");

        Gizmos.color = jumpColor * 0.5f;
        Vector3 doubleJumpTop = origin + Vector3.up * totalDoubleJumpH;
        Gizmos.DrawLine(origin + Vector3.right * 0.5f, doubleJumpTop + Vector3.right * 0.5f);
        DrawLabel(doubleJumpTop + Vector3.right * 0.5f, $"2xJump {totalDoubleJumpH:F1}");

        Gizmos.color = jumpArcColor;
        Vector3 jumpEnd = origin + Vector3.right * jumpDist;
        Gizmos.DrawLine(origin, jumpEnd);
        DrawLabel(jumpEnd + Vector3.up * 0.3f, $"JumpDist {jumpDist:F1}");

        int segments = 30;
        Vector3 prev = origin;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float x = t * jumpDist;
            float y;
            float tPhys = t * airTime;
            if (tPhys <= tUp)
                y = vJ * tPhys - 0.5f * gUp * tPhys * tPhys;
            else
            {
                float tFall = tPhys - tUp;
                y = jumpH - 0.5f * gDown * tFall * tFall;
            }
            Vector3 p = origin + new Vector3(x, y, 0);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }

        if (playerMove != null)
        {
            Gizmos.color = wallClimbColor;
            Vector3 wallClimbEnd = origin + Vector3.up * playerMove.wallClimbSpeed;
            Gizmos.DrawLine(origin, wallClimbEnd);
            DrawLabel(wallClimbEnd + Vector3.left * 0.3f, $"WallClimb\n{playerMove.wallClimbSpeed:F1}");

            Gizmos.color = crouchColor;
            Vector3 crouchEnd = origin + Vector3.up * playerMove.crouchHeight;
            Gizmos.DrawLine(origin + Vector3.right * jumpDist * 0.3f, crouchEnd + Vector3.right * jumpDist * 0.3f);
            DrawLabel(crouchEnd + Vector3.right * (jumpDist * 0.3f + 0.3f), $"Crouch {playerMove.crouchHeight:F1}");
        }

        Gizmos.color = playerColor;
        Vector3 playerTop = origin + Vector3.up * pHeight;
        Gizmos.DrawLine(origin, playerTop);
        DrawLabel(playerTop + Vector3.right * 0.3f, $"Height {pHeight:F2}");
    }

    private void DrawRuler()
    {
        if (rulerA == null || rulerB == null) return;

        Gizmos.color = rulerColor;
        Gizmos.DrawLine(rulerA.position, rulerB.position);

        float dist = Vector3.Distance(rulerA.position, rulerB.position);
        Vector3 mid = (rulerA.position + rulerB.position) / 2f;
        DrawLabel(mid + Vector3.up * 0.3f, $"{dist:F2}");
    }

    private void DrawLabel(Vector3 position, string text)
    {
#if UNITY_EDITOR
        var style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        UnityEditor.Handles.Label(position, text, style);
#endif
    }
}
