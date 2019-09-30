﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TouhouHeartstone
{
    /// <summary>
    /// Pile（牌堆）表示一个可以容纳卡片的有序集合，比如卡组，手牌，战场等等。一个Region中可以包含可枚举数量的卡牌。
    /// 注意，卡片在Region中的顺序代表了它的位置。0是最左边（手牌），0也是最底部（卡组）。
    /// </summary>
    public class Pile : IEnumerable<Card>
    {
        public Pile()
        {
            name = null;
        }
        public Pile(string name = null, Card[] cards = null)
        {
            this.name = name;
            if (cards == null)
                return;
            foreach (Card card in cards)
            {
                card.pile = this;
            }
            cardList.AddRange(cards);
        }
        public Player owner { get; set; } = null;
        public string name { get; } = null;
        /// <summary>
        /// 将一张不属于任何牌堆的卡牌插入该牌堆。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="position"></param>
        internal void insert(Card card, int position)
        {
            if (card.pile == null)
            {
                card.pile = this;
                cardList.Insert(position, card);
            }
            else
                throw new InvalidOperationException(card + "已经属于Pile[" + card.pile.name + "]");
        }
        /// <summary>
        /// 将位于该牌堆中的一张牌移动到其他的牌堆中。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="targetPile"></param>
        /// <param name="position"></param>
        public void moveTo(Card card, Pile targetPile, int position)
        {
            if (cardList.Remove(card))
            {
                card.pile = targetPile;
                targetPile.cardList.Insert(position, card);
            }
        }
        public void moveTo(Card[] cards, Pile targetPile, int position)
        {
            List<Card> removedCardList = new List<Card>(cards.Length);
            foreach (Card card in cards)
            {
                if (cardList.Remove(card))
                {
                    card.pile = targetPile;
                    removedCardList.Add(card);
                }
            }
            targetPile.cardList.InsertRange(position, removedCardList);
        }
        /// <summary>
        /// 将该牌堆中的一些卡换成其他牌堆中的另一些卡。
        /// </summary>
        /// <param name="originalCards"></param>
        /// <param name="replacedCards"></param>
        public void replace(Card[] originalCards, Card[] replacedCards)
        {
            if (originalCards.Length != replacedCards.Length)
                throw new IndexOutOfRangeException("originalCards与replacedCards数量不一致");
            for (int i = 0; i < originalCards.Length; i++)
            {
                int originIndex = indexOf(originalCards[i]);
                if (originIndex < 0)
                    throw new InvalidOperationException(originalCards[i] + "不在" + this + "中");
                else
                {
                    int replaceIndex = replacedCards[i].pile.indexOf(replacedCards[i]);
                    this[originIndex] = replacedCards[i];
                    replacedCards[i].pile[replaceIndex] = originalCards[i];
                    originalCards[i].pile = replacedCards[i].pile;
                    replacedCards[i].pile = this;
                }
            }
        }
        /// <summary>
        /// 将牌堆中的一些牌与目标牌堆中随机的一些牌相替换。
        /// </summary>
        /// <param name="engine">用于提供随机功能的引擎</param>
        /// <param name="originalCards">要进行替换的卡牌</param>
        /// <param name="pile">目标牌堆</param>
        /// <param name="shuffle">在进行随机替换之前是否先将要替换的卡牌洗入目标牌堆？</param>
        public void replaceByRandom(CardEngine engine, Card[] originalCards, Pile pile, bool shuffle)
        {
            if (shuffle)
            {
                int[] indexArray = new int[originalCards.Length];
                for (int i = 0; i < originalCards.Length; i++)
                {
                    //把牌放回去
                    pile.cardList.Add(originalCards[i]);
                    originalCards[i].pile = pile;
                    //记录当前牌堆中的空位
                    cardList[i] = null;
                    indexArray[i] = indexOf(originalCards[i]);
                }
                for (int i = 0; i < indexArray.Length; i++)
                {
                    //将牌堆中的随机卡片填入空位
                    int targetIndex = engine.randomInt(0, pile.count - 1);
                    cardList[indexArray[i]] = pile.cardList[targetIndex];
                    cardList[indexArray[i]].pile = this;
                    //并将其从牌堆中移除
                    pile.cardList.RemoveAt(targetIndex);
                }
            }
            else
            {
                Card[] replacedCards = new Card[originalCards.Length];
                List<int> indexList = new List<int>();
                for (int i = 0; i < pile.count; i++)
                {
                    indexList.Add(i);
                }
                for (int i = 0; i < replacedCards.Length; i++)
                {
                    int targetIndexOfIndex = engine.randomInt(0, indexList.Count - 1);//好他妈绕啊。
                    replacedCards[i] = pile[indexList[targetIndexOfIndex]];
                    indexList.RemoveAt(targetIndexOfIndex);
                }
                replace(originalCards, replacedCards);
            }
        }
        internal void remove(Card card)
        {
            if (cardList.Remove(card))
            {
                card.pile = null;
            }
        }
        public void shuffle(CardEngine engine)
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                int index = engine.randomInt(i, cardList.Count - 1);
                Card card = cardList[i];
                cardList[i] = cardList[index];
                cardList[index] = card;
            }
        }
        public Card top
        {
            get
            {
                if (cardList.Count < 1)
                    return null;
                return cardList[cardList.Count - 1];
            }
        }
        public int indexOf(Card card)
        {
            return cardList.IndexOf(card);
        }
        public int count
        {
            get { return cardList.Count; }
        }
        public Card this[int index]
        {
            get { return cardList[index]; }
            internal set
            {
                cardList[index] = value;
            }
        }
        public Card[] this[int startIndex, int endIndex]
        {
            get
            {
                return cardList.GetRange(startIndex, endIndex - startIndex + 1).ToArray();
            }
            internal set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    cardList[startIndex + i] = value[i];
                }
            }
        }
        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)cardList).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Card>)cardList).GetEnumerator();
        }
        internal List<Card> cardList { get; } = new List<Card>();
        public override string ToString()
        {
            return name + "[" + cardList.Count + "]";
        }
        public static implicit operator Pile[](Pile pile)
        {
            if (pile != null)
                return new Pile[] { pile };
            else
                return new Pile[0];
        }
        public static implicit operator Card[](Pile pile)
        {
            if (pile != null)
                return pile.cardList.ToArray();
            else
                return new Card[0];
        }
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}