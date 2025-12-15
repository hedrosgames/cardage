using UnityEngine;
public abstract class SOTutorialCondition : ScriptableObject
{
    public virtual void OnStart(ManagerTutorial manager)
    {
    }
    public abstract bool CheckCompleted(ManagerTutorial manager);
}

