
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIAppear : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float displayDuration = 1f;

    private bool hasActivated = false;

    private void Start()
    {
        canvasGroup.alpha = 0f;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!hasActivated && other.CompareTag("Player"))
        {
            hasActivated = true;
            StartCoroutine(FadeSequence());
        }
    }

    private IEnumerator FadeSequence()
    {
        yield return StartCoroutine(FadeCanvasGroup(1f)); // Fade in
        yield return new WaitForSeconds(displayDuration); // Stay visible
        yield return StartCoroutine(FadeCanvasGroup(0f)); // Fade out
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class UIAppear : MonoBehaviour
//{
//    [SerializeField] private CanvasGroup canvasGroup;

//    private void Start()
//    {
//        HideUI(); // Hide at start
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            ShowUI();
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            HideUI();
//        }
//    }

//    private void ShowUI()
//    {
//        canvasGroup.alpha = 1f;
//    }

//    private void HideUI()
//    {
//        canvasGroup.alpha = 0f;
//    }
//}