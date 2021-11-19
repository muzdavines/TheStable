using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveMoveCellView : MoveCellView
{
    public override void OnClick() {
        control.ActiveMoveClicked(thisMove);
    }
}
