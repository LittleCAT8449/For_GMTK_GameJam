using UnityEngine;

public class TrajectoryDrawer : MonoBehaviour
{
    [SerializeField] private bool showTrajectory = true;
    [SerializeField] private int pointCount = 30;
    [SerializeField] private float stepTime = 0.03f;

    private LineRenderer line;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.white;
        line.endColor = new Color(1, 1, 1, 0.3f);
        line.startWidth = 0.1f;
        line.endWidth = 0.02f;
        line.positionCount = pointCount;
        line.enabled = false;
    }

    private void Update()
    {
        if (!showTrajectory) return;

        Vector3[] points = new Vector3[pointCount];
        Vector2 pos = rb.position;
        Vector2 vel = rb.velocity;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i * stepTime;
            points[i] = pos + vel * t + 0.5f * Physics2D.gravity * t * t;
        }

        if (!line.enabled) line.enabled = true;
        line.SetPositions(points);
    }
}
