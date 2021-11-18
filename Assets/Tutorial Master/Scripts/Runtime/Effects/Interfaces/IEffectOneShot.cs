namespace HardCodeLab.TutorialMaster
{
    public delegate void EffectEvent();

    /// <summary>
    /// Indicates if a given effect is done only once.
    /// </summary>
    public interface IEffectOneShot
    {
        event EffectEvent EffectEnd;

        event EffectEvent EffectStart;

        bool HasFinished { get; set; }
    }
}