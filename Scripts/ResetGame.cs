using UnityEngine;
using UnityEngine.EventSystems;

public class ResetGame : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isPressed = false;
    public GameManager gameManager;
    private float countDown = 3f;

    private void Update()
    {
        if (!isPressed) return;
        if(countDown > 0)
        {
            countDown -= Time.deltaTime;
        }
        else
        {
            countDown = 0;
            gameManager.HardReset();
            Debug.Log("resetting");
            countDown = 3f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        countDown = 3f;
    }
}
