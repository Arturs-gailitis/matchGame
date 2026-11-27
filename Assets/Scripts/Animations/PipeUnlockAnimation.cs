using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Fades pipe sections in order when a pipe obstacle is unlocked.
/// </summary>
public static class PipeUnlockAnimation
{
    private const float SectionFadeDuration = 0.3f;
    private const float SectionDelay = 0.08f;
    private const Ease FadeEase = Ease.InOutSine;

    public static async UniTask PlayAsync(IReadOnlyList<CanvasGroup> sections)
    {
        if (sections == null || sections.Count == 0)
        {
            return;
        }

        Sequence sequence = DOTween.Sequence();

        for (int index = 0; index < sections.Count; index++)
        {
            CanvasGroup section = sections[index];

            if (section == null)
            {
                continue;
            }

            Tweener fadeTween = section
                .DOFade(0f, SectionFadeDuration)
                .SetEase(FadeEase);
            _ = sequence.Insert(index * SectionDelay, fadeTween);
        }

        var completionSource = new UniTaskCompletionSource();
        sequence = sequence
            .OnComplete(() => completionSource.TrySetResult())
            .OnKill(() => completionSource.TrySetResult());
        await completionSource.Task;
    }
}
