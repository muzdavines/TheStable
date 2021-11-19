using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
public class BusinessScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {

    private List<Finance.Business> _data;
    public EnhancedScroller myScroller;
    public BusinessCellView businessCellViewPrefab;
    public void OnEnable() {
        Start();
    }
    void Start() {
        _data = new List<Finance.Business>();
        foreach (Finance.Business b in Game.instance.playerStable.finance.businesses) {
            _data.Add(b);
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
        BusinessCellView cellView = scroller.GetCellView(businessCellViewPrefab) as
        BusinessCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Start();
    }
}