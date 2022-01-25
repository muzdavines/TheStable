using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public class ActiveMoveListScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    public List<Move> _data;
    public EnhancedScroller myScroller;
    public ActiveMoveCellView ActiveMoveCellViewPrefab;
    public Character activeChar;
    public MoveType moveType;
    public void OnEnable() {
        //Start();
    }

    public virtual void Start() {
        return;
        _data = new List<Move>();
        foreach (Move move in GetComponentInParent<HeroEditController>().activeCharacter.activeMeleeMoves) {

            _data.Add(move);
        }
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
    public virtual void Init(Character _activeChar) {
        activeChar = _activeChar;
        _data = new List<Move>();
        
        foreach (Move move in activeChar.activeMeleeMoves) {
            if (move.moveType != moveType) { continue; }
            _data.Add(move);
           
        }
        foreach (Move move in activeChar.activeRangedMoves) {
            if (move.moveType != moveType) { continue; }
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
        MoveCellView cellView = scroller.GetCellView(ActiveMoveCellViewPrefab) as
        MoveCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Init(activeChar);
    }
}