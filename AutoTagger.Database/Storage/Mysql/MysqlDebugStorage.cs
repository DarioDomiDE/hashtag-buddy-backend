﻿namespace AutoTagger.Database.Storage.Mysql
{
    using System.Collections.Generic;
    using System.Linq;

    using AutoTagger.Contract.Models;
    using AutoTagger.Contract.Storage;

    public class MysqlDebugStorage : MysqlBaseStorage, IDebugStorage
    {

        public string GetPhotosCount()
        {
            var query = "SELECT count(*) as count from photos";
            var results = this.ExecuteCustomQuery(query);
            return results?.FirstOrDefault()?.FirstOrDefault();
        }

        public string GetHumanoidTagsCount()
        {
            var query = "SELECT count(*) FROM itags WHERE";
            var results = this.ExecuteCustomQuery(query);
            return results?.FirstOrDefault()?.FirstOrDefault();
        }

        public string GetHumanoidTagRelationCount()
        {
            var query = "SELECT count(*) from photo_itag_rel";
            var results = this.ExecuteCustomQuery(query);
            return results?.FirstOrDefault()?.FirstOrDefault();
        }

        public string GetMachineTagsCount()
        {
            var query = "SELECT count(distinct m.photoId) from mtags as m";
            var results = this.ExecuteCustomQuery(query);
            return results?.FirstOrDefault()?.FirstOrDefault();
        }

        public IEnumerable<ILog> GetLogs()
        {
            return this.db.Debug
                //.Where(x => x.Deleted == 0)
                .OrderByDescending(x => x.Id)
                .Select(x => x.ToLog());
        }

    }

}
