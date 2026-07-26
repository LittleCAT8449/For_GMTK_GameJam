using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Control
{
    public class TypeWriter : MonoBehaviour
    {
        [Range(0, 1)] public float speed = 0.05f;

        public bool IsTyping { get; private set; }
        private TMP_Text text;

        public static event Action<TMP_Text> OnTextComplete;

        // ===== 静态接口 =====

        /// <summary>在指定 TMP_Text 上启动打字机效果（自动查找或创建 TypeWriter 组件）</summary>
        public static TypeWriter Play(TMP_Text target)
        {
            if (target == null) return null;
            if (!target.gameObject.activeInHierarchy)
                target.gameObject.SetActive(true);
            var tw = target.GetComponent<TypeWriter>();
            if (tw == null)
                tw = target.gameObject.AddComponent<TypeWriter>();
            tw.text = target;
            tw.StartTyping();
            return tw;
        }

        /// <summary>立即完成指定 TMP_Text 上的打字机效果</summary>
        public static void Complete(TMP_Text target)
        {
            if (target == null) return;
            var tw = target.GetComponent<TypeWriter>();
            if (tw != null) tw.DoComplete();
        }

        /// <summary>指定 TMP_Text 上的打字机是否正在播放</summary>
        public static bool IsPlaying(TMP_Text target)
        {
            if (target == null) return false;
            var tw = target.GetComponent<TypeWriter>();
            return tw != null && tw.IsTyping;
        }

        // ===== 实例方法 =====

        public void StartTyping()
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[TypeWriter] 无法在 inactive 的 GameObject '{gameObject.name}' 上启动打字机，请确保其父级已激活");
                return;
            }
            if (text == null)
                gameObject.TryGetComponent(out text);
            StopAllCoroutines();
            StartCoroutine(OnTypeWriter());
        }

        public void Complete() => DoComplete();

        private void DoComplete()
        {
            StopAllCoroutines();
            IsTyping = false;
            if (text != null)
            {
                text.maxVisibleCharacters = text.text.Length;
                OnTextComplete?.Invoke(text);
            }
        }

        private IEnumerator OnTypeWriter()
        {
            if (text == null) yield break;

            IsTyping = true;
            text.ForceMeshUpdate();
            TMP_TextInfo textInfo = text.textInfo;
            int total = textInfo.characterCount;
            int current = 0;

            while (current <= total)
            {
                text.maxVisibleCharacters = current;
                current++;
                yield return new WaitForSecondsRealtime(speed);
            }

            OnTextComplete?.Invoke(text);

            yield return new WaitForSecondsRealtime(1);
            IsTyping = false;
        }
    }
}
