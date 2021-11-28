using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public class StableTrainingScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    public List<Training> _data;
    public EnhancedScroller myScroller;
    public TrainingCellView trainingCellView;
    public void OnEnable() {
        Start();
    }
    public void Start() {
        _data = new List<Training>();
        foreach (Training t in Game.instance.playerStable.availableTrainings) {
            _data.Add(t);
        }
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
    public int GetNumberOfCells(EnhancedScroller scroller) {
        return _data.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) {
        return 100f;
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