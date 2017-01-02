using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using Library.Common;
using Framework.Couchbase;

namespace Library.DAL
{
    public class ProfileDAL
    {
        public List<CoreProfile> GetProfileList()
        {
            string n1ql = "select enable,exceptionConsumerNum,modifyTime,mqServer,normalConsumerNum,operateConsumerNum,profileKey,projectKey,systemConsumerNum,description from TrackInfo where profileKey is not null";

            CouchbaseHelper helper = new CouchbaseHelper("TrackInfo");

            return helper.Query<CoreProfile>(n1ql);
        }
    }
}
