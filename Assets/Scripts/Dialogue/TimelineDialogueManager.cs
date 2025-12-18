using UnityEngine;
using TMPro; // TextMeshProUGUI를 사용하기 위해 필요
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement; 

public class TimelineDialogueManager : MonoBehaviour
{
    public static TimelineDialogueManager Instance { get; private set; }

    // UI 요소 (기존 유지)
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Image speakerPortrait; // 초상화 이미지
    public GameObject narrationPanel; // 내레이션 UI 패널 (내레이션 전용 UI)
    public TextMeshProUGUI narrationText; // 내레이션 텍스트 (대사) 

    // 🔊 오디오 요소 (기존 유지)
    [Header("오디오 설정")]
    public AudioSource typingAudioSource;
    public AudioClip typingSoundClip;
    
    [Tooltip("사운드를 얼마나 자주 재생할지 결정하는 간격(초)")]
    public float typingSoundInterval = 0.05f; 
    [Range(0f, 1f)]
    public float defaultTypingVolume = 0.5f;
    private float typingSoundCooldown = 0f;

    // 상태 변수 (기존 유지)
    private DialogueSO currentDialogueData;
    private int currentSentenceIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    public Action onDialogueEndCallback;

    void Awake()
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

        if (dialoguePanel == null || dialogueText == null || speakerNameText == null || speakerPortrait == null)
        {
            Debug.LogError("DialogueManager UI 연결 안 됨!");
            gameObject.SetActive(false);
            return;
        }
        
        // 🔊 오디오 설정 초기화
        if (typingAudioSource != null)
        {
            typingAudioSource.volume = defaultTypingVolume;
            typingAudioSource.loop = false;
            typingAudioSource.playOnAwake = false;
        }

        dialoguePanel.SetActive(false);
        narrationPanel.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive)
        {
            // E를 눌렀을 때 대화 진행 (또는 마우스 클릭)
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                HandleAdvanceDialogue();
        }
    }

    // 대화 진행
    private void HandleAdvanceDialogue()
    {
        // 1. E를 눌렀는데 현재 타이핑 중인 경우 (스킵)
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            isTyping = false;
            
            // 🔊 오디오 중지 추가
            if (typingAudioSource != null)
                typingAudioSource.Stop(); 
            
            var currentSentence = currentDialogueData.dialogueSentences[currentSentenceIndex];
            
            // 텍스트를 즉시 전체 표시
            if (string.IsNullOrEmpty(currentSentence.speakerName))
                narrationText.text = currentSentence.sentence;
            else
                dialogueText.text = currentSentence.sentence;
            
            return; 
        }

        // 2. 타이핑이 완료되었거나 이미 스킵된 상태라면 다음 문장으로 진행
        currentSentenceIndex++;

        if (currentSentenceIndex < currentDialogueData.SentenceCount)
        {
            DisplayCurrentSentence();
        }
        else
        {
            // 모든 문장이 끝났을 때 EndDialogue 호출
            EndDialogue();
        }
    }

    // 대화 시작 (기존 유지)
    public void StartDialogue(DialogueSO dialogueData, Action onEnd = null)
    {
        if (isDialogueActive) return;

        if (dialogueData == null || dialogueData.SentenceCount == 0)
        {
            Debug.LogError("DialogueSO 데이터 없음!");
            return;
        }

        onDialogueEndCallback = onEnd;
        currentDialogueData = dialogueData;
        currentSentenceIndex = 0;
        isDialogueActive = true;

        DisplayCurrentSentence();
        
        Time.timeScale = 0f;
    }

    // 문장 표시
   private void DisplayCurrentSentence()
    {
        if (currentSentenceIndex >= currentDialogueData.SentenceCount)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogueData.dialogueSentences[currentSentenceIndex];

        // UI 토글 로직
        bool isNarration = string.IsNullOrEmpty(line.speakerName);
        
        narrationPanel.SetActive(isNarration);
        dialoguePanel.SetActive(!isNarration); 

        TextMeshProUGUI targetText;

        if (isNarration)
        {
            targetText = narrationText;
            // ⭐️ 내레이션 모드일 때도 오디오를 포함하는 통합 함수 호출
            StartTypingWithAudio(targetText, line.sentence);
        }
        else
        {
            targetText = dialogueText;
            speakerNameText.text = line.speakerName;
            
            // 초상화 설정
            if (line.portrait != null)
            {
                speakerPortrait.sprite = line.portrait;
                speakerPortrait.gameObject.SetActive(true);
            }
            else
            {
                speakerPortrait.gameObject.SetActive(false);
            }
            
            // ⭐️ 캐릭터 대사 모드일 때 오디오를 포함하는 통합 함수 호출
            StartTypingWithAudio(targetText, line.sentence);
        }
    }


    // ------------------------------------------------------------
    // 🔊 통합 타이핑 시작 함수 (내레이션/캐릭터 공통)
    // ------------------------------------------------------------

    // StartTypingWithAudio 함수
    private void StartTypingWithAudio(TextMeshProUGUI targetText, string sentence)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        // 💡 모든 타이핑은 이제 오디오 로직을 포함한 TypeSentenceWithAudio 코루틴을 사용합니다.
        typingCoroutine = StartCoroutine(TypeSentenceWithAudio(targetText, sentence));
    }

    // TypeSentenceWithAudio 코루틴 (오디오 로직 포함)
    IEnumerator TypeSentenceWithAudio(TextMeshProUGUI targetText, string sentence)
    {
        isTyping = true;
        targetText.text = "";

        // 타자 소리 간격을 초기화
        typingSoundCooldown = 0f; // 오디오 재생 여부를 결정하는 변수 (DialogueManager 로직 유지)
        float typingSpeed = 0.05f; // 기존 코드의 yield 값 사용

        foreach (char letter in sentence.ToCharArray())
        {
            targetText.text += letter;

            // 🔊 타자 소리가 끝나고 난 후 다음 타자 소리를 재생
            // (DialogueManager에서 이식된 로직)
            if (typingAudioSource != null && typingSoundClip != null && !typingAudioSource.isPlaying)
            {
                // 타자 소리 재생
                typingAudioSource.PlayOneShot(typingSoundClip);

                // 타자 소리 다음 재생까지 대기하는 시간 설정 (로직 유지를 위해 남김)
                typingSoundCooldown = typingSoundInterval;
            }

            // 타자 속도만큼 대기
            yield return new WaitForSecondsRealtime(typingSpeed); 
        }

        isTyping = false;
        typingCoroutine = null;
        if (typingAudioSource != null)
            typingAudioSource.Stop();
    }


    // 대화 종료
    public void EndDialogue()
    {
        // 이미 비활성화 상태라면 중복 호출 방지 (안전 장치)
        if (!isDialogueActive) return; 

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isDialogueActive = false;
        isTyping = false;

        dialoguePanel.SetActive(false);
        narrationPanel.SetActive(false);

        currentSentenceIndex = 0;
        onDialogueEndCallback?.Invoke();

        // 🔊 오디오 정지
        if (typingAudioSource != null)
            typingAudioSource.Stop(); 

        Time.timeScale = 1f; 
    }
    
    // PauseMenu 스크립트에서 참조할 수 있도록 IsActive() 함수 추가
    public bool IsActive()
    {
        return isDialogueActive;
    }
}