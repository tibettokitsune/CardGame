using System;
using System.Collections;
using Game.Scripts.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts
{
    //script for prototype only
    public class CardController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerMoveHandler,
        IPointerExitHandler
    {
        private Transform HandPosition => transform.parent; // Позиция, куда карта возвращается в руке
        private const float FollowSpeed = 10f; // Скорость следования за мышью
        private const float ReturnSpeed = 5f; // Скорость возврата в руку
        private readonly Vector2 _rotationLimits = new Vector2(8f, 15f); // Ограничения по углу поворота (X, Y)
        private const float RotationSpeed = 10f;
        private static Camera UICamera => Camera.main;

        private Quaternion _originalRotation;
        private Vector3 _originalScale;
        private bool _isDragging = false;
        private bool _isPointerOver = false;
        
        private Vector3 direction;
        private Vector3 pos;
        private Vector3 mousePosition;
        private Vector3 eulerAngles;

        
        private CanvasScaler _canvasScaler;
        private CanvasScaler CanvasScaler
        {
            get
            {
                if (_canvasScaler == null) _canvasScaler = GetComponentInParent<CanvasScaler>();
                return _canvasScaler;
            }
        }
        
        private LayoutElement _layoutElement;
        private LayoutElement LayoutElement
        {
            get
            {
                if (_layoutElement == null) _layoutElement = GetComponentInParent<LayoutElement>();
                return _layoutElement;
            }
        }

        private IEnumerator ReturnToHand()
        {
            while (!_isDragging && Vector3.Distance(transform.position, HandPosition.position) > 0.01f)
            {
                transform.position =
                    Vector3.Lerp(transform.position, HandPosition.position, Time.deltaTime * ReturnSpeed);
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, _originalRotation, Time.deltaTime * ReturnSpeed);
                yield return null;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            LayoutElement.ignoreLayout = true;
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            LayoutElement.ignoreLayout = false;
            // StartCoroutine(ReturnToHand());
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                ((RectTransform) transform).anchoredPosition = ScaledPosition(Input.mousePosition);
            }
        }

        private Vector2 ScaledPosition(Vector3 mousePosition)
        {
            return CanvasScaler.referenceResolution * new Vector2(mousePosition.x - Screen.width / 2f,
                       mousePosition.y - Screen.height / 2f)
                   / new Vector2(Screen.width, Screen.height);
        }

        [SerializeField] private float distance;
        [SerializeField] private Vector3 dv;

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                mousePosition = ScaledPosition(eventData.position);
                pos = ((RectTransform) transform).anchoredPosition;
                distance = (mousePosition - pos).magnitude;

                direction = (mousePosition - pos).normalized;
                dv = new Vector3(direction.y * _rotationLimits.x, -direction.x * _rotationLimits.y, 0)
                    * Mathf.Clamp(distance, 30f, 100f) / 100f;
                eulerAngles = _originalRotation.eulerAngles + dv;


                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(eulerAngles),
                    Time.deltaTime * RotationSpeed);

                transform.localScale = Vector3.Lerp(transform.localScale, _originalScale * 1.05f,
                    Time.deltaTime * FollowSpeed);
                
                GetComponentInParent<HandLayoutGroup>().SetLayoutHorizontal();
            }
            else
            {
                ResetCard();
            }
        }

        private void ResetCard()
        {
            transform.rotation =
                Quaternion.Lerp(transform.rotation, _originalRotation, Time.deltaTime * RotationSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.deltaTime * FollowSpeed);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                transform.rotation = _originalRotation;
                transform.localScale = _originalScale;
            }
        }

        private void ResetDefaultData()
        {
            _originalRotation = transform.rotation;
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            ResetDefaultData();
        }
    }
}