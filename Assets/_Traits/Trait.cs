using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class Trait : ScriptableObject
{
    public string traitName;
    public string description;
    public int level = 1;
    [SerializeField]
    public List<StepType> usableFor;
    public Texture icon;
    public int baseCost = 1000;
    public ClipTransition attemptAnim;
    public ClipTransition successAnim;
    public ClipTransition failAnim;
}
