using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace CardTemplate
{
    /// <summary>
    /// Cette classe contient des méthodes statiques afin de faire l'affichage dans Unity
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();

                    if (_instance == null)
                    {
                        _instance = new UIManager();
                    }
                }

                return _instance;
            }
        }

        [SerializeField] TextMeshProUGUI winnerText;
        [SerializeField] GameObject betPanel;
        [SerializeField] TextMeshProUGUI currentPlayerText;
        [SerializeField] TextMeshProUGUI currentPlayerChipCountText;
        [SerializeField] TMP_InputField playerRaiseText;
        [SerializeField] TextMeshProUGUI pot;
        [SerializeField] Button btnCall;
        [SerializeField] Button btnCheck;
        [SerializeField] GameObject raiseSection;
        [SerializeField] GameObject winnerPanel;
        [SerializeField] GameObject nextRoundPanel;

        private Player currentPlayer;

        public void ChangeWinnerText(List<Player> winners, long handValue)
        {
            string winnersValue = "";
            foreach(Player winner in winners)
            {
                winnersValue += winner.playerNumber + " ";
            }
            winnerText.text = "The winner is Player " + winnersValue;
        }

        public void Raise()
        {
            int amount = int.Parse(playerRaiseText.text);
            if (amount > 0)
            {
                if (((PokerPlayer)currentPlayer).Raise(amount))
                {
                    btnCall.interactable = true;
                    btnCheck.interactable = false;
                    playerRaiseText.text = "";
                }
            }
        }

        public void Call()
        {
            ((PokerPlayer)currentPlayer).Call();
        }

        public void Check()
        {
            currentPlayer.Check();
        }

        public void Fold()
        {
            currentPlayer.Fold();
        }

        public void AllIn()
        {
            int amount = currentPlayer.GetChipCount() - Table.Instance.GetCurrentMaxBet();
            if (amount > 0)
            {
                if (((PokerPlayer)currentPlayer).Raise(amount))
                {
                    btnCall.interactable = true;
                    btnCheck.interactable = false;
                    playerRaiseText.text = "";
                }
            }
        }

        public void StartBetPhase(Player player)
        {
            betPanel.SetActive(true);
            btnCheck.interactable = true;
            btnCall.interactable = false;
            btnCall.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Call";
            BetPhase(player);
        }

        public void BetPhase(Player player)
        {
            ShowRaiseSection(true);
            currentPlayer = player;
            currentPlayerText.text = "Player " + player.playerNumber;
            currentPlayerChipCountText.text = "Chip Count: " + player.GetChipCount();
            int currentTableBet = Table.Instance.GetCurrentMaxBet();
            if(currentTableBet > 0)
            {
                string toCallText = "";
                int toCall = currentTableBet - player.GetCurrentBet();
                if(toCall >= player.GetChipCount())
                {
                    toCallText = "All In";
                    ShowRaiseSection(false);
                }
                else
                {
                    toCallText = toCall.ToString();
                }
                btnCall.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Call " + toCallText;
            }
        }

        public void HideBetPhase()
        {
            betPanel.SetActive(false);
        }

        public void EndRound()
        {
            currentPlayer = null;
            betPanel.SetActive(false);
            pot.text = "0";
            nextRoundPanel.SetActive(true);
        }

        public void UpdatePot(int amount)
        {
            pot.text = amount.ToString();
        }

        public bool IsCurrentPlayer(Player player)
        {
            return player == currentPlayer;
        }

        void ShowRaiseSection(bool show)
        {
            foreach (Transform t in raiseSection.transform)
            {
                t.GetComponent<Selectable>().interactable = show;
            }
        }

        public void ShowWinner(Player player)
        {
            winnerPanel.SetActive(true);
            winnerPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "The winner is " + player.playerNumber;
        }

        public void NextRound()
        {
            nextRoundPanel.SetActive(false);
            Table.Instance.StartNewRound();
        }

    }
}
