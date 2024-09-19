using UnityEngine;
using UnityEngine.EventSystems;

namespace PuzzleGame.Input
{
    public class PointerInputController : InputController, IPointerDownHandler, IPointerClickHandler, IDragHandler
    {
        public float swipeThreshold = 20f;
        public float moveThreshold = 80f;

        Vector2 lastMoveCallback;
        bool isMoved;

        public void OnPointerDown(PointerEventData eventData)
        {
            isMoved = false;
            lastMoveCallback = eventData.position;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isMoved)
                return;

            float xDelta = eventData.position.x - eventData.pressPosition.x;
            if (xDelta > swipeThreshold)
                OnRight();
            else if (xDelta < -swipeThreshold)
                OnLeft();
            else
                OnDown();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.position.x - lastMoveCallback.x >= moveThreshold)
                OnRight();
            else if (eventData.position.x - lastMoveCallback.x <= -moveThreshold)
                OnLeft();
            else
                return;

            lastMoveCallback = eventData.position;
            isMoved = true;
        }
    }
}