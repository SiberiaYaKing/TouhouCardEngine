﻿using System;
using System.Linq;
using System.Collections.Generic;
using TouhouCardEngine.Interfaces;
using NitoriNetwork.Common;
using TouhouCardEngine.Shared;
namespace TouhouCardEngine
{
    public abstract class Rule
    {
        public CardDefine[] defines { get; }
        public Rule(CardDefine[] defines)
        {
            this.defines = defines;
        }
        public abstract void onGameStart(CardEngine game, RoomPlayerInfo[] playersInfo);
    }
    [Serializable]
    public partial class CardEngine : IGame
    {
        #region 公共成员
        public CardEngine(int randomSeed = 0, params CardDefine[] defines)
        {
            trigger = new SyncTriggerSystem(this);
            random = new Random(randomSeed);
            foreach (CardDefine define in defines)
            {
                addDefine(define);
            }
        }
        public ITimeManager time { get; set; } = null;
        public ITriggerManager triggers { get; set; } = null;
        public SyncTriggerSystem trigger { get; }
        IAnswerManager _answers;
        public IAnswerManager answers
        {
            get { return _answers; }
            set
            {
                if (_answers != null)
                    _answers.onResponse -= onAnswer;
                _answers = value;
                if (_answers != null)
                    _answers.onResponse += onAnswer;
            }
        }
        public ILogger logger { get; set; }
        #endregion
        #region 状态
        public Rule rule { get; }
        public Pile this[string pileName]
        {
            get { return getPile(pileName); }
        }
        public void addPile(Pile pile)
        {
            pileList.Add(pile);
            pile.owner = null;
            foreach (Card card in pile)
            {
                card.owner = null;
            }
        }
        public Pile getPile(string name)
        {
            return pileList.FirstOrDefault(e => { return e.name == name; });
        }
        public Pile[] getPiles()
        {
            return pileList.ToArray();
        }
        private List<Pile> pileList { get; } = new List<Pile>();
        //public Task<ISetPropEventArg> setProp<T>(IGame game, string propName, T value)
        //{
        //    if (game != null && game.triggers != null)
        //        return game.triggers.doEvent<ISetPropEventArg>(new SetPropEventArg() { card = this, propName = propName, beforeValue = getProp<T>(game, propName), value = value }, arg =>
        //        {
        //            Card card = arg.card as Card;
        //            propName = arg.propName;
        //            var v = arg.value;
        //            propDic[propName] = v;
        //            game.logger?.log("Game", card + "的" + propName + "=>" + propToString(v));
        //            return Task.CompletedTask;
        //        });
        //    else
        //    {
        //        propDic[propName] = value;
        //        return Task.FromResult<ISetPropEventArg>(default);
        //    }
        //}
        //public class SetPropEventArg : EventArg, ISetPropEventArg
        //{
        //    public Card card;
        //    public string propName;
        //    public object beforeValue;
        //    public object value;
        //    ICard ISetPropEventArg.card => card;
        //    string ISetPropEventArg.propName => propName;
        //    object ISetPropEventArg.beforeValue => beforeValue;
        //    object ISetPropEventArg.value => value;
        //}
        //public T getProp<T>(IGame game, string propName)
        //{
        //    T value = default;
        //    if (propDic.ContainsKey(propName) && propDic[propName] is T t)
        //        value = t;
        //    foreach (var modifier in modifierList.OfType<PropModifier<T>>().Where(mt =>
        //        mt.propName == propName &&
        //        (game == null || mt.checkCondition(game, this))))
        //    {
        //        value = modifier.calc(game, this, value);
        //    }
        //    return (T)(object)value;
        //}
        //public object getProp(IGame game, string propName)
        //{
        //    object value = default;
        //    if (propDic.ContainsKey(propName))
        //        value = propDic[propName];
        //    foreach (var modifier in modifierList.Where(m =>
        //        m.propName == propName &&
        //        (game == null || m.checkCondition(game, this))))
        //    {
        //        value = modifier.calc(game, this, value);
        //    }
        //    return value;
        //}
        //internal Dictionary<string, object> propDic { get; } = new Dictionary<string, object>();
        #endregion
        #region 游戏流程
        public void start(Rule rule, RoomPlayerInfo[] playersInfo)
        {
            rule.onGameStart(this, playersInfo);
        }
        #endregion
        public virtual void onAnswer(IResponse response)
        {
        }
        #region CardDefine
        public void addDefine(CardDefine define)
        {
            if (cardDefineDic.ContainsKey(define.id))
                throw new ConflictDefineException(cardDefineDic[define.id], define);
            cardDefineDic.Add(define.id, define);
        }
        public T getDefine<T>() where T : CardDefine
        {
            foreach (var pair in cardDefineDic)
            {
                if (pair.Value is T t)
                    return t;
            }
            throw new UnknowDefineException(typeof(T));
        }
        public CardDefine getDefine(int id)
        {
            if (cardDefineDic.ContainsKey(id))
                return cardDefineDic[id];
            else
                throw new UnknowDefineException(id);
        }
        public T getDefine<T>(int id) where T : CardDefine
        {
            if (cardDefineDic.ContainsKey(id) && cardDefineDic[id] is T t)
                return t;
            else
                throw new UnknowDefineException(id);
        }
        public CardDefine[] getDefines()
        {
            return cardDefineDic.Values.ToArray();
        }
        public CardDefine[] getDefines(IEnumerable<int> idCollection)
        {
            return idCollection.Select(id => getDefine(id)).ToArray();
        }
        Dictionary<int, CardDefine> cardDefineDic { get; } = new Dictionary<int, CardDefine>();
        #endregion
        #region Card
        public virtual Card createCard(CardDefine define)
        {
            int id = cardDic.Count + 1;
            while (cardDic.ContainsKey(id))
                id++;
            Card card = new Card(id, define);
            cardDic.Add(id, card);
            return card;
        }
        public Card createCardById(int id)
        {
            CardDefine define = getDefine(id);
            if (define == null)
                throw new NoCardDefineException(id);
            return createCard(define);
        }
        public Card getCard(int id)
        {
            if (cardDic.TryGetValue(id, out var card))
                return card;
            else
                return null;
        }
        public Card[] getCards(int[] ids)
        {
            return ids.Select(id => getCard(id)).ToArray();
        }
        Dictionary<int, Card> cardDic { get; } = new Dictionary<int, Card>();
        #endregion
        public T getProp<T>(string varName)
        {
            if (dicVar.ContainsKey(varName) && dicVar[varName] is T)
                return (T)dicVar[varName];
            return default;
        }
        public void setProp<T>(string propName, T value)
        {
            dicVar[propName] = value;
        }
        public void setProp(string propName, PropertyChangeType changeType, int value)
        {
            if (changeType == PropertyChangeType.set)
                dicVar[propName] = value;
            else if (changeType == PropertyChangeType.add)
                dicVar[propName] = getProp<int>(propName) + propName;
        }
        public void setProp(string propName, PropertyChangeType changeType, float value)
        {
            if (changeType == PropertyChangeType.set)
                dicVar[propName] = value;
            else if (changeType == PropertyChangeType.add)
                dicVar[propName] = getProp<float>(propName) + propName;
        }
        public void setProp(string propName, PropertyChangeType changeType, string value)
        {
            if (changeType == PropertyChangeType.set)
                dicVar[propName] = value;
            else if (changeType == PropertyChangeType.add)
                dicVar[propName] = getProp<string>(propName) + propName;
        }
        internal Dictionary<string, object> dicVar { get; } = new Dictionary<string, object>();
        public int registerCard(Card card)
        {
            dicCard.Add(dicCard.Count + 1, card);
            card.id = dicCard.Count;
            return card.id;
        }
        public int[] registerCards(Card[] cards)
        {
            return cards.Select(c => { return registerCard(c); }).ToArray();
        }
        Dictionary<int, Card> dicCard { get; } = new Dictionary<int, Card>();
        public Player getPlayerAt(int playerIndex)
        {
            return (0 <= playerIndex && playerIndex < playerList.Count) ? playerList[playerIndex] : null;
        }
        public int getPlayerIndex(Player player)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i] == player)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 获取所有玩家，玩家在数组中的顺序与玩家被添加的顺序相同。
        /// </summary>
        /// <remarks>为什么不用属性是因为每次都会生成一个数组。</remarks>
        public Player[] getPlayers()
        {
            return playerList.ToArray();
        }
        public int playerCount
        {
            get { return playerList.Count; }
        }
        public void addPlayer(Player player)
        {
            playerList.Add(player);
        }
        protected int getNewPlayerId()
        {
            int id = playerList.Count;
            if (playerList.Any(p => p.id == id))
                id++;
            return id;
        }
        private List<Player> playerList { get; } = new List<Player>();
        public delegate void EventAction(Event @event);
        public event EventAction beforeEvent;
        public event EventAction afterEvent;
        Event currentEvent { get; set; } = null;
        List<Event> eventList { get; } = new List<Event>();
        /// <summary>
        /// 随机整数1~max
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public int dice(int max)
        {
            return randomInt(1, max);
        }
        /// <summary>
        /// 随机整数，注意该函数返回的值可能包括最大值与最小值。
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>介于最大值与最小值之间，可能为最大值也可能为最小值</returns>
        public int randomInt(int min, int max)
        {
            return random.Next(min, max + 1);
        }
        /// <summary>
        /// 随机实数，注意该函数返回的值可能包括最小值，但是不包括最大值。
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>介于最大值与最小值之间，不包括最大值</returns>
        public float randomFloat(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }
        Random random { get; set; }
    }
    public enum EventPhase
    {
        logic = 0,
        before,
        after
    }
}