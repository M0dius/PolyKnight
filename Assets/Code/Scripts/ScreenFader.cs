using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public float fadeSpeed = 1.5f;  // Speed of the fade
    private Image fadeImage;
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    void Awake()
    {
        // Get the Image component from the FaderPanel
        fadeImage = GetComponent<Image>();
        // Ensure the panel is initially black and fully opaque
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        DontDestroyOnLoad(gameObject); // Make the Canvas persist
    }

    void Start()
    {
        StartFadeIn(); // Start fading in when the scene loads
    }

    // Call this method to start fading in
    public void StartFadeIn()
    {
        isFadingIn = true;
        isFadingOut = false;
        StartCoroutine(FadeIn());
    }

    // Call this method to start fading out
    public void StartFadeOut()
    {
        isFadingOut = true;
        isFadingIn = false;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f); // Ensure 0 at the end
        isFadingIn = false;
    }

    IEnumerator FadeOut()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f); // Ensure 1 at the end
        isFadingOut = false;
    }

    public bool IsFading()
    {
        return isFadingIn || isFadingOut;
    }
}
