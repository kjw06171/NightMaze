using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class StoryUIFader : MonoBehaviour
{
    [Header("UI 페이드 설정")]
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 1f;
    public float showDuration = 1.5f;
    public float fadeOutDuration = 1f;

    public static bool IsStoryPlaying = false; // ESC 차단용

    private bool isPlaying = false;

    // 🔊 스토리 전용 BGM 설정
    [Header("스토리 BGM 설정 🔊")]
    public AudioSource bgmSource;              // BGM 재생용 AudioSource
    public AudioClip bgmClip;                  // 재생할 BGM 클립
    [Range(0f, 1f)]
    public float bgmTargetVolume = 1f;         // 최종 볼륨
    public float bgmFadeInDuration = 1f;       // BGM 페이드 인 시간
    public float bgmFadeOutDuration = 1f;      // BGM 페이드 아웃 시간

    void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        // BGM 기본 설정
        if (bgmSource != null)
        {
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0f;   // 시작은 항상 0에서
        }
    }

    // ===============================================================
    // 🔥 ItemPickup.cs 등에서 호출하는 함수
    // ===============================================================
    public void Play(Action onComplete = null)
    {
        StartCoroutine(PlayStorySequence(onComplete));
    }

    // ===============================================================
    // 🔥 스토리 UI 재생 코루틴
    // ===============================================================
    private IEnumerator PlayStorySequence(Action onComplete)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup이 StoryUIFader에 연결되지 않음!");
            onComplete?.Invoke();
            yield break;
        }

        isPlaying = true;
        IsStoryPlaying = true;  // ESC 차단 ON

        // UI가 화면을 막기 시작
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // 🔥 게임 멈춤
        Time.timeScale = 0f;

        // 🔊 BGM 페이드 인 시작
        if (bgmSource != null && bgmClip != null)
        {
            StartCoroutine(FadeInBGM());
        }

        // 🔥 UI 페이드 인
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }

        // 🔥 유지 시간
        yield return new WaitForSecondsRealtime(showDuration);

        // 🔥 UI 페이드 아웃
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        // UI 클릭 방지 OFF
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // 🔊 BGM 페이드 아웃 시작
        if (bgmSource != null && bgmClip != null)
        {
            StartCoroutine(FadeOutBGM());
        }

        // 🔥 게임 재개
        Time.timeScale = 1f;

        IsStoryPlaying = false;
        isPlaying = false;

        // 🔥 콜백 실행 → 이제 대화창 열림
        onComplete?.Invoke();
    }

    // ===============================================================
    // 🔊 BGM 페이드 인
    // ===============================================================
    private IEnumerator FadeInBGM()
    {
        if (bgmSource == null || bgmClip == null)
            yield break;

        bgmSource.clip = bgmClip;

        // 페이드 인 시간 0이면 바로 볼륨 셋팅 후 재생
        if (bgmFadeInDuration <= 0f)
        {
            bgmSource.volume = bgmTargetVolume;
            bgmSource.Play();
            yield break;
        }

        bgmSource.volume = 0f;
        bgmSource.Play();

        float t = 0f;
        while (t < bgmFadeInDuration)
        {
            t += Time.unscaledDeltaTime; // Time.timeScale=0이어도 진행되게
            float lerp = t / bgmFadeInDuration;
            bgmSource.volume = Mathf.Lerp(0f, bgmTargetVolume, lerp);
            yield return null;
        }

        bgmSource.volume = bgmTargetVolume;
    }

    // ===============================================================
    // 🔊 BGM 페이드 아웃
    // ===============================================================
    private IEnumerator FadeOutBGM()
    {
        if (bgmSource == null)
            yield break;

        float startVolume = bgmSource.volume;

        // 페이드아웃 시간이 0이거나 음원이 안 나오는 상태면 바로 정지
        if (bgmFadeOutDuration <= 0f || !bgmSource.isPlaying)
        {
            bgmSource.volume = 0f;
            bgmSource.Stop();
            yield break;
        }

        float t = 0f;
        while (t < bgmFadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / bgmFadeOutDuration;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, lerp);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
    }
}
