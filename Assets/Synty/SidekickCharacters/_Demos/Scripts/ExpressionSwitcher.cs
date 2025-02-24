using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.Animations;
using System.Linq; // Required for LINQ extensions

public class ExpressionSwitcher : MonoBehaviour
{
    private Animator animator;

    // Define a list of expression names (will be dynamically populated)
    private List<string> expressionNames = new List<string>();

    private int currentIndex = 0;

    // Reference to the UI text to display the current emotion
    [SerializeField] private Text emotionText;

    // Reference to the UI slider to adjust transition time
    [SerializeField] private Slider transitionTimeSlider;

    private float transitionTime = 0.2f; // Default transition time

    void Start()
    {
        animator = GetComponent<Animator>();

        // Ensure the Animator is assigned
        if (animator == null)
        {
            Debug.LogWarning("No Animator component found on the GameObject.");
            return;
        }

        // Populate the expression names from the Animator states in the specified layer
        PopulateExpressionNames();

        // Sort expression names based on naming convention
        SortExpressionNames();

        // Initialize the label with the current emotion name
        UpdateEmotionText();

        // Ensure the slider is assigned
        if (transitionTimeSlider != null)
        {
            transitionTimeSlider.minValue = 0.0f;
            transitionTimeSlider.maxValue = 1.0f;
            transitionTimeSlider.value = transitionTime;

            // Read the initial value from the slider to set the transition time
            UpdateTransitionTime(transitionTimeSlider.value);

            // Add a listener to handle value changes
            transitionTimeSlider.onValueChanged.AddListener(UpdateTransitionTime);
        }
    }

    private void PopulateExpressionNames()
    {
        AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;

        if (ac != null)
        {
            string targetLayerName = "Emotion_Additive";
            foreach (AnimatorControllerLayer layer in ac.layers)
            {
                if (layer.name == targetLayerName)
                {
                    foreach (ChildAnimatorState state in layer.stateMachine.states)
                    {
                        if (!expressionNames.Contains(state.state.name))
                        {
                            expressionNames.Add(state.state.name);
                        }
                    }
                }
            }

        }
        else
        {
            Debug.LogError("AnimatorController is not found.");
        }
    }

    private void SortExpressionNames()
    {
        expressionNames.Sort((x, y) =>
        {
            // Extract the numeric part of the expression names and compare them
            string xNumber = new string(x.Where(char.IsDigit).ToArray());
            string yNumber = new string(y.Where(char.IsDigit).ToArray());

            if (int.TryParse(xNumber, out int xVal) && int.TryParse(yNumber, out int yVal))
            {
                return xVal.CompareTo(yVal);
            }

            // If parsing fails, compare the names as strings
            return x.CompareTo(y);
        });
    }

    public void CycleExpressions()
    {
        if (animator != null && expressionNames.Count > 0)
        {
            string layerName = "Emotion_Additive";
            int layerIndex = animator.GetLayerIndex(layerName);

            if (layerIndex != -1)
            {
                string expressionName = expressionNames[currentIndex];

                // Attempt to play the current expression name on the specified layer
                if (animator.HasState(layerIndex, Animator.StringToHash(expressionName)))
                {
                    animator.CrossFadeInFixedTime(expressionName, transitionTime, layerIndex);
                }
                else
                {
                    // If the state is not found, play the 'Neutral' state on the same layer
                    animator.CrossFadeInFixedTime("Neutral", transitionTime, layerIndex);
                }

                // Update the emotion text after playing the animation
                UpdateEmotionText();

                // Move to the next expression in the list
                currentIndex = (currentIndex + 1) % expressionNames.Count;
            }
        }
    }

    private void UpdateEmotionText()
    {
        if (emotionText != null && expressionNames.Count > 0)
        {
            string expressionName = expressionNames[currentIndex];
            int underscoreIndex = expressionName.IndexOf('_');
            if (underscoreIndex != -1 && underscoreIndex < expressionName.Length - 1)
            {
                emotionText.text = expressionName.Substring(underscoreIndex + 1);
            }
            else
            {
                emotionText.text = expressionName; // Fallback if no underscore found
            }
        }
    }

    private void UpdateTransitionTime(float value)
    {
        transitionTime = value;
    }
}
