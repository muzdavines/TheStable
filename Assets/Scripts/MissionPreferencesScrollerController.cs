using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
public class MissionPreferencesScrollerController : MonoBehaviour, IEnhancedScrollerDelegate {
    public List<Stage> _data;
    public EnhancedScroller myScroller;
    public MissionPreferencesCellView missionPrefCellView;
    public void Init(MissionContract contract) {
        
        _data = contract.stages;
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
        MissionPreferencesCellView cellView = scroller.GetCellView(missionPrefCellView) as
        MissionPreferencesCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }
}