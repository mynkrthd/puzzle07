using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    public class SortingOrderApplier : MonoBehaviour
    {
        Canvas canvas;
        GraphicRaycaster raycaster;
    
        void Awake()
        {
            canvas = GetComponent<Canvas>();

            raycaster = GetComponent<GraphicRaycaster>();
        }

        public void SetSortingOrder(int sortingOrder)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        public void Hide()
        {
            Destroy(raycaster);
            Destroy(canvas);
        }
    }
}
