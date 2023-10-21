using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionLine : MonoBehaviour
{
    public Button button;
    public int parentNodeIndex;
    public int childNodeIndex;

    public System.Action<int, int> onClickAction;

    public void Initialize(int parentNodeIndex, int childNodeIndex, System.Action<int, int> onClickAction)
    {
        button.onClick.AddListener(OnClick);
        this.parentNodeIndex = parentNodeIndex;
        this.childNodeIndex = childNodeIndex;
        this.onClickAction += onClickAction;
    }

    public void OnClick()
    {
        // Handle the click event here by invoking the onLineClick action.
        onClickAction?.Invoke(parentNodeIndex, childNodeIndex);
        Destroy(gameObject);
    }
}
