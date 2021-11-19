using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
public class FreeAgentScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    private List<Character> _data;
    public EnhancedScroller myScroller;
    public FreeAgentCellView freeAgentCellViewPrefab;
    public void OnEnable() {
        Start();
    }
    void Start() {
        _data = new List<Character>();
        foreach (Character c in Game.instance.freeAgentMarket.market) {
            _data.Add(c);
        }
        print(_data.Count + " Scroller");
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
        FreeAgentCellView cellView = scroller.GetCellView(freeAgentCellViewPrefab) as
        FreeAgentCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Start();
    }
}