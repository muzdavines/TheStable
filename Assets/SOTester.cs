using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOTester : MonoBehaviour
{
    public string testString;
    public SpecialMove move;
    public void Start() {
        Type myType = Type.GetType(testString);
        SpecialMove myObj = (SpecialMove)Activator.CreateInstance(myType);
        move = myObj;
    }
}
