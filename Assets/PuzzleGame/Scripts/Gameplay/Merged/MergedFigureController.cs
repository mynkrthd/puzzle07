using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PuzzleGame.Gameplay.Merged
{
    public class MergedFigureController : FigureController, IBeginDragHandler
    {
        bool isAnimating;
        bool isRotating;

        public override void ResetPosition()
        {
            base.ResetPosition();
            transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        public void ResetBricksRotation()
        {
            foreach (var brick in bricks)
                brick.transform.localRotation = Quaternion.identity;
        }

        void DoRotate()
        {
            isAnimating = true;
            var rotation = Quaternion.Euler(transform.localEulerAngles + Vector3.forward * -90);
            var bricksRotation = Quaternion.Euler(bricks.First().transform.localEulerAngles + Vector3.forward * 90);

            foreach (var brick in bricks.Cast<MergedBrick>())
                brick.DoRotate(bricksRotation, null);

            StartCoroutine(rectTransform.DoRotation(rotation, OnRotationComplete));
        }

        void OnRotationComplete()
        {
            isAnimating = false;
            isRotating = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Interactable) return;

            isRotating = false;
        
            targetPosition = ScreenPointToAnchoredPosition(eventData.position);
            StartCoroutine(MoveHelper.DoLerp(cachedPosition - targetPosition, offset, value => currentOffset = value));
            StartCoroutine(rectTransform.DoLocalScale(Vector3.one));
        }
    
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            isRotating = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!Interactable) return;

            if (bricks.Count > 0)
            {
                if (isRotating)
                {
                    if (isAnimating) return;

                    DoRotate();
                    OnPointerClick(eventData);
                    return;
                }

                base.OnPointerUp(eventData);
            }

            isRotating = false;
        }
    }
}
