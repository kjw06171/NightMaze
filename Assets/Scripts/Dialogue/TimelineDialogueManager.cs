using UnityEngine;
using TMPro; // TextMeshProUGUI를 사용하기 위해 필요
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement; 

public class TimelineDialogueManager : MonoBehaviour
{
    public static TimelineDialogueManager Instance { get; private set; }

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Image speakerPortrait; // 초상화 이미지
    public GameObject narrationPanel; // 내레이션 UI 패널 (내레이션 전용 UI)
    
    public TextMeshProUGUI narrationText; // 내레이션 텍스트 (대사) 

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

            var currentSentence = currentDialogueData.dialogueSentences[currentSentenceIndex];
            
            // 텍스트를 즉시 전체 표시
            if (string.IsNullOrEmpty(currentSentence.speakerName))
                narrationText.text = currentSentence.sentence;
            else
                dialogueText.text = currentSentence.sentence;
            
            // 🟢 [수정]: 마지막 문장을 스킵한 경우, 즉시 EndDialogue 호출 (타임스케일 복구 포함)
            if (currentSentenceIndex == currentDialogueData.SentenceCount - 1)
            {
                EndDialogue();
                return; 
            }
            
            // 중간 문장의 스킵은 타이핑만 완료하고 다음 E 입력을 기다림 (아래의 else 경로로 진입하기 위해 return 제거)
            // 즉, 스킵 후 한 번 더 E를 눌러야 다음 문장으로 넘어갑니다.
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
            // 모든 문장이 끝났을 때 EndDialogue 호출 (타이핑 코루틴에서 처리되지만, 혹시 모를 경우를 대비)
            EndDialogue();
        }
    }

    // 대화 시작
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
        
        // 🟢 대화가 시작되는 순간 Time.timeScale = 0f로 정지
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
        
        // 내레이션일 때는 dialoguePanel을 비활성화하고, narrationPanel을 활성화
        narrationPanel.SetActive(isNarration);
        dialoguePanel.SetActive(!isNarration); // 대화일 때는 dialoguePanel을 활성화하고 narrationPanel을 비활성화

        TextMeshProUGUI targetText;

        if (isNarration)
        {
            targetText = narrationText;
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
        }
        
        // 타이핑 시작
        StartTyping(targetText, line.sentence);
    }



    // StartTyping 함수
    private void StartTyping(TextMeshProUGUI targetText, string sentence)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(targetText, sentence));
    }

    // TypeSentence 코루틴 
    IEnumerator TypeSentence(TextMeshProUGUI targetText, string sentence)
    {
        isTyping = true;
        targetText.text = ""; 

        foreach (char letter in sentence.ToCharArray())
        {
            targetText.text += letter;
            yield return new WaitForSecondsRealtime(0.05f); 
        }

        isTyping = false;
        typingCoroutine = null;

        // 🟢 [수정]: 타이핑이 끝났을 때, 마지막 문장이라면 즉시 EndDialogue 호출 (E 입력 불필요)
        // 특정 씬에서는 타이핑이 끝나면 자동으로 대화창을 종료
        if (currentSentenceIndex == currentDialogueData.SentenceCount - 1)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "SpecificScene") // 특정 씬에서는 자동 종료
            {
                EndDialogue();
            }
        }
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

        // 🟢 [최종]: 대화가 완전히 끝났을 때만 Time.timeScale 재개 (요청하신 기능 구현)
        Time.timeScale = 1f; 
    }
}
