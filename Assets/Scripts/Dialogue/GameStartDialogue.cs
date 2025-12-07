using UnityEngine;
using System.Collections;
using System;

public class GameStartDialogue : MonoBehaviour
{
    [Header("시작 대화 데이터")]
    public DialogueSO initialDialogue;

    [Header("UI 관리 설정")]
    public GameObject gameUICanvas;

    void Awake()
    {
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        // 1) FadeManager 대기
        while (FadeManager.Instance == null || FadeManager.Instance.fadeImage == null)
            yield return null;

        // 2) Fade-In이 끝날 때까지 기다림
        while (FadeManager.Instance.fadeImage.gameObject.activeSelf)
            yield return null;

        // 3) DialogueManager 준비될 때까지 대기
        int waitFrames = 0;
        while (DialogueManager.Instance == null && waitFrames < 10)
        {
            waitFrames++;
            yield return null;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager 준비 안됨");
            yield break;
        }

        if (initialDialogue == null)
        {
            Debug.LogWarning("시작 대화가 연결되지 않음");
            yield break;
        }

        // 4) 게임 UI 숨김 + 대화 시작
        if (gameUICanvas != null)
            gameUICanvas.SetActive(false);

        Time.timeScale = 0f;

        DialogueManager.Instance.StartDialogue(initialDialogue, OnDialogueEnd);
    }

    private void OnDialogueEnd()
    {
        // UI 다시 활성화
        if (gameUICanvas != null)
            gameUICanvas.SetActive(true);

        Time.timeScale = 1f;

        // 역할 끝
        Destroy(gameObject);
    }
}
