using System;
using UnityEngine;
using UnityEngine.UI;

public class NodeButton : MonoBehaviour
{
    public Button button;
    public Image icon;
    public int index;

    private Action<int> onClickAction; // Action to execute when the button is clicked

    public void Initialize(int index, Sprite sprite, Action<int> onClickAction)
    {
        button.onClick.AddListener(OnClick);
        //icon.sprite = sprite;
        this.index = index;
        this.onClickAction = onClickAction;
    }

    private void OnClick()
    {
        // Execute the assigned action when the button is clicked
        onClickAction?.Invoke(index);
    }
}
