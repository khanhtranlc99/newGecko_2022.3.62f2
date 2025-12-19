using UnityEngine;
using System.Collections;

public class FaceAnimation : MonoBehaviour
{

    [Header("Blink Settings")]
    [SerializeField] private Vector2 blinkIntervalRange = new Vector2(0.5f, 2f); // thời gian chờ giữa 2 lần chớp
    [SerializeField] private float blinkDuration = 0.12f; // thời gian đóng rồi mở (ngắn thôi)

    [Header("Tongue Settings")]
    [SerializeField] private Vector2 tongueIntervalRange = new Vector2(0.5f, 2f); // thời gian chờ giữa 2 lần lè
    [SerializeField] private float tongueOutDuration = 0.5f; // thời gian thò lưỡi ra
    [SerializeField] private float tongueHoldTime = 0.1f;     // giữ lưỡi ở ngoài
    [SerializeField] private float tongueInDuration = 0.3f;  // thời gian thu lưỡi vào

    public SkinnedMeshRenderer smr;
    public int idxTongue = -1;
    public int idxBlink = -1;

    private Coroutine blinkLoop;
    private Coroutine tongueLoop;

    public void StartAnim()
    {
        SetTongue(0f);
        SetBlink(0f);

        if (idxBlink >= 0)
            blinkLoop = StartCoroutine(AutoBlinkLoop());

        if (idxTongue >= 0)
            tongueLoop = StartCoroutine(AutoTongueLoop());
    }

    public void StopAnim()
    {
        if (blinkLoop != null) StopCoroutine(blinkLoop);
        if (tongueLoop != null) StopCoroutine(tongueLoop);

        SetTongue(0f);
        SetBlink(0f);
    }

    private void SetTongue(float percent)
    {
        if (idxTongue >= 0)
            smr.SetBlendShapeWeight(idxTongue, Mathf.Clamp(percent, 0f, 100f));
    }

    private void SetBlink(float percent)
    {
        if (idxBlink >= 0)
            smr.SetBlendShapeWeight(idxBlink, Mathf.Clamp(percent, 0f, 100f));
    }

    private IEnumerator AutoBlinkLoop()
    {
        while (true)
        {
            float wait = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
            yield return new WaitForSeconds(wait);

            yield return BlinkOnce();
        }
    }

    private IEnumerator BlinkOnce()
    {
        float half = blinkDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / half);
            SetBlink(k * 100f);
            yield return null;
        }

        // mở lại
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / half); // 0->1
            SetBlink((1f - k) * 100f);
            yield return null;
        }

        SetBlink(0f);
    }

    // ====== Auto Tongue ======
    private IEnumerator AutoTongueLoop()
    {
        while (true)
        {
            // chờ ngẫu nhiên giữa 2 lần lè lưỡi
            float wait = Random.Range(tongueIntervalRange.x, tongueIntervalRange.y);
            yield return new WaitForSeconds(wait);

            // lè 1 lần
            yield return TongueOnce();
        }
    }

    private IEnumerator TongueOnce()
    {
        float t = 0f;

        // thò lưỡi ra
        while (t < tongueOutDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / tongueOutDuration); // 0->1
            SetTongue(k * 100f);
            yield return null;
        }

        SetTongue(100f);

        // giữ ở ngoài một lúc
        yield return new WaitForSeconds(tongueHoldTime);

        // thu lưỡi vào
        t = 0f;
        while (t < tongueInDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / tongueInDuration); // 0->1
            SetTongue((1f - k) * 100f);
            yield return null;
        }

        SetTongue(0f);
    }
}
