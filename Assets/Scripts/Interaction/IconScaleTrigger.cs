using UnityEngine;

public class IconScalerTrigger : MonoBehaviour
{
    [SerializeField] public Vector3 smallScale;
    [SerializeField] public Vector3 largeScale;
    [SerializeField] public float duration;

    private Coroutine scalingCoroutine;

    private void Start()
    {
        scalingCoroutine = StartCoroutine(ScaleLoop());
    }

    private System.Collections.IEnumerator ScaleLoop()
    {
        while (true)
        {
            yield return StartCoroutine(ScaleOverTime(smallScale, largeScale, duration));
            yield return StartCoroutine(ScaleOverTime(largeScale, smallScale, duration));
        }
    }

    private System.Collections.IEnumerator ScaleOverTime(Vector3 start, Vector3 end, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            transform.localScale = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = end;
    }
}
