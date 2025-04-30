using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class PlayerParameterWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI parameterNameLbl;
        [SerializeField] private TextMeshProUGUI parameterValueLbl;
        [SerializeField] private Image parameterIcon;

        public void Setup(string parameterName, string parameterValue, Sprite icon)
        {
            parameterNameLbl.text = parameterName;
            parameterValueLbl.text = parameterValue;
            parameterIcon.sprite = icon;
        }
    }
}