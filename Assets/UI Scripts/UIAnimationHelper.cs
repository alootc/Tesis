using UnityEngine;
using DG.Tweening;

public class UIAnimationHelper : MonoBehaviour
{
    public float defaultScaleDuration = 0.35f;

    public Tween ScaleUp(Transform target, float toScale = 1f, float duration = -1f)
    {
        if (duration <= 0) duration = defaultScaleDuration;
        return target.DOScale(toScale, duration).SetEase(Ease.OutBack);
    }

    public Tween ScaleDown(Transform target, float toScale = 0.8f, float duration = -1f)
    {
        if (duration <= 0) duration = defaultScaleDuration;
        return target.DOScale(toScale, duration).SetEase(Ease.InBack);
    }

    public Tween MoveLocalY(Transform target, float distance, float duration = 0.3f)
    {
        return target.DOLocalMoveY(target.localPosition.y + distance, duration)
                     .SetEase(Ease.OutCubic);
    }
}
