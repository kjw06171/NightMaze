using System.Collections; // 코루틴 사용을 위해 필수
using UnityEngine;
using TMPro;

public class TutorialTrigger : MonoBehaviour
{
    [Header("연결할 UI 설정")]
    public GameObject tutorialUI; 
    public TMP_Text tutorialText;

    [Header("대화 내용")]
    [TextArea(3, 10)] 
    public string message = "이곳은 어두우니 조심하세요.\n천천히 이동하는 것이 좋습니다.";

    [Header("타자기 효과 설정")]
    [Tooltip("글자 나오는 속도 (작을수록 빠름)")]
    public float typingSpeed = 0.05f; 

    [Header("사운드")]
    public AudioSource audioSource;
    [Tooltip("글자가 나올 때마다 재생될 '탁' 소리")]
    public AudioClip typingSound; 
    [Tooltip("창이 열릴 때 한 번 재생될 소리 (선택)")]
    public AudioClip openSound;

    [Header("옵션")]
    public bool pauseGame = true; 
    public bool isOneTimeOnly = false;
    
    // 내부 변수
    private bool hasTriggered = false; 
    private bool isUIActive = false; 
    private bool isTyping = false; // 현재 글자가 나오는 중인가?

    private void Start()
    {
        if (tutorialUI != null) tutorialUI.SetActive(false);
    }

    private void Update()
    {
        if (isUIActive && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 1. 타이핑 중이라면 -> 즉시 완성시키기 (스킵)
                StopAllCoroutines();
                tutorialText.text = message;
                isTyping = false;
            }
            else
            {
                // 2. 타이핑이 끝났다면 -> 창 닫기
                CloseTutorial();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerFeet"))
        {
            if (isOneTimeOnly && hasTriggered) return;
            OpenTutorial();
        }
    }

    private void OpenTutorial()
    {
        if (tutorialUI != null)
        {
            isUIActive = true;
            tutorialUI.SetActive(true);

            // 게임 일시정지
            if (pauseGame) Time.timeScale = 0f;

            // 창 열리는 효과음
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            // ✨ 타자기 효과 시작
            StartCoroutine(TypeMessage());
        }
    }

    // 글자를 한 글자씩 출력하는 코루틴
    IEnumerator TypeMessage()
    {
        isTyping = true;
        tutorialText.text = ""; // 텍스트 비우기

        // 한 글자씩 루프 돌기
        foreach (char letter in message.ToCharArray())
        {
            tutorialText.text += letter; // 글자 추가

            // 🔊 타이핑 소리 재생
            if (audioSource != null && typingSound != null)
            {
                // 피치(음높이)를 살짝 랜덤으로 주면 더 자연스러운 타자 소리가 남
                audioSource.pitch = Random.Range(0.9f, 1.1f); 
                audioSource.PlayOneShot(typingSound);
            }

            // ⏳ 대기 (게임이 멈춰있어도 작동하게 Realtime 사용)
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false; // 타이핑 끝
    }

    private void CloseTutorial()
    {
        isUIActive = false;
        if (tutorialUI != null) tutorialUI.SetActive(false);

        if (pauseGame) Time.timeScale = 1f;

        if (isOneTimeOnly) hasTriggered = true;
    }
}