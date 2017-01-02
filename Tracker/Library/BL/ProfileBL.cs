using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Common;
using Entity;
using Couchbase;
using Library.DAL;
using Framework.Couchbase;


namespace Library.BL
{
    public class ProfileBL
    {
        CouchbaseHelper helper = new CouchbaseHelper("TrackInfo");

        public CoreProfile GetProfile(string key)
        {
            
            return helper.GetValue<CoreProfile>(key);

        }

        public bool UpsertProfile(CoreProfile profile)
        {
            var doc= new Document<CoreProfile>()
            {
                Id=profile.ProfileKey,
                Content = profile
            };


            return helper.Upsert<CoreProfile>(doc);
        }

        public List<CoreProfile> GetProfileList()
        {
            ProfileDAL dal = new ProfileDAL();

            return dal.GetProfileList();
        }

        public bool RemoveProfile(string key)
        {
            return helper.Remove(key);
        }
    }
}
