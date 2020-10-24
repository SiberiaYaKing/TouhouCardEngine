﻿using System.Linq;
using TouhouCardEngine.Interfaces;
namespace TouhouCardEngine
{
    /// <summary>
    /// 卡片定义，包含了一张卡的静态数据和效果逻辑。
    /// </summary>
    public abstract class CardDefine : ICardDefine
    {
        /// <summary>
        /// 卡片定义ID，这个ID应该是独特的并用于区分不同的卡片。
        /// </summary>
        public abstract int id { get; set; }
        public abstract string type { get; set; }
        public virtual IEffect[] effects { get; set; } = new IEffect[0];
        public object this[string propName]
        {
            get { return getProp<object>(propName); }
        }
        public virtual T getProp<T>(string propName)
        {
            if (propName == nameof(id))
                return (T)(object)id;
            else
                return default;
        }
        public virtual void setProp<T>(string propName, T value)
        {
            if (propName == nameof(id))
                id = (int)(object)value;
        }
        /// <summary>
        /// 将读取到的更新的卡牌数据合并到这个卡牌上来。
        /// </summary>
        /// <param name="newVersion"></param>
        public abstract void merge(CardDefine newVersion);
        public abstract string isUsable(CardEngine engine, Player player, Card card);
        public IActiveEffect getActiveEffect()
        {
            return effects.FirstOrDefault(e => e is IActiveEffect) as IActiveEffect;
        }
        public ITriggerEffect getEffectOn<T>(ITriggerManager manager) where T : IEventArg
        {
            return effects.FirstOrDefault(e => e is ITriggerEffect te && te.getEvents(manager).Contains(manager.getName<T>())) as ITriggerEffect;
        }
        public ITriggerEffect getEffectAfter<T>(ITriggerManager manager) where T : IEventArg
        {
            return effects.FirstOrDefault(e => e is ITriggerEffect te && te.getEvents(manager).Contains(manager.getNameAfter<T>())) as ITriggerEffect;
        }
        public override bool Equals(object obj)
        {
            if (obj is CardDefine other)
                return id == other.id;
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return id;
        }
    }

    /// <summary>
    /// 忽略这张卡
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class IgnoreCardDefineAttribute : System.Attribute
    {
    }
}