using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
public class ContractScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    public List<MissionContract> _data;
    public EnhancedScroller myScroller;
    public ContractCellView contractCellViewPrefab;
    public void OnEnable() {
        Start();
    }
    public virtual void Start() {
       
    }
    public int GetNumberOfCells(EnhancedScroller scroller) {
        return _data.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) {
        return 200f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int
    dataIndex, int cellIndex) {
        ContractCellView cellView = scroller.GetCellView(contractCellViewPrefab) as
        ContractCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Start();
    }
}