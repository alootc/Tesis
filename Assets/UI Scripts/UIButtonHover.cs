using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UIAnimationHelper animator;
    public void Initialize(UIAnimationHelper anim)
    {
        animator = anim;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        animator?.ScaleUp(transform, 2.5f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        animator?.ScaleDown(transform, 2f);
    }
}