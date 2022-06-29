using UnityEngine;
using TMPro;

public class InputFieldManager : MonoBehaviour
{
    [SerializeField] private InformationCollector informationCollector;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private int warningLimit;

    [SerializeField] private int minimalInput;
    [SerializeField] private bool isWidth;

    private TMP_InputField inputField;

    private void Awake() => inputField = GetComponent<TMP_InputField>();

    public void CheckInput()
    {
        if (int.Parse(inputField.text) < minimalInput) inputField.text = minimalInput.ToString();

        var otherInputFieldValue = isWidth ? informationCollector.GetHeightInput() : informationCollector.GetWidthInput();
        if (otherInputFieldValue * int.Parse(inputField.text) > warningLimit) warningText.enabled = true;
        else warningText.enabled = false;
    }
}
