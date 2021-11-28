using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public class MoveListScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    public List<Move> _data;
    public EnhancedScroller myScroller;
    public MoveCellView MoveCellViewPrefab;
    public void OnEnable() {
        Start();
    }

    public virtual void Start() {
        _data = new List<Move>();
        foreach (Move move in GetComponentInParent<HeroEditController>().activeCharacter.knownMoves) {

            _data.Add(move);
        }
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
    public int GetNumberOfCells(EnhancedScroller scroller) {
        return _data.Count;
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) {
        return 150f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int
    dataIndex, int cellIndex) {
        MoveCellView cellView = scroller.GetCellView(MoveCellViewPrefab) as
        MoveCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Start();
    }
}