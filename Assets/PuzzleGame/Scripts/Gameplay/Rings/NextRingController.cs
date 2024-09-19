using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PuzzleGame.Gameplay.Rings
{
    public class NextRingController : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IDragHandler
    {
        public event Action<NextRingController> PointerUp = delegate { };
        public event Action<NextRingController> PointerDown = delegate { };
    
        public float verticalOffset;
    
        [HideInInspector]
        public RingsController ringsController;
    
        private RectTransform rectTransform;
        private Vector2 cachedPosition;


        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            cachedPosition = rectTransform.anchoredPosition;
        }

        public void Init(RingsController ringPrefab)
        {
            RingsController ring = Instantiate(ringPrefab, transform).GetComponent<RingsController>();
            ring.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            ring.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            ring.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            ringsController = ring;
        }
    
        public void SetState(int index, int value)
        {
            ringsController.Merge(index, value);
        }
    
        public void Clear()
        {
            Destroy(gameObject);
        }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke(this);

            StartCoroutine(rectTransform.DoLocalMove(ScreenPointToAnchoredPosition(eventData.position)));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUp?.Invoke(this);

            StartCoroutine(rectTransform.DoLocalMove(cachedPosition));
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        }

        private  Vector2 ScreenPointToAnchoredPosition(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent.GetComponent<RectTransform>(),
                screenPoint,
                Camera.main, 
                out Vector2 position
            );
            
            position.y += verticalOffset;
            return position;
        }
    }
}
