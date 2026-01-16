using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Utilities;

namespace BlueWombMod.Common.Utilities;

public sealed class WeightedAttackPool<T> where T : notnull
{
    private sealed record class AttackDefinition(T Item, double Weight, params Func<bool>[] Include)
    {
        public bool CanBePicked { get; set; }

        public double PickChance { get; set; } = 1.0;
    }

    private readonly List<AttackDefinition> Attacks = [];

    public int Count => Attacks.Count;

    public void Add(T item, double weight, params Func<bool>[] include)
    {
        Attacks.Add(new AttackDefinition(item, weight, include));
    }

    public void Remove(T item)
    {
        Attacks.RemoveAll(item.Equals);
    }

    public void Clear()
    {
        Attacks.Clear();
    }

    public double GetChance(T item)
    {
        return Attacks.FirstOrDefault(n => n.Item.Equals(item), null)?.PickChance ?? 0;
    }

    public T PickFromTop(int count = -1, double weightAdjustment = 0.1)
    {
        var random = new WeightedRandom<AttackDefinition>(Main.rand);

        var i = 0;
        if (count < 1 || count >= Attacks.Count)
        {
            count = Attacks.Count - 1;
        }

        while (count > 0)
        {
            if (Attacks[i].Weight > 0 && Attacks[i].Include.All(x => x.Invoke()))
            {
                count--;
                Attacks[i].CanBePicked = true;
                random.Add(Attacks[i], Attacks[i].PickChance * Attacks[i].Weight);
            }

            if (++i >= Attacks.Count)
            {
                break;
            }
        }

        var pickedAttack = random.Get();

        if (pickedAttack is null)
        {
            return default(T);
        }

        pickedAttack.CanBePicked = false;

        Attacks.Remove(pickedAttack);

        foreach (var attack in Attacks)
        {
            if (attack.CanBePicked)
            {
                attack.CanBePicked = false;
                attack.PickChance += weightAdjustment;
            }
        }

        pickedAttack.PickChance = Math.Max(pickedAttack.PickChance - weightAdjustment, 0);

        Attacks.Add(pickedAttack);

        return pickedAttack.Item;
    }
}