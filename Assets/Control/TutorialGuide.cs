using System.Collections;
using Control;
using TMPro;
using UnityEngine;

public class TutorialGuide : MonoBehaviour
{
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        if (guideText == null) return;
        TypeWriter.OnTextComplete += OnTypingComplete;
        TypeWriter.Play(guideText);
    }

    private void OnTypingComplete(TMP_Text text)
    {
        if (text != guideText) return;
        TypeWriter.OnTextComplete -= OnTypingComplete;
        StartCoroutine(AutoDismiss());
    }

    private IEnumerator AutoDismiss()
    {
        yield return new WaitForSecondsRealtime(displayDuration);
        gameObject.SetActive(false);
    }
}
