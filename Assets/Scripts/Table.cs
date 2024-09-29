using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardTemplate
{
    /// <summary>
    /// Classe qui gère la boucle de jeu ainsi que ses interactions
    /// </summary>
    public abstract class Table : MonoBehaviour
    {
        /// <summary>
        /// Liste des cartes présentes dans le jeu (utile pour gérer des configurations de cartes particulières, comme retirer les 2, les jokers, etc.).
        /// </summary>
        [SerializeField] protected List<Card> initialCards;

        /// <summary>
        /// Le paquet de cartes utilisé pour la partie.
        /// </summary>
        protected Stack<Card> deck = new Stack<Card>();

        protected IEnumerable<Card> commonPool;
        protected SpriteRenderer[] commonPoolRenderers;
        [SerializeField] protected GameObject commonPoolObject;

        /// <summary>
        /// Pot contenant le nombre de jetons misés actuellement.
        /// </summary>
        protected int pot;

        [SerializeField] protected List<Player> playerOrder;

        /// <summary>
        /// Dictionnaire permettant de trouver les joueurs selon leur numéro.
        /// </summary>
        protected Dictionary<int, Player> players = new Dictionary<int, Player>();

        [SerializeField] Chip[] allChipsArray;
        protected Dictionary<Chip.Color, Chip> allChips;

        /// <summary>
        /// Liste des joueurs encore actifs pendant le tour en cours.
        /// </summary>
        protected List<Player> activePlayers = new List<Player>();

        /// <summary>
        /// Numéro du round actuel
        /// </summary>
        protected int roundNumber;

        /// <summary>
        /// Mise la plus élevée actuellement.
        /// </summary>
        protected int currentMaxBet;

        /// <summary>
        /// Nombre maximum de joueurs
        /// </summary>
        protected int maxPlayer = 7;
        protected int phase = 0;
        [SerializeField] protected Sprite faceDownCardSprite;
        [SerializeField] protected Transform dealerTransform;

        //Poker related stuff
        int cptDealInitial = 0;
        int cptCommon = 0;
        int cptPlayer = 1;
        const int PLAYER_CARD_NUMBER = 2;
        const int COMMON_POOL_CARD_NUMBER = 5;
        Dictionary<int, int> phaseCardNumber = new Dictionary<int, int>();


        protected static Table _instance;
        public static Table Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Table>();

                    if (_instance == null)
                    {
                        return null;
                    }
                }

                return _instance;
            }
        }

        public void Start()
        {
            foreach (Player player in playerOrder)
            {
                players.Add(player.playerNumber, player);
            }

            foreach (Chip chip in allChipsArray)
            {
                allChips.Add(chip.GetColor(), chip);
            }

            phaseCardNumber.Add(1, 3);
            phaseCardNumber.Add(2, 1);
            phaseCardNumber.Add(3, 1);
            commonPool = new List<Card>();
            commonPoolRenderers = new SpriteRenderer[5];
            for (int i = 0; i < commonPoolObject.transform.childCount; i++)
            {
                commonPoolRenderers[i] = commonPoolObject.transform.GetChild(i).GetComponent<SpriteRenderer>();
            }
            StartRound();
        }

        /// <summary>
        /// Méthode pour mélanger les cartes du paquet. <br/>
        /// <i>Les cartes de <b>initialCards</b> sont copiées pour remplir <b>deck</b>.</i>
        /// </summary>
        public abstract void Shuffle();

        /// <summary>
        /// Méthode pour mettre à jour le pot commun, <br/>
        /// <i>qui sera attribué au gagnant à la fin d'un round.</i>
        /// </summary>
        /// <param name="amount">Quantité ajoutée au pot.</param>
        public abstract void UpdatePot(int amount);

        /// <summary>
        /// Méthode pour calculer le ou les gagnants d'une main.<br/>
        /// <i>Elle détermine la meilleure main parmi toutes les combinaisons possibles.</i>
        /// </summary>
        /// <returns>Liste des gagnants.</returns>
        public abstract List<Player> CalculateWinner();

        /// <summary>
        /// Méthode qui donne une carte à un joueur
        /// </summary>
        /// <param name="card">la carte à distribuer</param>
        /// <param name="player">le joueur qui reçoit la carte</param>
        /// <param name="facedDown">Indique si la carte est distribuée face cachée.</param>
        public abstract void Deal(Card card, Player player, bool facedDown);

        /// <summary>
        /// Méthode qui donne une carte au pool commun
        /// </summary>
        /// <param name="card">La carte à ajouter</param>
        /// <param name="facedDown">Indique si la carte est distribuée face cachée.</param>
        public abstract void Deal(Card card, bool facedDown);

        /// <summary>
        /// Méthode pour répartir le pot final entre les gagnants.
        /// </summary>
        /// <param name="winners">liste des gagnants</param>
        public abstract void GiveChipToWinners(List<Player> winners);

        /// <summary>
        /// Méthode pour définir s'il y a un gagnant pour la partie en cours. <br/>
        /// Appelle la fin de la partie si nécessaire.
        /// </summary>
        public abstract void DefineWinner();

        public virtual void StartRound()
        {
            Shuffle();
            roundNumber++;
            for (int i = 1; i < players.Count + 1; i++)
            {
                if (!players[i].hasLost)
                {
                    activePlayers.Add(players[i]);
                    players[i].hasPlayed = false;
                    players[i].SetCurrentBet(0);
                    players[i].isAllIn = false;
                }
            }
            InvokeRepeating(nameof(DealInitialCards), 1f, 0.3f);
            phase = 0;
        }

        public virtual void StartPhase()
        {
            foreach (Player player in activePlayers)
            {
                player.SetCurrentBet(0);
                player.hasPlayed = false;
            }
            if (phaseCardNumber.ContainsKey(phase))
            {
                InvokeRepeating(nameof(DealCommon), 0f, 0.3f);
            }
            else
            {
                EndRound();
            }
        }

        public virtual void EndRound()
        {
            foreach (Player player in activePlayers)
            {
                player.RevealCards();
            }

            List<Player> winners = new List<Player>();
            if (activePlayers.Count == 1)
            {
                winners.Add(activePlayers[0]);
                UIManager.Instance.ChangeWinnerText(winners, 0000000000);
            }
            else
            {
                winners = CalculateWinner();
            }

            GiveChipToWinners(winners);
            UIManager.Instance.EndRound();
        }

        public virtual void ResetRound()
        {
            activePlayers.Clear();
            foreach (KeyValuePair<int, Player> player in players)
            {
                player.Value.ClearHand();
                if (!player.Value.hasLost)
                {
                    player.Value.VerifyLoseCondition();
                }
            }
            ReorderPlayers();
            int initialCount = players.Count;
            ClearCommonPool();
            pot = 0;
            currentMaxBet = 0;
            cptDealInitial = 0;
            cptCommon = 0;
            cptPlayer = 1;
            phase = 0;
        }


        /// <summary>
        /// Méthode utilisé afin d'ajouter une carte au pool commun
        /// </summary>
        /// <param name="card">La carte à ajouter</param>
        /// <param name="facedDown">Si la carte doit être donné face caché</param>
        public virtual void AddCardToCommonPool(Card card, bool facedDown)
        {
            List<Card> commonPoolList = commonPool as List<Card>;

            Sprite cardSprite;
            if (facedDown)
            {
                cardSprite = faceDownCardSprite;
            }
            else
            {
                cardSprite = card.GetFaceUpSprite();
            }

            commonPoolList.Add(card);
            commonPoolRenderers[commonPoolList.Count - 1].sprite = cardSprite;
            
            //Animation de distribution de cartes
            Vector3 initialPosition = commonPoolRenderers[commonPoolList.Count - 1].transform.position;
            Quaternion initialRotation = commonPoolRenderers[commonPoolList.Count - 1].transform.rotation;
            commonPoolRenderers[commonPoolList.Count - 1].transform.position = Table.Instance.GetDealerTransform().position;
            commonPoolRenderers[commonPoolList.Count - 1].transform.rotation = Table.Instance.GetDealerTransform().rotation;

            StartCoroutine(PlaceCardInPositionAnimation(commonPoolRenderers[commonPoolList.Count - 1].transform, initialPosition, initialRotation));
        }

        public virtual void ClearCommonPool()
        {
            ((List<Card>)commonPool).Clear();
            foreach (SpriteRenderer renderer in commonPoolRenderers)
            {
                renderer.sprite = null;
            }
        }

        public virtual void StartBetPhase()
        {
            currentMaxBet = 0;
            Player firstPlayer = playerOrder[0];
            int firstPlayerIndex = firstPlayer.playerNumber;
                    
            
            if (phase != 0)
            {
                firstPlayer = players[firstPlayerIndex];
                for(int i = firstPlayerIndex;i < firstPlayerIndex + players.Count; i++)
                {
                    if (activePlayers.Contains(players[i]))
                    {
                        firstPlayer = players[((i - 1) % players.Count) + 1];
                        break;
                    }
                }
            }

            if(firstPlayer.isAllIn)
            {
                firstPlayer = GetNextActivePlayer(firstPlayer);
            }

            if (!firstPlayer.hasPlayed)
            {
                UIManager.Instance.StartBetPhase(firstPlayer);
            }
            else
            {
                GoToNextPhase();
            }
        }

        public void GoToNextPlayerBet(Player nextPlayer, bool getNext = false)
        {
            if(getNext)
            {
                nextPlayer = GetNextActivePlayer(nextPlayer);
            }
            bool isTurnFinished = true;

            foreach(Player player in activePlayers)
            {
                if(!player.isAllIn && (player.GetCurrentBet() != currentMaxBet || !player.hasPlayed))
                {
                    isTurnFinished = false;
                }
            }
            if(activePlayers.Count == 1)
            {
                EndRound();
            }
            else if (isTurnFinished)
            {
                GoToNextPhase();
            }
            else
            {
                UIManager.Instance.BetPhase(nextPlayer);
            }
        }

        void GoToNextPhase()
        {
            UIManager.Instance.HideBetPhase();
            phase++;
            StartPhase();
        }

        public Sprite GetFaceDownCardSprite()
        {
            return faceDownCardSprite;
        }

        protected Player GetNextActivePlayer(Player currentPlayer)
        {
            int currentPlayerIndex =  activePlayers.IndexOf(currentPlayer);
            Player nextPlayer = activePlayers[(currentPlayerIndex + 1) % activePlayers.Count];
            if (nextPlayer.isAllIn && !nextPlayer.hasPlayed)
            {
                nextPlayer.hasPlayed = true;
                return GetNextActivePlayer(nextPlayer);
            }
            
            return nextPlayer;
        }

        public void SetCurrentMaxBet(int value)
        {
            currentMaxBet = value;
        }

        public int GetCurrentMaxBet()
        {
            return currentMaxBet;
        }

        public void RemoveFromPlayerOrder(Player player)
        {
            playerOrder.Remove(player);
        }

        protected void ReorderPlayers()
        {
            Player firstPlayer = playerOrder[0];
            playerOrder.RemoveAt(0);
            playerOrder.Add(firstPlayer);
        }

        public Transform GetDealerTransform()
        {
            return dealerTransform;
        }

        public IEnumerator PlaceCardInPositionAnimation(Transform cardtransform, Vector2 targetPosition, Quaternion targetRotation)
        {
            while (Vector2.Distance(cardtransform.position, targetPosition) > 0.000001f)
            {
                cardtransform.position = Vector2.MoveTowards(cardtransform.position, targetPosition, 0.1f);
                cardtransform.rotation = Quaternion.RotateTowards(cardtransform.rotation, targetRotation, 30f);
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void StartNewRound()
        {
            ResetRound();
            DefineWinner();
            StartRound();
        }

        public void FoldPlayer(Player player)
        {
            Player nextPlayer = GetNextActivePlayer(player);
            activePlayers.Remove(player);
            GoToNextPlayerBet(nextPlayer);
        }

        #region Poker
        public void DealCommon()
        {
            if (cptCommon < phaseCardNumber[phase])
            {
                Deal(deck.Pop(), false);
                cptCommon++;
            }
            else
            {
                CancelInvoke(nameof(DealCommon));
                cptCommon = 0;
                StartBetPhase();
            }
        }

        public void DealInitialCards()
        {
            if (cptPlayer > players.Count)
            {
                cptPlayer = 1;
                cptDealInitial++;
            }

            if (cptDealInitial < PLAYER_CARD_NUMBER)
            {
                while (players[cptPlayer].hasLost && cptPlayer < players.Count)
                {
                    cptPlayer++;
                }
                Deal(deck.Pop(), players[cptPlayer], true);
                cptPlayer++;
            }
            else
            {
                CancelInvoke(nameof(DealInitialCards));
                StartBetPhase();
            }
        }
        #endregion
    }
}
