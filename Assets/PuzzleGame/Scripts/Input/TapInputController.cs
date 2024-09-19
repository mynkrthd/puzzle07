using UnityEngine;
using UnityEngine.EventSystems;

namespace PuzzleGame.Input
{
    public class TapInputController : InputController, 
        IPointerDownHandler, 
        IPointerUpHandler,
        IDragHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            int index = eventData.pointerCurrentRaycast.gameObject.transform.GetSiblingIndex();
            OnMove(index);
        }

        public void OnDrag(PointerEventData eventData)
        {
            GameObject current = eventData.pointerCurrentRaycast.gameObject;
            if (current == null) return;
        
            int index = current.transform.GetSiblingIndex();
            OnMove(index);
        }
    
        public void OnPointerUp(PointerEventData eventData)
        {
            OnDown();
        }
    }
}
