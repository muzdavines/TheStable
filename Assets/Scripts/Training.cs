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
    [SerializeField]
    public Move moveToTrain;
    //public Character myCharacter;
    

    public void BeginTraining(Character myChar) {
        
        dateToTrain = myChar.returnDate = Helper.Today().Add(duration);
    }

}
[System.Serializable]
public class TrainingSave {
    public Training.Type type = Training.Type.Attribute;
    public string training = "None";
    public int duration = 0;
    public int cost = 0;
    public Game.GameDate dateToTrain = new Game.GameDate() { year = 99999 };
    [SerializeField]
    public Move moveToTrain;

    public TrainingSave CopyValues(Training source) {
        this.type = source.type;
        this.training = source.training;
        this.duration = source.duration;
        this.cost = source.cost;
        this.dateToTrain = source.dateToTrain;
        this.moveToTrain = source.moveToTrain;
        return this;
    }

}
