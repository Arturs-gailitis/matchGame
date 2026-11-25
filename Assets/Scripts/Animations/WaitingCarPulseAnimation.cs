using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Highlights waiting cars with a subtle looping scale pulse while a power-up is waiting for a selection.
/// </summary>
public static class WaitingCarPulseAnimation
{
    private const float PulseScale = 1.08f;
    private const float PulseDuration = 0.45f;

    private static readonly Dictionary<CarInteraction, Tween> ActiveTweens = new Dictionary<CarInteraction, Tween>();
    private static readonly Dictionary<CarInteraction, Vector3> RestingScales = new Dictionary<CarInteraction, Vector3>();

    public static void Play(IReadOnlyList<CarInteraction> cars)
    {
        Stop(cars);

        if (cars == null)
        {
            return;
        }

        foreach (CarInteraction car in cars)
        {
            if (car == null)
            {
                continue;
            }

            Vector3 restingScale = car.transform.localScale;
            RestingScales[car] = restingScale;
            Tween tween = car.transform
                .DOScale(restingScale * PulseScale, PulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetLink(car.gameObject, LinkBehaviour.KillOnDestroy);

            ActiveTweens[car] = tween;
        }
    }

    public static void Stop(IReadOnlyList<CarInteraction> cars)
    {
        if (cars == null)
        {
            return;
        }

        foreach (CarInteraction car in cars)
        {
            if (car == null)
            {
                continue;
            }

            if (ActiveTweens.TryGetValue(car, out Tween tween))
            {
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }

                ActiveTweens.Remove(car);

                if (RestingScales.TryGetValue(car, out Vector3 restingScale))
                {
                    car.transform.localScale = restingScale;
                    RestingScales.Remove(car);
                }
            }
        }
    }
}
