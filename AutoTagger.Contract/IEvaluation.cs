﻿namespace AutoTagger.Contract
{
    using System.Collections.Generic;

    public interface IEvaluation
    {
        void AddDebugInfos(string key, object value);

        Dictionary<string, object> GetDebugInfos();

        IEnumerable<IHumanoidTag> GetMostRelevantHumanoidTags(IUiStorage storage, IEnumerable<IMachineTag> mTags);

        IEnumerable<IHumanoidTag> GetTrendingHumanoidTags(
            IUiStorage storage,
            IEnumerable<IMachineTag> mTags,
            IEnumerable<IHumanoidTag> mostRelevantHTags);
    }
}
