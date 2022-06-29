using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationCollector : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputFieldWidth;
    [SerializeField] private TMP_InputField inputFieldHeight;
    [SerializeField] private TMP_InputField inputFieldSeed;
    [SerializeField] private Toggle toggleRandomSeed;

    private const int MAX_RANDOM_SEED = 20000;

    public void SetDisableRandomisationSeed()
        => inputFieldSeed.interactable = !toggleRandomSeed.isOn;

    public void GetInformation(out int width, out int height, out int randomSeed)
    {
        width = GetWidthInput();
        height = GetHeightInput();
        randomSeed = toggleRandomSeed.isOn ? Mathf.FloorToInt(Random.Range(0, MAX_RANDOM_SEED)) : int.Parse(inputFieldSeed.text);
        inputFieldSeed.text = randomSeed.ToString();
    }

    public int GetWidthInput() => int.Parse(inputFieldWidth.text);
    public int GetHeightInput() => int.Parse(inputFieldHeight.text);
}
