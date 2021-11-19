using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
public class MissionHeroesScrollerController : MonoBehaviour, IEnhancedScrollerDelegate {
    public List<Character> _data;
    public EnhancedScroller myScroller;
    public MissionHeroCellView HeroCellViewPrefab;
    public virtual void OnEnable() {
        
    }
    public int GetNumberOfCells(EnhancedScroller scroller) {
        return _data.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) {
        return 100f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int
    dataIndex, int cellIndex) {
        MissionHeroCellView cellView = scroller.GetCellView(HeroCellViewPrefab) as
        MissionHeroCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }
}