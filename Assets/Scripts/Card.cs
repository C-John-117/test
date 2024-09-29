using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardTemplate
{
    /// <summary>
    /// Classe permettant de créer des objets de type carte
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Card/Card")]
    public class Card : ScriptableObject, IComparable<Card>
    {
        public enum Suit
        {
            Heart,
            Diamond,
            Club,
            Spade
        }

        public enum State
        {
            FaceUp,
            FaceDown,
            Empty
        }

        [SerializeField] Sprite faceUpSprite;
        [SerializeField] Suit suit;
        [SerializeField] int minValue = 0;
        [SerializeField] int maxValue;
        [SerializeField] State state = State.Empty;

        public Sprite GetFaceUpSprite()
        {
            return faceUpSprite;
        }

        public int GetMaxValue()
        {
            return maxValue;
        }

        public Suit GetSuit()
        {
            return suit;
        }

        public int CompareTo(Card other)
        {
            if(other.maxValue > maxValue)
            {
                return -1;
            }
            else if (other.maxValue == maxValue)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
