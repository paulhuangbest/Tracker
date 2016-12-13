using Couchbase;
using Couchbase.N1QL;
using Couchbase.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Common
{
    public class CouchbaseHelper
    {
        private static Cluster Cluster = new Cluster("couchbaseClients/couchbase");

        private string BucketName{get;set;}

        public CouchbaseHelper(string bucketName)
        {
            BucketName = bucketName;
        }


        public T GetValue<T>(string key) 
        {
            using (var bucket = Cluster.OpenBucket(BucketName))
            {
                if (bucket.Exists(key))
                    return bucket.Get<T>(key).Value;
                else
                    return default(T);
            }
        }


        public T GetDocument<T>(string key)
        {
            using (var bucket = Cluster.OpenBucket(BucketName))
            {
                if (bucket.Exists(key))
                    return bucket.GetDocument<T>(key).Content;
                else
                    return default(T);
            }
            
        }


        public bool Upsert<T>(string key, T obj)
        {
            using (var bucket = Cluster.OpenBucket(BucketName))
            {
                return bucket.Upsert<T>(key, obj).Success;
            }
        }


        public bool Upsert<T>(IDocument<T> obj)
        {
            using (var bucket = Cluster.OpenBucket(BucketName))
            {
                return bucket.Upsert<T>(obj).Success;
            }
        }


        public bool Remove(string key)
        {
            using (var bucket = Cluster.OpenBucket(BucketName))
            {
                return bucket.Remove(key).Success;
            }
        }


        public List<T> Query<T>(string n1ql)
        {
            using (var bucket = Cluster.OpenBucket(BucketName))
            {
                var queryRequest = new QueryRequest()
                               .Statement(n1ql)
                               .Metrics(false);

                var result = bucket.Query<T>(queryRequest);

                if (result.Success)
                    return result.Rows;
                else
                    return new List<T>();

            }
        }

        
    }
}
