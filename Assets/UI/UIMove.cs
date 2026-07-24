using UnityEngine;

public class UIMove : MonoBehaviour
{
    [SerializeField] private Vector3 showOffset = new Vector3(0, 100, 0);
    [SerializeField] protected float duration = 0.3f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private GameObject targetObject;

    private Transform cachedTransform;
    private GameObject Target => targetObject != null ? targetObject : gameObject;
    private Coroutine moveCoroutine;
    private Vector3 startPosition;

    private void Awake()
    {
        var target = Target;
        cachedTransform = target.transform;
        startPosition = cachedTransform.position;
    }

    public void Open()
    {
        cachedTransform.position = startPosition;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move(cachedTransform.position, startPosition + showOffset));
    }

    public void Close()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move(cachedTransform.position, startPosition));
    }

    private System.Collections.IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = curve.Evaluate(elapsed / duration);
            cachedTransform.position = Vector3.LerpUnclamped(from, to, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cachedTransform.position = to;
    }
}
