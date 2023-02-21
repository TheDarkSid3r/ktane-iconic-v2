using UnityEngine;
using UnityEngine.EventSystems;

public class Icon : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private Animator _animator;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            _animator.SetTrigger("Spin");
    }
}
