using CardTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRevealer : MonoBehaviour
{
    Player player;

    private void Start()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnMouseEnter()
    {
        if(UIManager.Instance.IsCurrentPlayer(player))
            player.RevealCards();
    }

    private void OnMouseExit()
    {
        if (UIManager.Instance.IsCurrentPlayer(player))
            player.HideCards();
    }
}
