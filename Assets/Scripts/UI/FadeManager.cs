using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Fade UI")]
    public Image fadeImage;   // 🔥 Canvas 안에 있는 전체 화면용 Image

    [Header("Fade 설정")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1f;

    [Tooltip("씬 시작 시 페이드 인 할지 여부")]
    public bool enableFadeIn = true;

    [Tooltip("씬 전환 시 페이드 아웃 할지 여부")]
    public bool enableFadeOut = true;

    private bool isFading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // 🔥 FadeManager + 자식(Canvas, Image) 전부 유지

        if (fadeImage == null)
        {
            Debug.LogError("[FadeManager] fadeImage가 연결되어 있지 않습니다!");
            return;
        }

        // 처음 시작할 때 상태 셋업
        Color c = fadeImage.color;

        if (enableFadeIn)
        {
            // 시작은 화면이 까만 상태 → 점점 투명해짐
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
        else
        {
            // 페이드 인 안 할 거면 그냥 투명 + 비활성화
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (enableFadeIn && fadeImage != null)
        {
            StartCoroutine(FadeInCoroutine());
        }
    }

    // ======================================================
    // 🔥 밖에서 호출하는 함수: 씬 전환 요청
    // ======================================================
    public void FadeToScene(string sceneName)
    {
        if (!enableFadeOut)
        {
            // 페이드 아웃 비활성화면 바로 씬 로드
            SceneManager.LoadScene(sceneName);

            // 새 씬에서도 페이드 인 하고 싶으면 여기서 다시 코루틴 호출
            if (enableFadeIn && fadeImage != null)
            {
                StartCoroutine(FadeInCoroutine());
            }
            return;
        }

        if (!isFading)
        {
            StartCoroutine(FadeOutAndLoad(sceneName));
        }
    }

    // ======================================================
    // 🔥 부드러운 Fade In
    // ======================================================
    private IEnumerator FadeInCoroutine()
    {
        isFading = true;

        fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        float alpha = 1f;
        c.a = alpha;
        fadeImage.color = c;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime / fadeInDuration;
            if (alpha < 0f) alpha = 0f;

            c.a = alpha;
            fadeImage.color = c;

            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
        isFading = false;
    }

    // ======================================================
    // 🔥 부드러운 Fade Out + Scene Load + Fade In
    // ======================================================
    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isFading = true;

        fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        float alpha = 0f;
        c.a = alpha;
        fadeImage.color = c;

        // 🔥 현재 씬에서 서서히 까매지기
        while (alpha < 1f)
        {
            alpha += Time.deltaTime / fadeOutDuration;
            if (alpha > 1f) alpha = 1f;

            c.a = alpha;
            fadeImage.color = c;

            yield return null;
        }

        // 여기서는 화면이 완전 까맣기 때문에
        // 씬을 바꿔도 "번쩍" 보일 일이 없음
        SceneManager.LoadScene(sceneName);

        // 새 씬에서 다시 서서히 밝아지기
        if (enableFadeIn)
        {
            // 새 씬의 한 프레임이 그려진 뒤 페이드 인 시작
            yield return null;
            yield return StartCoroutine(FadeInCoroutine());
        }
        else
        {
            isFading = false;
        }
    }
}
