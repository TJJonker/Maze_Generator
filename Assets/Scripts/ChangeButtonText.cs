using TMPro;
using UnityEngine;

public class ChangeButtonText : MonoBehaviour
{
    [SerializeField] private string initialText;
    [SerializeField] private string changedText;

    private bool toggled = false;

    private TextMeshProUGUI m_TextMeshPro;

    private void Start()
    {
        m_TextMeshPro = transform.Find("text").GetComponent<TextMeshProUGUI>();
        m_TextMeshPro.text = initialText;   
    }

    public void ToggleText()
    {
        var desiredString = !toggled ? changedText : initialText;
        toggled = !toggled;
        m_TextMeshPro.text = desiredString; 
    }
}
