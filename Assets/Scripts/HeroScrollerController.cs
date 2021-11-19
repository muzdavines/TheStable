using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
public class HeroScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private List<Character> _data;
    public EnhancedScroller myScroller;
    public HeroCellView HeroCellViewPrefab;
    public void OnEnable()
    {
        Start();
    }

    public void Start() {
        _data = new List<Character>();
        foreach (Character c in Game.instance.playerStable.heroes) {
            _data.Add(c);
        }
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _data.Count; 
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 100f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int
    dataIndex, int cellIndex)
    {
        HeroCellView cellView = scroller.GetCellView(HeroCellViewPrefab) as
        HeroCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void ClearActives() {
        foreach (HeroTrainingCellView t in FindObjectsOfType<HeroTrainingCellView>()) {
            t.Deselect();
        }
    }
}