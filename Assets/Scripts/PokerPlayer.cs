using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardTemplate
{
    /// <summary>
    /// Implémentation de la classe Player version Poker
    /// </summary>
    public class PokerPlayer : Player
    {
        [SerializeField] PokerHand pokerHand;

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            hand = pokerHand;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void Bet(int amount)
        {
            int chipsToPay = amount;
            if (amount != 0)
            {
                chipsToPay = amount - currentBet;
            }
            currentBet = amount;
            Table.Instance.UpdatePot(chipsToPay);
            hasPlayed = true;
            Table.Instance.GoToNextPlayerBet(this, true);
            chipCount -= chipsToPay;
            if(chipCount == 0)
            {
                isAllIn = true;
            }
        }

        public override void Fold()
        {
            hasPlayed = true;
            ClearHand();
            Table.Instance.FoldPlayer(this);
        }

        public override void Check()
        {
            Bet(0);
        }

        public bool Raise(int amount)
        {
            int newMaxBet = Table.Instance.GetCurrentMaxBet() + amount;
            if (newMaxBet - currentBet > chipCount)
            {
                return false;
            }
            else
            {
                Table.Instance.SetCurrentMaxBet(newMaxBet);
                Bet(newMaxBet);
                return true;
            }
        }

        public void Call()
        {
            int currentTableMaxBet = Table.Instance.GetCurrentMaxBet();
            if (currentTableMaxBet > chipCount + currentBet)
            {
                Bet(chipCount+currentBet);
            }
            else
            {
                Bet(currentTableMaxBet);
            }
        }
    }
}
