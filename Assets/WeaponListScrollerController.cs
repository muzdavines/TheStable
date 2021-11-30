using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public class WeaponListScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    public List<Weapon> _data;
    public EnhancedScroller myScroller;
    public WeaponCellView WeaponCellViewPrefab;
    public void OnEnable() {
        Start();
    }

    public virtual void Start() {
        _data = new List<Weapon>();
        foreach (Item w in Game.instance.playerStable.inventory) {
            if (w.GetType() == typeof(Weapon) && !w.isOwned) {
                _data.Add((Weapon)w);
            }
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
        WeaponCellView cellView = scroller.GetCellView(WeaponCellViewPrefab) as
        WeaponCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Start();
    }
}