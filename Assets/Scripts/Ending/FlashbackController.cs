using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections;

public class FlashbackController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public Image memoryImage;

    [Header("Memory Images")]
    public Sprite[] memories;

    [Header("Timing")]
    public float fadeTime = 0.3f;
    public float imageInterval = 0.12f;

    [Header("Timeline")]
    public PlayableDirector mainDirector;   // ⭐ 메인 컷씬
    public PlayableDirector endingBTimeline;

    Playable rootPlayable;

    public void PlayFlashback()
    {
        // ⭐ 타임라인 정지 (Pause 아님!)
        rootPlayable = mainDirector.playableGraph.GetRootPlayable(0);
        rootPlayable.SetSpeed(0);

        gameObject.SetActive(true);
        StartCoroutine(FlashbackRoutine());
    }

    IEnumerator FlashbackRoutine()
    {
        // 페이드 인
        yield return StartCoroutine(Fade(0, 1));

        // 기억 이미지 훅훅
        for (int i = 0; i < memories.Length; i++)
        {
            memoryImage.sprite = memories[i];
            yield return new WaitForSecondsRealtime(imageInterval);
        }

        // 페이드 아웃
        yield return StartCoroutine(Fade(1, 0));

        gameObject.SetActive(false);

        // ⭐ 메인 컷씬은 이제 다시 안 씀 (분기)
        // 엔딩 B로 이동
        if (endingBTimeline != null)
            endingBTimeline.Play();
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
