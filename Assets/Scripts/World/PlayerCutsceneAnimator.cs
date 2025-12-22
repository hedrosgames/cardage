using System;
using System.Collections;
using UnityEngine;
public class PlayerCutsceneAnimator : MonoBehaviour
{
    public Animator animator;
    public PlayerControl playerControl;
    public PlayerMove playerMove;
    public float defaultMinDelay = 0.3f;
    public string defaultStateTag = "Interact";
    public bool waitAnimationEnd = true;
    public float maxExtraTime = 1f;
    bool isPlaying;
    public bool IsPlaying
    {
        get { return isPlaying; }
    }
    public void PlayCutscene(string triggerName, Action onComplete)
    {
        PlayCutscene(triggerName, defaultMinDelay, defaultStateTag, true, onComplete);
    }
    public void PlayCutscene(string triggerName, float minDelay, string stateTag, Action onComplete)
    {
        PlayCutscene(triggerName, minDelay, stateTag, true, onComplete);
    }
    public void PlayCutsceneNoControl(string triggerName, float minDelay, string stateTag, Action onComplete)
    {
        PlayCutscene(triggerName, minDelay, stateTag, false, onComplete);
    }
    public void PlayCutscene(string triggerName, float minDelay, string stateTag, bool manageControl, Action onComplete)
    {
        if (isPlaying) return;
        if (!gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(PlayRoutine(triggerName, minDelay, stateTag, manageControl, onComplete));
    }
    IEnumerator PlayRoutine(string triggerName, float minDelay, string stateTag, bool manageControl, Action onComplete)
    {
        isPlaying = true;
        if (manageControl && playerControl != null)
        {
            playerControl.SetControl(false);
        }
        if (animator != null)
        {
            if (playerMove != null)
            {
                Vector2 dir = playerMove.LastDirection;
                animator.SetFloat("directionX", dir.x);
                animator.SetFloat("directionY", dir.y);
            }
            if (!string.IsNullOrEmpty(triggerName))
            {
                animator.ResetTrigger(triggerName);
                animator.SetTrigger(triggerName);
            }
        }
        float elapsed = 0f;
        bool animFinished = false;
        while (true)
        {
            elapsed += Time.deltaTime;
            if (!waitAnimationEnd)
            {
                if (elapsed >= minDelay) break;
            }
            else
            {
                if (animator == null)
                {
                    animFinished = true;
                }
                else
                {
                    AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
                    if (!string.IsNullOrEmpty(stateTag))
                    {
                        if (info.IsTag(stateTag) && info.normalizedTime >= 1f)
                        {
                            animFinished = true;
                        }
                    }
                    else
                    {
                        if (info.normalizedTime >= 1f)
                        {
                            animFinished = true;
                        }
                    }
                }
                if (elapsed >= minDelay && animFinished) break;
                if (elapsed > minDelay + maxExtraTime) break;
            }
            yield return null;
        }
        if (manageControl && playerControl != null)
        {
            playerControl.SetControl(true);
        }
        isPlaying = false;
        onComplete?.Invoke();
    }
}

