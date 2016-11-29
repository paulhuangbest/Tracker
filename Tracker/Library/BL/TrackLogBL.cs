using Couchbase;
using Entity;
using Library.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.BL
{
    public class TrackLogBL
    {
        CouchbaseHelper helper = new CouchbaseHelper("default");

        public bool UpsertSystemLog(SystemLog log)
        {
            var doc = new Document<SystemLog>()
            {
                Id = log.LogId,
                Content = log
            };


            return helper.Upsert<SystemLog>(doc);
        }
    }
}
