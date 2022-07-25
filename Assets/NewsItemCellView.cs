using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsItemCellView : EnhancedScrollerCellView {
    public TextMeshProUGUI subject, body, date, sender;
    private NewsItem data;
    public void SetData(NewsItem _data) {
        data = _data;
        subject.text = "Subject: "+data.subject;
        body.text = data.body;
        date.text = "Date: "+data.date.GetDateString();
        sender.text = "From: "+data.sender;
    }

    public void OnClick() {
        data.read = true;
        FindObjectOfType<NewsController>().SetNews();
    }
}
