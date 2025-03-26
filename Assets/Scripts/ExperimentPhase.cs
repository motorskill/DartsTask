using System;

[Serializable]
public struct ExperimentPhase
{
    public string name;
    public float duration;
    public Action onEnter;
    public Action onUpdate;
    public Action onExit;

    public ExperimentPhase(string name, float duration, Action onEnter = null, Action onUpdate = null, Action onExit = null)
    {
        this.name = name;
        this.duration = duration;
        this.onEnter = onEnter;
        this.onUpdate = onUpdate;
        this.onExit = onExit;
    }
}
