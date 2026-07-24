using UnityEngine;
using UnityEngine.UI;

public class SatietyUI : UIMove
{
    [SerializeField] private Image underBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private int maxEatCount = 8;
    [SerializeField] private float fillDuration = 0.3f;

    private int eatCount;

    public void OnEat()
    {
        Open();
        StartCoroutine(DoEatAnimation());
    }

    private System.Collections.IEnumerator DoEatAnimation()
    {
        yield return new WaitForSeconds(duration);

        eatCount = Mathf.Min(eatCount + 1, maxEatCount);
        float from = fillImage != null ? fillImage.fillAmount : 0f;
        float to = eatCount / (float)maxEatCount;

        float elapsed = 0f;
        while (elapsed < fillDuration)
        {
            if (fillImage != null)
                fillImage.fillAmount = Mathf.Lerp(from, to, elapsed / fillDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (fillImage != null)
            fillImage.fillAmount = to;

        yield return new WaitForSeconds(0.5f);
        Close();
    }
}
