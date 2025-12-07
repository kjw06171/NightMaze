using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;
    public float fadeDuration = 1.5f;

    public float CurrentVolume => audioSource.volume;   // ⭐ 현재 볼륨 가져오기

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.clip = null;
    }

    // 씬 로드 직후 자동으로 BGM 재생
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StageController sc = FindObjectOfType<StageController>();

        if (sc != null && sc.stageBGM != null)
        {
            FadeIn(sc.stageBGM, 0f, sc.bgmVolume);
        }
    }

    // ★ 즉시 재생
    public void PlayBGM(AudioClip newClip, float targetVolume = 1f)
    {
        if (newClip == null) return;

        StopAllCoroutines();

        audioSource.clip = newClip;
        audioSource.volume = targetVolume;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    // ★ 부드러운 페이드 인 (새 BGM 시작)
    public void FadeIn(AudioClip newClip, float startVolume, float targetVolume)
    {
        if (newClip == null) return;

        StopAllCoroutines();

        audioSource.clip = newClip;
        audioSource.volume = startVolume;

        if (!audioSource.isPlaying)
            audioSource.Play();

        StartCoroutine(FadeToCoroutine(targetVolume, fadeDuration));
    }

    // ★ 페이드 아웃 후 정지
    public void FadeOutAndStop()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToCoroutine(0f, fadeDuration, stopAfterFade: true));
    }

    // ★ 원하는 볼륨으로 페이드 (스토리 패널용)
    public void FadeTo(float targetVolume, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeToCoroutine(targetVolume, duration));
    }

    // 공용 코루틴
    private IEnumerator FadeToCoroutine(float targetVolume, float duration, bool stopAfterFade = false)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (stopAfterFade)
            audioSource.Stop();
    }
}
