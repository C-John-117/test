using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.WSA;

namespace CardTemplate
{
    /// <summary>
    /// Classe qui gère les actions que le joueur peut faire
    /// </summary>
    public abstract class Player : MonoBehaviour
    {
        [SerializeField]protected int chipCount = 600;
        protected Hand hand;
        [SerializeField] GameObject handObject;
        protected SpriteRenderer[] cardsRenderer;
        protected int currentBet;
        string playerName;
        public int playerNumber; //Used to full Table Dictionnary only
        public bool hasPlayed = false;
        public bool isAllIn = false;
        public bool hasLost = false;


        public virtual void Start()
        {
            cardsRenderer = new SpriteRenderer[handObject.transform.childCount];
            for (int i = 0; i < handObject.transform.childCount; i++)
            {
                cardsRenderer[i] = handObject.transform.GetChild(i).GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Méthode utilisée pour ajouter une carte à la main du joueur.
        /// </summary>
        /// <param name="card">La carte à ajouter à la main.</param>
        /// <param name="facedDown">Indique si la carte doit être distribuée face cachée. Par défaut, elle est face cachée.</param>
        public void AddCardToHand(Card card, bool facedDown = true)
        {
            int index = hand.AddCard(card,facedDown);

            Sprite cardSprite;

            // Détermine la sprite à utiliser en fonction de la position de la carte (face cachée ou visible)
            if (facedDown)
            {
                cardSprite = Table.Instance.GetFaceDownCardSprite();
            }
            else
            {
                cardSprite = card.GetFaceUpSprite();
            }

            cardsRenderer[index].sprite = cardSprite;

            //Animation de distribution de cartes
            Vector2 initialPosition = cardsRenderer[index].transform.position;
            Quaternion initialRotation = cardsRenderer[index].transform.rotation;
            cardsRenderer[index].transform.position = Table.Instance.GetDealerTransform().position;
            cardsRenderer[index].transform.rotation = Table.Instance.GetDealerTransform().rotation;

            // Lance une animation pour placer la carte à sa position finale
            StartCoroutine(Table.Instance.PlaceCardInPositionAnimation(cardsRenderer[index].transform, initialPosition, initialRotation));
        }

        

        public void RevealCards()
        {
            for (int i = 0; i < cardsRenderer.Length; i++)
            {
                cardsRenderer[i].sprite = hand.GetCardX(i).GetFaceUpSprite();
            }
        }

        public void HideCards()
        {
            for (int i = 0; i < cardsRenderer.Length; i++)
            {
                cardsRenderer[i].sprite = Table.Instance.GetFaceDownCardSprite();
            }
        }

        public abstract void Bet(int amount);

        public abstract void Fold();

        public abstract void Check();

        public Hand GetHand()
        {
            return hand;
        }

        public int GetCurrentBet()
        {
            return currentBet;
        }

        public void SetCurrentBet(int bet)
        {
            currentBet = bet;
        }

        public int GetChipCount()
        {
            return chipCount;
        }

        public void AddToChipCount(int amount)
        {
            chipCount += amount;
        }

        public void ClearHand()
        {
            foreach (SpriteRenderer cardRenderer in cardsRenderer)
            {
                cardRenderer.sprite = null;
            }
            hand.Clear();
        }

        public void VerifyLoseCondition()
        {
            if (chipCount == 0)
            {
                hasLost = true;
                gameObject.SetActive(false);
                Table.Instance.RemoveFromPlayerOrder(this);
            }
        }
    }
}
