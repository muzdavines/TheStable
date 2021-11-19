using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu]
public class Training : ScriptableObject { 
    public enum Type { Attribute, Skill}
    public Type type = Type.Attribute;
    [Tooltip("The name of the attribute or skill to be trained")]
    public string training = "None";
    [Tooltip("Length of Training in Days")]
    public int duration = 0;
    [Tooltip("One time cost per point")]
    public int cost = 0;
    public Game.GameDate dateToTrain = new Game.GameDate() {year = 99999 };
    public Move moveToTrain;
    //public Character myCharacter;
    

    public void BeginTraining(Character myChar) {
        
        dateToTrain = myChar.returnDate = Helper.Today().Add(duration);
    }

}
