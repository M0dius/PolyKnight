using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{

    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetMaxHealth(int health)
    {
        if (slider == null)
        {
            Debug.LogError("Health slider is null! Please assign it in the inspector.");
            return;
        }

        slider.maxValue = health;
        slider.value = health;

        if (fill != null && gradient != null)
        {
            fill.color = gradient.Evaluate(1f);
        }
    }

    public void SetHealth(int health)
    {
        if (slider == null)
        {
            Debug.LogError("Health slider is null! Please assign it in the inspector.");
            return;
        }

        // Make sure health doesn't go below zero
        health = Mathf.Max(0, health);
        slider.value = health;

        if (fill != null && gradient != null)
        {
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
            if (slider == null)
            {
                Debug.LogError("No Slider component found! Assign the slider in the inspector.");
                return;
            }
        }

        if (fill == null && slider != null)
        {
            // Try to find fill in the children of the slider
            Image[] images = slider.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img.gameObject.name == "Fill")
                {
                    fill = img;
                    break;
                }
            }

            if (fill == null)
            {
                Debug.LogWarning("Fill image not assigned and couldn't be found in children.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
