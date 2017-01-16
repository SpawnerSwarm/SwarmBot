using System.Collections.Generic;
using System;
using Discord;

namespace SwarmBot
{
    public sealed class Rank
    {
        private readonly string name;
        private readonly int value;
        public readonly Color color;

        private static readonly Dictionary<string, Rank> instance = new Dictionary<string, Rank>();

        public static readonly Rank Recruit = new Rank(1, "Recruit", Color.Default);
        public static readonly Rank Member = new Rank(2, "Member", Color.Default);
        public static readonly Rank MemberII = new Rank(3, "Member II", Color.Default);
        public static readonly Rank Veteran = new Rank(4, "Veteran", new Color(241, 196, 15));
        public static readonly Rank Officer = new Rank(5, "Officer", new Color(52, 152, 219));
        public static readonly Rank General = new Rank(6, "General", new Color(26, 188, 156));
        public static readonly Rank GuildMaster = new Rank(7, "Guild Master", new Color(233, 30, 99));

        private Rank(int value, string name, Color color)
        {
            this.name = name;
            this.value = value;
            this.color = color;
            instance[name] = this;
        }
        public override string ToString()
        {
            return name;
        }
        public static implicit operator Rank(string str)
        {
            Rank result;
            if (instance.TryGetValue(str, out result))
                return result;
            else
                throw new InvalidCastException();
        }
        public static implicit operator Rank(int i)
        {
            Rank[] arr = new Rank[7];

            if(i <= 7) {
                instance.Values.CopyTo(arr, 0);
                return arr[i - 1];
            }
            else { throw new InvalidCastException(); }
        }
        public static implicit operator string(Rank rank)
        {
            return rank.name;
        }
        public static implicit operator int(Rank rank)
        {
            return rank.value;
        }
    }
    public sealed class DefineType
    {
        private readonly string name;
        private readonly int value;

        public static readonly DefineType Promotion = new DefineType(0, "Promotion");
        public static readonly DefineType RankCapacity = new DefineType(1, "RankCapacity");
        public static readonly DefineType LastRankUp = new DefineType(2, "LastRankUp");

        private DefineType(int value, string name)
        {
            this.name = name;
            this.value = value;
        }
        public override string ToString()
        {
            return name;
        }
        public static implicit operator string(DefineType defineType)
        {
            return defineType.ToString();
        }
    }
}