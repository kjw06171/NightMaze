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
                if (_instance == null)
                    Debug.LogError("DialogueManager instance not found!");
            }
            return _instance;
        }
    }

    [Header("UI 요소 연결")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Image characterPortrait;
    public GameObject pauseMenuCanvas;

    [Header("오디오 설정")]
    public AudioSource typingAudioSource;
    public AudioClip typingSoundClip;

    [Tooltip("사운드를 얼마나 자주 재생할지 결정하는 간격(초)")]
    public float typingSoundInterval = 0.05f;  // ★ 핵심 옵션

    private float typingSoundCooldown = 0f;     // ★ 쿨다운 타이머

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
    // 타이핑 볼륨 조절
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

            // 🔊 타이핑 즉시 종료 → 오디오도 즉시 꺼짐
            if (typingAudioSource != null)
                typingAudioSource.Stop();

            dialogueText.text = currentDialogueData.sentences[currentSentenceIndex];
            return;
        }


        currentSentenceIndex++;

        if (currentDialogueData != null && currentSentenceIndex < currentDialogueData.SentenceCount)
            DisplayCurrentSentence();
        else
            EndDialogue();
    }

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

        speakerNameText.text = dialogueData.characterName;

        if (characterPortrait != null)
        {
            characterPortrait.sprite = dialogueData.portrait;
            characterPortrait.gameObject.SetActive(dialogueData.portrait != null);
        }

        DisplayCurrentSentence();
        OnDialogueStart?.Invoke();
    }

    private void DisplayCurrentSentence()
    {
        if (currentSentenceIndex >= currentDialogueData.SentenceCount)
        {
            EndDialogue();
            return;
        }

        string sentence = currentDialogueData.sentences[currentSentenceIndex];

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    // ------------------------------------------------------------
    // ★ 타이핑 효과: 쿨다운 방식 적용 (겹침 방지)
    // ------------------------------------------------------------
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        typingSoundCooldown = 0f;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            // 🔊 사운드 재생 (쿨다운 방식)
            if (typingAudioSource != null && typingSoundClip != null)
            {
                typingSoundCooldown -= Time.unscaledDeltaTime;

                if (typingSoundCooldown <= 0f)
                {
                    typingAudioSource.PlayOneShot(typingSoundClip);
                    typingSoundCooldown = typingSoundInterval;
                }
            }

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
        typingAudioSource.Stop();   // ★ 타이핑 종료 시 즉시 사운드 정지

    }

    // ------------------------------------------------------------
    public void EndDialogue()
    {
        isDialogueActive = false;

        if (typingAudioSource != null)
        typingAudioSource.Stop();

        dialoguePanel.SetActive(false);

        if (characterPortrait != null)
            characterPortrait.gameObject.SetActive(false);

        dialogueText.text = "";
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
