using UnityEngine;
using UnityEngine.UI;

public class InvokeButtonEvent : MonoBehaviour
{
    private Button button;
    private void Start()
        => button = GetComponent<Button>(); 

    public void InvokeButton()
        => button.onClick.Invoke();
}
