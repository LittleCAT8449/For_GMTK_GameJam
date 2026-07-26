using System.Collections;
using System.Collections.Generic;
using Control;
using TMPro;
using UnityEngine;

public class OneTimeTip : MonoBehaviour
{
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private string tipId;
    [SerializeField] private float displayDuration = 4f;

    private static readonly HashSet<string> shown = new HashSet<string>();

    public static OneTimeTip FindByTipId(string id)
    {
        var tips = FindObjectsByType<OneTimeTip>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tip in tips)
            if (tip.tipId == id) return tip;
        return null;
    }

    private void Start()
    {
        if (tipText != null)
            tipText.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (string.IsNullOrEmpty(tipId) || shown.Contains(tipId)) return;
        shown.Add(tipId);

        if (tipText == null) return;
        TypeWriter.OnTextComplete += OnTypingDone;
        TypeWriter.Play(tipText);
    }

    private void OnTypingDone(TMP_Text text)
    {
        if (text != tipText) return;
        TypeWriter.OnTextComplete -= OnTypingDone;
        StartCoroutine(AutoHide());
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSecondsRealtime(displayDuration);
        gameObject.SetActive(false);
    }
}
