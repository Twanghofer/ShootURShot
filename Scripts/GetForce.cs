using UnityEngine;
using UnityEngine.EventSystems;

public class GetForce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameManager gameManager;

    private void Update()
    {
        if (!gameManager.multiplySpeed) return;
        gameManager.MultiplySpeed();
        gameManager.ScaleLine();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameManager.state != GameManager.PlayState.PLAYER1SHOOTING && gameManager.state != GameManager.PlayState.PLAYER2SHOOTING) return;
        gameManager.multiplySpeed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gameManager.setVariables();
    }
}
