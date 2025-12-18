using UnityEngine;
using UnityEngine.Playables;

public class ChoiceUIController : MonoBehaviour
{
    public PlayableDirector mainDirector;
    public PlayableDirector endingATimeline;
    public PlayableDirector endingBTimeline;
    public CutsceneBinder cutsceneBinder;

    public KeyCode yesKey = KeyCode.Y;
    public KeyCode noKey = KeyCode.N;

    bool active;
    Playable rootPlayable;

    private void OnEnable()
    {
        active = true;

        if (mainDirector != null)
        {
            rootPlayable = mainDirector.playableGraph.GetRootPlayable(0);
            rootPlayable.SetSpeed(0);   // ⭐️ 핵심
        }
    }

    private void OnDisable()
    {
        active = false;

        if (rootPlayable.IsValid())
            rootPlayable.SetSpeed(1);
    }

    void Update()
    {
        if (!active) return;

        if (Input.GetKeyDown(yesKey))
            PlayEndingA();
        else if (Input.GetKeyDown(noKey))
            PlayEndingB();
    }

    void PlayEndingA()
    {
        gameObject.SetActive(false);
        endingATimeline.Play();
    }

    void PlayEndingB()
    {
        gameObject.SetActive(false);
        cutsceneBinder.EndCutscene();
        endingBTimeline.Play();
    }
}
