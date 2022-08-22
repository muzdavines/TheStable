using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public class StableTrainingScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    public List<Trait> _data;
    public EnhancedScroller myScroller;
    public TrainingCellView trainingCellView;
    public Character activeChar;
    public TrainingController controller;
    public void OnEnable() {
        Start();
    }
    public void Start() {
        Debug.Log("#Training#ScrollerStart");
        activeChar = controller.activeChar;
        _data = new List<Trait>();
        if (activeChar?.activeTraits != null) {
            foreach (Trait t in activeChar.activeTraits) {
                Debug.Log("#Training#" + t.traitName + "  " + t.level);
                _data.Add(t);
            }
        }

        foreach (Trait t in Game.instance.playerStable.availableTrainings) {
            if (!_data.Any(n => n.traitName == t.traitName)) {
                _data.Add(t);
            }
        }
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
    
    public int GetNumberOfCells(EnhancedScroller scroller) {
        return _data.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) {
        return 180f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int
    dataIndex, int cellIndex) {
        TrainingCellView cellView = scroller.GetCellView(trainingCellView) as
        TrainingCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void ClearActives() {
        foreach (TrainingCellView t in FindObjectsOfType<TrainingCellView>()) {
            t.Deselect();
        }
    }

    public void UpdateOnAdvance() {
        Start();
    }
}