using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollThreshold = 0.001f;
    public float inputDelay;               // Seconds before input is allowed
    public Image fadeImage;                     // UI Image used for fade to black
    public float fadeDuration;

    private bool returning = false;
    private float timeElapsed = 0f;

    void Start()
    {
        // Force start scroll at top
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        // Ensure fade image is fully transparent at start
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (!returning)
        {
            // Allow input only after delay
            if (timeElapsed >= inputDelay && Input.anyKeyDown)
            {
                StartCoroutine(FadeAndLoadScene());
            }

            // Allow auto-return when scroll reaches bottom AFTER a short delay
            if (timeElapsed >= 1.5f && scrollRect != null &&
                scrollRect.verticalNormalizedPosition <= scrollThreshold)
            {
                StartCoroutine(FadeAndLoadScene());
            }
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        returning = true;

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
        }

        SceneManager.LoadScene("_MainMenu");
    }
}