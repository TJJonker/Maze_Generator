using UnityEngine;
using UnityEngine.UI;

public class InvokeButtonEvent : MonoBehaviour
{
    [SerializeField] private SideMenuBehaviour menu;
    private Button button;

    private void Start()
        => button = GetComponent<Button>(); 

    public void InvokeButton()
        => button.onClick.Invoke();

    public void Close()
    {
        if (!menu.IsIn()) InvokeButton();
    }

    public void Open()
    {
        if(menu.IsIn()) InvokeButton();
    }
}
