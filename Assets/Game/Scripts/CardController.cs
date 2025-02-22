using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class CardController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerMoveHandler,
        IPointerExitHandler
    {
        public Transform handPosition; // Позиция, куда карта возвращается в руке
        private const float FollowSpeed = 10f; // Скорость следования за мышью
        private const float ReturnSpeed = 5f; // Скорость возврата в руку
        private readonly Vector2 rotationLimits = new Vector2(8f, 15f); // Ограничения по углу поворота (X, Y)
        private const float RotationSpeed = 10f;
        private static Camera UICamera => Camera.main;

        private Quaternion originalRotation;
        private Vector3 originalScale;
        private bool isDragging = false;
        private bool isPointerOver = false;

        [SerializeField] private CanvasScaler canvasScaler;

        private Vector3 direction;
        private Vector3 pos;
        private Vector3 mousePosition;
        private Vector3 eulerAngles;
        private LayoutElement layoutElement;

        void Start()
        {
            originalRotation = transform.rotation;
            originalScale = transform.localScale;
            layoutElement = GetComponent<LayoutElement>();
        }

        private IEnumerator ReturnToHand()
        {
            while (!isDragging && Vector3.Distance(transform.position, handPosition.position) > 0.01f)
            {
                transform.position =
                    Vector3.Lerp(transform.position, handPosition.position, Time.deltaTime * ReturnSpeed);
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, originalRotation, Time.deltaTime * ReturnSpeed);
                yield return null;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            layoutElement.ignoreLayout = true;
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            layoutElement.ignoreLayout = false;
            // StartCoroutine(ReturnToHand());
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                ((RectTransform) transform).anchoredPosition = ScaledPosition(Input.mousePosition);
            }
        }

        private Vector2 ScaledPosition(Vector3 mousePosition)
        {
            return canvasScaler.referenceResolution * new Vector2(mousePosition.x - Screen.width / 2f,
                       mousePosition.y - Screen.height / 2f)
                   / new Vector2(Screen.width, Screen.height);
        }

        [SerializeField] private float distance;
        [SerializeField] private Vector3 dv;

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!isDragging)
            {
                mousePosition = ScaledPosition(eventData.position);
                pos = ((RectTransform) transform).anchoredPosition;
                distance = (mousePosition - pos).magnitude;

                direction = (mousePosition - pos).normalized;
                dv = new Vector3(direction.y * rotationLimits.x, -direction.x * rotationLimits.y, 0)
                    * Mathf.Clamp(distance, 30f, 100f) / 100f;
                eulerAngles = originalRotation.eulerAngles + dv;


                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(eulerAngles),
                    Time.deltaTime * RotationSpeed);

                transform.localScale = Vector3.Lerp(transform.localScale, originalScale * 1.05f,
                    Time.deltaTime * FollowSpeed);
            }
            else
            {
                ResetCard();
            }
        }

        private void ResetCard()
        {
            transform.rotation =
                Quaternion.Lerp(transform.rotation, originalRotation, Time.deltaTime * RotationSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * FollowSpeed);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isDragging)
            {
                transform.rotation = originalRotation;
                transform.localScale = originalScale;
            }
        }
    }
}