using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;


public class FinalEndingTyper : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textUI;

    [Header("Text")]
    [TextArea(3, 10)]
    public string[] lines;

    [Header("Timing")]
    public float typingSpeed = 0.05f;
    public float lineDelay = 1.5f;
    public float fadeTime = 1.5f;
    public float finalHoldTime = 3f;

    [Header("Sound")]
    public AudioSource typingAudioSource;
    public AudioClip typingSoundClip;

    [Tooltip("타이핑 사운드 재생 간격 (초)")]
    public float typingSoundInterval = 0.06f;

    [Range(0f, 1f)]
    public float typingVolume = 0.3f;

    private float typingSoundCooldown = 0f;

    public string returnSceneName = "TitleScene";


    void Start()
    {
        if (typingAudioSource != null)
        {
            typingAudioSource.loop = false;
            typingAudioSource.playOnAwake = false;
            typingAudioSource.volume = typingVolume;
        }

        StartCoroutine(PlayEnding());
    }

    IEnumerator PlayEnding()
    {
        canvasGroup.alpha = 0f;
        textUI.text = "";

        // 페이드 인
        yield return StartCoroutine(Fade(0f, 1f));

        // 타이핑
        for (int i = 0; i < lines.Length; i++)
        {
            yield return StartCoroutine(TypeLine(lines[i]));

            if (i == lines.Length - 1)
                yield return new WaitForSeconds(finalHoldTime);
            else
                yield return new WaitForSeconds(lineDelay);
        }

        // 페이드 아웃
        yield return StartCoroutine(Fade(1f, 0f));

        // ⭐ 씬 이동 (여기!)
        SceneManager.LoadScene(returnSceneName);
    }


    IEnumerator TypeLine(string line)
    {
        textUI.text = "";
        typingSoundCooldown = 0f;

        foreach (char c in line)
        {
            textUI.text += c;

            // 공백/줄바꿈 제외
            if (!char.IsWhiteSpace(c))
            {
                TryPlayTypingSound();
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        if (typingAudioSource != null)
            typingAudioSource.Stop();
    }

    void TryPlayTypingSound()
    {
        if (typingAudioSource == null || typingSoundClip == null)
            return;

        // 쿨타임 감소
        typingSoundCooldown -= typingSpeed;

        // 아직 소리 재생 중이거나 쿨타임 남았으면 패스
        if (typingAudioSource.isPlaying || typingSoundCooldown > 0f)
            return;

        typingAudioSource.PlayOneShot(typingSoundClip);
        typingSoundCooldown = typingSoundInterval;
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
