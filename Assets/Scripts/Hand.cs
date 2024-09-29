using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardTemplate
{
    /// <summary>
    /// Classe Permettant de gérer une main de carte
    /// </summary>
    public abstract class Hand
    {
        [SerializeField] protected List<Card>  cards;

        public Hand()
        {
            cards = new List<Card>();
        }

        public long HandValue
        {
            get {
                return CalculateHandValue(); 
            }
        }

        public int AddCard(Card card, bool facedDown = true)
        {
            cards.Add(card);
            return cards.Count-1;
        }
        public Card GetCardX(int x)
        {
            return cards[x];
        }

        public virtual void SortHand()
        {
            cards.Sort();
        }

        public List<Card> GetAllCards()
        {
            return cards;
        }

        public void Clear()
        {
            cards.Clear();
        }

        public int GetCardCount()
        {
            return cards.Count;
        }

        /// <summary>
        /// Méthode qui calcule la valeur d'une main donnée.
        /// </summary>
        /// <returns>La valeur numérique de cette main.</returns>
        public abstract long CalculateHandValue();
 
    }
}
