using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public class ActiveWeaponController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement {
    public List<Weapon> _data;
    public EnhancedScroller myScroller;
    public ActiveWeaponCellView ActiveWeaponCellViewPrefab;
    public void OnEnable() {
        Start();
    }

    public virtual void Start() {
        _data = new List<Weapon>();
        Character c = GetComponentInParent<HeroEditController>().activeCharacter;
        Weapon thisWeapon = c.weapon;
        if (thisWeapon == null) { c.weapon = thisWeapon = c.GetDefaultWeapon(); }
        
        _data.Add(thisWeapon);
        
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
        ActiveWeaponCellView cellView = scroller.GetCellView(ActiveWeaponCellViewPrefab) as
            ActiveWeaponCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Start();
    }
}