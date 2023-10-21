using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Button button;
    public int index;
    public bool starter;

    private System.Action<bool, int> onClickAction; // Action to execute when the button is clicked

    public void Initialize(bool starter, int index, System.Action<bool, int> onClickAction)
    {
        this.starter = starter;
        this.index = index;
        button.onClick.AddListener(OnClick);
        this.onClickAction = onClickAction;
    }

    private void OnClick()
    {
        // Execute the assigned action when the button is clicked
        onClickAction?.Invoke(starter, index);
    }
}
