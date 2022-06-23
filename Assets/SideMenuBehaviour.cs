using UnityEngine;
using Jonko.Timers;

public class SideMenuBehaviour : MonoBehaviour
{
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float movementTime;

    private Vector2 startPosition;
    [SerializeField] Vector2 desiredPosition;

    private float position = 1;


    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    public void MoveTo()
    {
        var direction = position == 1 ? -1 : 1;
        var progressionSteps = 1 / movementTime;
        var startTime = Time.realtimeSinceStartup;

        FunctionUpdater.Create(() =>
        {
            position += Time.deltaTime * progressionSteps * direction;
            rectTransform.anchoredPosition = Vector2.Lerp(desiredPosition, startPosition, movementCurve.Evaluate(position));

            if (Time.realtimeSinceStartup - startTime < movementTime) return false;
            
            position = Mathf.RoundToInt(position);
            return true;
        });
    }
}
