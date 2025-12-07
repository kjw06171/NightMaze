using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Restart : MonoBehaviour
{
    [Header("🔊 클릭 사운드 설정")]
    public AudioSource audioSource;
    public AudioClip clickClip;
    [Range(0f, 1f)]
    public float clickVolume = 1f;

    public void RestartGame()
    {
        // 🔊 클릭음 재생 (Pause 무시)
        PlayClickSound();

        // 🎯 씬 리셋 전까지 모든 오디오 정지 유지
        AudioListener.pause = true;

        // 🎯 타임스케일 정상화
        Time.timeScale = 1f;

        // 🔥 즉시 씬 로드하면 구 씬 오디오가 잠깐 다시 살아남 → 한 프레임 뒤에 로드!
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        yield return null; // 한 프레임 대기 → 오디오 부활 타이밍 차단

        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);

        // 씬 로드 완료될 때까지 대기
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == scene);

        // 🎯 새로운 씬이 완전히 로드된 이후에만 오디오 재개
        AudioListener.pause = false;
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.ignoreListenerPause = true; // 🔥 Pause 중에도 클릭음 들림
            audioSource.PlayOneShot(clickClip, clickVolume);
        }
    }
}
