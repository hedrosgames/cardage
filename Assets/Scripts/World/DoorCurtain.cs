using System.Collections;
using UnityEngine;
public class DoorCurtain : MonoBehaviour
{
    public Animator animator;
    [Header("Sincronia")]
    [Tooltip("TEM QUE SER IGUAL OU MAIOR que o tempo da animação de fechar.")]
    public float closeDuration = 0.5f;
    [Tooltip("Tempo da animação de abrir.")]
    public float openDuration = 0.5f;
    [Header("Animator")]
    public string closeTrigger = "Close";
    public string openTrigger = "Open";
    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }
    void Start()
    {
        if (animator != null)
        {
            animator.ResetTrigger(closeTrigger);
            animator.SetTrigger(openTrigger);
        }
    }
    public IEnumerator PlayCloseRoutine()
    {
        if (animator != null)
        {
            animator.ResetTrigger(openTrigger);
            animator.SetTrigger(closeTrigger);
        }
        yield return new WaitForSeconds(closeDuration);
    }
    public IEnumerator PlayOpenRoutine()
    {
        if (animator != null)
        {
            animator.ResetTrigger(closeTrigger);
            animator.SetTrigger(openTrigger);
        }
        yield return new WaitForSeconds(openDuration);
    }
}

