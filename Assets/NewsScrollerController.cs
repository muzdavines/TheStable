using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, UIElement 
{
    public List<NewsItem> _data;
    public NewsItemCellView newsItemCellViewPrefab;
    public EnhancedScroller myScroller;
    
    public void OnEnable() {
        Start();
    }
    public void Start() {
        _data = new List<NewsItem>();
        foreach (NewsItem c in Game.instance.news) {
            if (c.read) {
                continue;
            }
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
        return 200f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int
        dataIndex, int cellIndex) {
        NewsItemCellView cellView = scroller.GetCellView(newsItemCellViewPrefab) as
            NewsItemCellView;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }

    public void UpdateOnAdvance() {
        Start();
    }
}
