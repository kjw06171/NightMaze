using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;
    public float fadeDuration = 1.5f;   // 페이드 인/아웃 시간

    private void Awake()
    {
        // 싱글톤 설정
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
        audioSource.volume = 0f;   // 초기 볼륨 0 → FadeIn 때 채워짐
        audioSource.clip = null;
    }

    // ============================================================
    // 🔥 씬이 로드될 때 자동으로 BGM 재생
    // ============================================================
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
        // 씬에 StageController가 있는지 찾기
        StageController sc = FindObjectOfType<StageController>();

        if (sc != null && sc.stageBGM != null)
        {
            // 새로운 스테이지 BGM 시작
            FadeIn(sc.stageBGM, 0f, sc.bgmVolume);
        }
    }

    // ============================================================
    // 🔥 BGM 즉시 재생
    // ============================================================
    public void PlayBGM(AudioClip newClip, float targetVolume = 1f)
    {
        if (newClip == null) return;

        StopAllCoroutines();

        // AudioClip 변경
        audioSource.clip = newClip;
        audioSource.volume = targetVolume;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    // ============================================================
    // 🔥 부드러운 페이드 인 재생
    // ============================================================
    public void FadeIn(AudioClip newClip, float startVolume = 0f, float targetVolume = 1f)
    {
        if (newClip == null) return;

        StopAllCoroutines();

        audioSource.clip = newClip;
        audioSource.volume = startVolume;

        if (!audioSource.isPlaying)
            audioSource.Play();

        StartCoroutine(FadeInCoroutine(targetVolume));
    }

    // ============================================================
    // 🔥 부드러운 페이드 아웃 후 정지
    // ============================================================
    public void FadeOutAndStop()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutCoroutine(0f));
    }

    // ============================================================
    // 🔽 코루틴 내부 구현
    // ============================================================
    private IEnumerator FadeOutCoroutine(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        audioSource.Stop();
    }

    private IEnumerator FadeInCoroutine(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}
