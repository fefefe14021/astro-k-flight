// Assets/Scripts/UI/UITween.cs
// Namespace: KANYE.UI

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KANYE.UI
{
    /// <summary>
    /// Lightweight, zero-dependency UI micro-animation helper.
    /// Provides smooth cubic easing, hover/click punch-scale, modal transitions, and glowing pulse effects.
    /// Uses unscaled delta time so animations continue smoothly even during pause.
    /// </summary>
    public static class UITween
    {
        public static void AttachButtonFeedback(Button button, float hoverScale = 1.035f, float clickScale = 0.94f)
        {
            if (button == null) return;

            var trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            var rt = button.GetComponent<RectTransform>();
            if (rt == null) return;

            // Pointer Enter
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((_) =>
            {
                if (button.interactable)
                    ScaleTo(button, rt, Vector3.one * hoverScale, 0.12f);
            });
            trigger.triggers.Add(enterEntry);

            // Pointer Exit
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((_) =>
            {
                ScaleTo(button, rt, Vector3.one, 0.12f);
            });
            trigger.triggers.Add(exitEntry);

            // Pointer Down (Punch)
            var downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            downEntry.callback.AddListener((_) =>
            {
                if (button.interactable)
                    ScaleTo(button, rt, Vector3.one * clickScale, 0.08f);
            });
            trigger.triggers.Add(downEntry);

            // Pointer Up
            var upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            upEntry.callback.AddListener((_) =>
            {
                if (button.interactable)
                    ScaleTo(button, rt, Vector3.one * hoverScale, 0.10f);
                else
                    ScaleTo(button, rt, Vector3.one, 0.10f);
            });
            trigger.triggers.Add(upEntry);
        }

        public static Coroutine ScaleTo(MonoBehaviour host, RectTransform target, Vector3 targetScale, float duration)
        {
            if (host == null || target == null || !host.gameObject.activeInHierarchy) return null;
            return host.StartCoroutine(ScaleRoutine(target, targetScale, duration));
        }

        private static IEnumerator ScaleRoutine(RectTransform target, Vector3 targetScale, float duration)
        {
            if (target == null) yield break;
            var start = target.localScale;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                // Ease out cubic
                var f = 1f - Mathf.Pow(1f - t, 3f);
                target.localScale = Vector3.LerpUnclamped(start, targetScale, f);
                yield return null;
            }
            if (target != null)
                target.localScale = targetScale;
        }

        public static Coroutine FadeInModal(MonoBehaviour host, GameObject modalRoot, float duration = 0.22f)
        {
            if (host == null || modalRoot == null) return null;
            var group = modalRoot.GetComponent<CanvasGroup>();
            if (group == null)
                group = modalRoot.AddComponent<CanvasGroup>();

            var rt = modalRoot.GetComponent<RectTransform>();
            return host.StartCoroutine(FadeInModalRoutine(group, rt, duration));
        }

        private static IEnumerator FadeInModalRoutine(CanvasGroup group, RectTransform rt, float duration)
        {
            if (group == null) yield break;
            group.alpha = 0f;
            var startScale = Vector3.one * 0.94f;
            if (rt != null) rt.localScale = startScale;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (group == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                // Ease out back for subtle punchy pop
                var f = 1f - Mathf.Pow(1f - t, 3f);
                var scaleF = 1f + 2.7f * Mathf.Pow(t - 1f, 3f) + 1.7f * Mathf.Pow(t - 1f, 2f);

                group.alpha = f;
                if (rt != null)
                    rt.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, scaleF);
                yield return null;
            }

            if (group != null) group.alpha = 1f;
            if (rt != null) rt.localScale = Vector3.one;
        }

        public static Coroutine PunchScale(MonoBehaviour host, RectTransform target, float punch = 1.18f, float duration = 0.2f)
        {
            if (host == null || target == null || !host.gameObject.activeInHierarchy) return null;
            return host.StartCoroutine(PunchRoutine(target, punch, duration));
        }

        private static IEnumerator PunchRoutine(RectTransform target, float punch, float duration)
        {
            if (target == null) yield break;
            var half = duration * 0.4f;
            var elapsed = 0f;
            while (elapsed < half)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                var t = elapsed / half;
                target.localScale = Vector3.Lerp(Vector3.one, Vector3.one * punch, t);
                yield return null;
            }

            var secondHalf = duration - half;
            elapsed = 0f;
            while (elapsed < secondHalf)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                var t = elapsed / secondHalf;
                var f = 1f - Mathf.Pow(1f - t, 3f);
                target.localScale = Vector3.Lerp(Vector3.one * punch, Vector3.one, f);
                yield return null;
            }

            if (target != null)
                target.localScale = Vector3.one;
        }
    }
}
