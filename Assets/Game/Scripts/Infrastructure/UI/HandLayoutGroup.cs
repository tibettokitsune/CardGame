using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Infrastructure.UI
{
    [ExecuteAlways]
    public class HandLayoutGroup : LayoutGroup
    {
        public float angle1 = -15f;
        public float angle2 = 15f;
        public float spacing = 10f;
        public float height1 = 50f;

        public override void CalculateLayoutInputHorizontal()
        {
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
            Arrange();
        }

        public override void SetLayoutVertical()
        {
            Arrange();
        }

        private void Arrange()
        {
            int childCount = transform.childCount;
            if (childCount == 0) return;

            var validChildren = new System.Collections.Generic.List<RectTransform>();
            for (int i = 0; i < childCount; i++)
            {
                RectTransform child = (RectTransform) transform.GetChild(i);
                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                if (child.gameObject.activeSelf && (layoutElement == null || !layoutElement.ignoreLayout))
                {
                    validChildren.Add(child);
                }
            }

            int validCount = validChildren.Count;
            if (validCount == 0) return;

            if (validCount == 1)
            {
                RectTransform singleChild = validChildren[0];
                singleChild.anchoredPosition = Vector2.zero;
                singleChild.rotation = Quaternion.Euler(0, 0, (angle1 + angle2) / 2f);
                return;
            }

            float totalWidth = (validCount - 1) * spacing;
            float startOffset = -totalWidth / 2f;
            float angleStep = (angle2 - angle1) / (validCount - 1);

            for (int i = 0; i < validCount; i++)
            {
                RectTransform child = validChildren[i];
                float angle = angle1 + angleStep * i;

                float normalizedPos = (validCount > 1) ? i / (float) (validCount - 1) : 0.5f;
                float yOffset = Mathf.Sin(normalizedPos * Mathf.PI) * height1;

                Vector2 position = new Vector2(startOffset + i * spacing, yOffset);

                switch (childAlignment)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.UpperRight:
                        position.y += height1 / 2;
                        break;
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                    case TextAnchor.LowerRight:
                        position.y -= height1 / 2;
                        break;
                }

                child.anchoredPosition = position;
                child.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}