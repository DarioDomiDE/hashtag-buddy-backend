﻿namespace AutoTagger.Common
{
    using AutoTagger.Contract;

    public class MachineTag : IMachineTag
    {
        public MachineTag()
        {
        }

        public MachineTag(string name, float score, string source)
        {
            this.Name   = name;
            this.Score  = score;
            this.Source = source;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public float Score { get; set; }

        public string Source { get; set; }
    }
}
