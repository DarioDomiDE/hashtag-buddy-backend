﻿namespace Instaq.Database.Storage.Mysql.Query
{
    using Instaq.Contract;
    using Instaq.Contract.Models;

    public abstract class FindHumanoidTagsQueryBase : IFindHumanoidTagsQuery
    {
        public abstract string GetQuery(IMachineTag[] machineTags);
        protected const int RefCountLimit = 30000;

        protected static string BuildWhereConditions(IMachineTag[]  machineTags)
        {
            return BuildWhereCondition(machineTags, "GCPVision_Label");
        }

        private static string BuildWhereCondition(IMachineTag[] machineTags, string source)
        {
            var where = "";
            for (var i = 0; i < machineTags.Length; i++)
            {
                var machineTag = machineTags[i];
                if (machineTag.Source != source || string.IsNullOrEmpty(machineTag.Name))
                {
                    continue;
                }
                where += $"`m`.`name` = '{machineTag.Name.Replace("'", @"\'")}' OR ";
            }

            char[] charsToTrim = { ' ', 'O', 'R' };
            return where.TrimEnd(charsToTrim);
        }
    }
}
