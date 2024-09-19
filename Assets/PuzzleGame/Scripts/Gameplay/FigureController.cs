using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PuzzleGame.Gameplay
{
    public class FigureController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
    {
        public float verticalOffset;
        public List<Brick> bricks = new List<Brick>();

        public event Action<FigureController> PointerUp = delegate { };
        public event Action<FigureController> PointerClick = delegate { };
        public event Action<FigureController> PointerDrag = delegate { };

        protected RectTransform rectTransform;

        protected Vector2 offset;
        protected Vector2 cachedPosition;
        Vector3 cachedScale;

        protected Vector2 targetPosition;
        protected Vector2 currentOffset;

        public bool Interactable { get; set; } = true;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            offset = new Vector2(0f, verticalOffset);
            cachedPosition = rectTransform.anchoredPosition;
            cachedScale = rectTransform.localScale;

            targetPosition = cachedPosition;
        }

        void Update()
        {
            if (bricks.Count == 0 || !Interactable) return;
        
            rectTransform.anchoredPosition = targetPosition + currentOffset;
        }

        public virtual void ResetPosition()
        {
            StopAllCoroutines();
        
            targetPosition = cachedPosition;
            currentOffset = Vector2.zero;
            rectTransform.localScale = cachedScale;
            rectTransform.anchoredPosition = cachedPosition;
        }

        public void Rotate(float rotation)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        
            foreach (var brick in bricks)
                brick.transform.localRotation = Quaternion.Euler(0f, 0f, rotation * -1);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (bricks.Count == 0) return;
            
            PointerClick.Invoke(this);
        }
    
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (bricks.Count == 0 || !Interactable) return;
            
            transform.SetAsLastSibling();
        
            targetPosition = ScreenPointToAnchoredPosition(eventData.position);
            StartCoroutine(MoveHelper.DoLerp(cachedPosition - targetPosition, offset, value => currentOffset = value));
            StartCoroutine(rectTransform.DoLocalScale(Vector3.one));
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!Interactable) return;
        
            if (bricks.Count > 0)
                PointerUp.Invoke(this);
            StopAllCoroutines();

            currentOffset = targetPosition - cachedPosition + currentOffset;
            targetPosition = cachedPosition;
            StartCoroutine(MoveHelper.DoLerp(currentOffset, Vector2.zero, value => currentOffset = value));
            StartCoroutine(rectTransform.DoLocalScale(cachedScale));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (bricks.Count == 0 || !Interactable) return;

            targetPosition = ScreenPointToAnchoredPosition(eventData.position);
            PointerDrag.Invoke(this);
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent.GetComponent<RectTransform>(),
                screenPoint,
                Camera.main, 
                out var position
            );

            float yMin = float.MaxValue;

            foreach (var brick in bricks)
            {
                Vector2 brickPosition = transform.localRotation * brick.RectTransform.anchoredPosition;
                yMin = Mathf.Min(yMin, brickPosition.y);
            }

            position.y -= yMin;

            return position;
        }
    }
}
