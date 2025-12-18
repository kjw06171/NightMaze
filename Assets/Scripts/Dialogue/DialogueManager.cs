using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using System.Collections;
using System;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;

    public Action OnDialogueStart;
    public Action OnDialogueEnd;

    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DialogueManager>();
                // ❌ [수정]: 인스턴스를 찾지 못했을 때 Debug.LogError를 출력하지 않습니다.
                // if (_instance == null)
                //     Debug.LogError("DialogueManager instance not found!"); 
            }
            return _instance;
        }
    }

    [Header("UI 요소 연결")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; 	 	 // 기존 캐릭터 대사
    public TextMeshProUGUI dialogueTextNarration; // 🔥 새로 추가 — 내레이션 전용(Text (1))
    public TextMeshProUGUI speakerNameText;
    public Image characterPortrait;
    public GameObject pauseMenuCanvas;

    [Header("오디오 설정")]
    public AudioSource typingAudioSource;
    public AudioClip typingSoundClip;

    [Tooltip("사운드를 얼마나 자주 재생할지 결정하는 간격(초)")]
    public float typingSoundInterval = 0.05f; 
    private float typingSoundCooldown = 0f;

    [Range(0f, 1f)]
    public float defaultTypingVolume = 0.5f;

    [Header("플레이어 제어")]
    public MonoBehaviour playerMovementComponent;

    [Header("설정")]
    public float typingSpeed = 0.05f; 

    private DialogueSO currentDialogueData;
    private int currentSentenceIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Action onDialogueEndCallback;

    private bool wasPauseMenuVisibleBeforeDialogue = false;

    void Awake()
    {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this) {
            Destroy(gameObject);
            return;
        }

        if (dialoguePanel == null || dialogueText == null || speakerNameText == null)
        {
            Debug.LogError("DialogueManager UI 연결 안 됨!");
            gameObject.SetActive(false);
            return;
        }

        if (typingAudioSource != null)
        {
            typingAudioSource.volume = defaultTypingVolume;
            typingAudioSource.loop = false;
            typingAudioSource.playOnAwake = false;
        }

        dialoguePanel.SetActive(false);

        if (characterPortrait != null)
            characterPortrait.gameObject.SetActive(false);

        if (dialogueTextNarration != null)
            dialogueTextNarration.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                HandleAdvanceDialogue();
        }
    }

    // ------------------------------------------------------------
    public void SetTypingVolume(float volume)
    {
        if (typingAudioSource != null)
        {
            typingAudioSource.volume = Mathf.Clamp01(volume);
            defaultTypingVolume = typingAudioSource.volume;
        }
    }

    private void HandleAdvanceDialogue()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            isTyping = false;
            if (typingAudioSource != null)
                typingAudioSource.Stop();

            // 기존 sentences 방식
            if (currentDialogueData.dialogueSentences == null || currentDialogueData.dialogueSentences.Length == 0)
            {
                dialogueText.text = currentDialogueData.sentences[currentSentenceIndex];
            }
            else
            {
                var line = currentDialogueData.dialogueSentences[currentSentenceIndex];
                if (string.IsNullOrEmpty(line.speakerName))
                    dialogueTextNarration.text = line.sentence;
                else
                    dialogueText.text = line.sentence;
            }
            return;
        }

        currentSentenceIndex++;

        if (currentSentenceIndex < currentDialogueData.SentenceCount)
            DisplayCurrentSentence();
        else
            EndDialogue();
    }

    // ------------------------------------------------------------
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

        Time.timeScale = 0f;

        if (playerMovementComponent != null)
            playerMovementComponent.enabled = false;

        dialoguePanel.SetActive(true);

        if (pauseMenuCanvas != null)
        {
            wasPauseMenuVisibleBeforeDialogue = pauseMenuCanvas.activeSelf;
            pauseMenuCanvas.SetActive(false);
        }

        DisplayCurrentSentence();

        OnDialogueStart?.Invoke();
    }

    // ------------------------------------------------------------
    // 🔥 핵심: 캐릭터 대사 vs 내레이션 UI 스위칭
    // ------------------------------------------------------------
    private void DisplayCurrentSentence()
    {
        if (currentSentenceIndex >= currentDialogueData.SentenceCount)
        {
            EndDialogue();
            return;
        }

        bool multi = currentDialogueData.dialogueSentences != null &&
                        currentDialogueData.dialogueSentences.Length > 0;

        if (multi)
        {
            var line = currentDialogueData.dialogueSentences[currentSentenceIndex];
            bool hasSpeaker = !string.IsNullOrEmpty(line.speakerName);

            if (hasSpeaker)
            {
                // 캐릭터 대사 모드
                dialogueText.gameObject.SetActive(true);
                dialogueTextNarration.gameObject.SetActive(false);

                // 화자 이름을 설정 (매 대사마다 업데이트)
                speakerNameText.text = line.speakerName;
                speakerNameText.gameObject.SetActive(true);

                if (line.portrait != null)
                {
                    characterPortrait.sprite = line.portrait;
                    characterPortrait.gameObject.SetActive(true);
                }
                else
                {
                    characterPortrait.gameObject.SetActive(false);
                }

                StartTyping(line.sentence);
            }
            else
            {
                // 내레이션 모드
                dialogueText.gameObject.SetActive(false);
                dialogueTextNarration.gameObject.SetActive(true);

                // 내레이션일 때는 이름을 숨김
                speakerNameText.gameObject.SetActive(false); 
                characterPortrait.gameObject.SetActive(false);

                StartTypingNarration(line.sentence);
            }

            return;
        }

        // ------------------------------------------------------------
        // 🔥 기존 코드(단일 캐릭터) 그대로 유지
        // ------------------------------------------------------------
        dialogueText.gameObject.SetActive(true);
        dialogueTextNarration.gameObject.SetActive(false);

        // 이름을 설정할 때 null 체크 후 업데이트 (빈 값이면 "Unknown" 사용)
        speakerNameText.text = !string.IsNullOrEmpty(currentDialogueData.characterName)
            ? currentDialogueData.characterName
            : "Unknown";

        // 단일 캐릭터 대사일 때는 이름을 다시 표시
        speakerNameText.gameObject.SetActive(true);

        if (currentDialogueData.portrait != null)
        {
            characterPortrait.sprite = currentDialogueData.portrait;
            characterPortrait.gameObject.SetActive(true);
        }
        else
        {
            characterPortrait.gameObject.SetActive(false);
        }

        StartTyping(currentDialogueData.sentences[currentSentenceIndex]);
    }




    // ------------------------------------------------------------
    private void StartTyping(string sentence)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        // 타자 소리 간격을 초기화
        typingSoundCooldown = 0f;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            // 타자 소리가 끝나고 난 후 다음 타자 소리를 재생
            if (typingAudioSource != null && typingSoundClip != null && !typingAudioSource.isPlaying)
            {
                // 타자 소리 재생
                typingAudioSource.PlayOneShot(typingSoundClip);

                // 타자 소리 다음 재생까지 대기하는 시간 설정
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



    // ------------------------------------------------------------
    // 🔥 내레이션 타이핑
    // ------------------------------------------------------------
    private void StartTypingNarration(string sentence)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentenceNarration(sentence));
    }

    IEnumerator TypeSentenceNarration(string sentence)
    {
        isTyping = true;
        dialogueTextNarration.text = "";
        typingSoundCooldown = 0f;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueTextNarration.text += letter;

            // 타자 소리가 끝난 후, 다음 타자 소리 재생
            if (typingAudioSource != null && typingSoundClip != null && !typingAudioSource.isPlaying)
            {
                typingAudioSource.PlayOneShot(typingSoundClip);
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

    // ------------------------------------------------------------
    public void EndDialogue()
    {
        if (!isDialogueActive) return; 

        isDialogueActive = false;

        if (typingAudioSource != null)
            typingAudioSource.Stop();

        dialoguePanel.SetActive(false);

        if (characterPortrait != null)
            characterPortrait.gameObject.SetActive(false);

        dialogueText.text = "";
        dialogueTextNarration.text = "";
        speakerNameText.text = "";

        onDialogueEndCallback?.Invoke();
        onDialogueEndCallback = null;

        OnDialogueEnd?.Invoke();

        if (pauseMenuCanvas != null && wasPauseMenuVisibleBeforeDialogue)
            pauseMenuCanvas.SetActive(true);

        Time.timeScale = 1f;

        if (playerMovementComponent != null)
            playerMovementComponent.enabled = true;
    }

    public bool IsActive()
    {
        return isDialogueActive;
    }
}