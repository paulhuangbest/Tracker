using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Interface;
using Library.Common;
using Library.DAL.DAO;
using Entity;
using Framework.Couchbase;
using Framework.SQL;

namespace Library.DAL
{
    public class WatcherDAL : IWatcherDAL
    {
        private int _pageSize = 5;

        public List<TotalDTO> GetTotalWithAllType(DateTime currentDate, string projectKey)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select type LogType, count(*) total from default 
                            where createTime like $createTime and projectKey = $projectKey
                            group by type
                            order by type";

            Dictionary<string, object> args = new Dictionary<string, object>();

            args["$projectKey"] = projectKey;
            args["$createTime"] = currentDate.ToString("yyyy-MM-dd") + "%";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql, args);

            return result;
        }


        public List<TotalDTO> GetTotalWithAllTypeMonth(DateTime currentDate, string projectKey)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select type LogType, count(*) total from default 
                            where createTime like $createTime and projectKey = $projectKey
                            group by type
                            order by type";

            Dictionary<string, object> args = new Dictionary<string, object>();

            args["$projectKey"] = projectKey;
            args["$createTime"] = currentDate.ToString("yyyy-MM") + "%";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql, args);

            return result;
        }

        public List<TotalDTO> GetSystemTotalByDate(DateTime currentDate, string projectKey)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select DATE_PART_STR(createTime,'day') day , count(*) total from default 
                            where createTime like $createTime and type = '3' and statusCode like '2%' and projectKey =$projectKey
                            group by DATE_PART_STR(createTime,'day')
                            order by DATE_PART_STR(createTime,'day')";


            Dictionary<string, object> args = new Dictionary<string, object>();

            args["$projectKey"] = projectKey;
            args["$createTime"] = currentDate.ToString("yyyy-MM") + "%";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql, args);

            return result;
        }

        public List<TotalDTO> GetExceptionTotalByDate(DateTime currentDate, string projectKey)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select DATE_PART_STR(createTime,'day') day , count(*) total from default 
                            where createTime like $createTime and type = '1' and projectKey =$projectKey
                            group by DATE_PART_STR(createTime,'day')
                            order by DATE_PART_STR(createTime,'day')";

            Dictionary<string, object> args = new Dictionary<string, object>();

            args["$projectKey"] = projectKey;
            args["$createTime"] = currentDate.ToString("yyyy-MM") + "%";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql, args);

            return result;
        }

        public List<TotalDTO> GetOperateTotalByDate(DateTime currentDate, string projectKey)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select DATE_PART_STR(createTime,'day') day , count(*) total from default 
                            where createTime like $createTime and type = '2' and action = 'test' and projectKey =$projectKey
                            group by DATE_PART_STR(createTime,'day')
                            order by DATE_PART_STR(createTime,'day')";


            Dictionary<string, object> args = new Dictionary<string, object>();

            args["$projectKey"] = projectKey;
            args["$createTime"] = currentDate.ToString("yyyy-MM") + "%";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql, args);

            return result;
        }

        public List<TotalDTO> GetNormalTotalByDate(DateTime currentDate, string projectKey)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select DATE_PART_STR(createTime,'day') day , count(*) total from default 
                            where createTime like '2016-12%' and type = '4' and subKey = 'IIS'
                            group by DATE_PART_STR(createTime,'day')
                            order by DATE_PART_STR(createTime,'day')";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql);

            return result;


        }

        public List<Entity.SystemLog> SearchSystemLog(Dictionary<string, string> condition)
        {
            throw new NotImplementedException();
        }

        public List<Entity.ExceptionLog> SearchExecptionLog(Dictionary<string, string> condition)
        {
            //CouchbaseHelper helper = new CouchbaseHelper("default");

            //string n1ql = @"select tag,serverIP,createTime,subKey from default where subKey=$subKey and createTime > '2016-12-06T00:29:57.451' and createTime < '2016-12-06T00:30:05.229' limit 2 offset 1";

            //List<ExceptionLog> result = helper.Query<ExceptionLog>(n1ql);

            ExceptionLogDAO dao = new ExceptionLogDAO();

            SqlFilter filter = new SqlFilter();

            filter.Selects.Add("List");

            filter.Wheres.And.Add("CreateTime", condition["CreateTime"]);

            filter.Wheres.And.Add("Keyword", condition["Keyword"]);

            filter.Wheres.And.Add("Subkey", condition["Subkey"]);

            filter.Wheres.And.Add("Level", condition["Level"]);

            filter.Wheres.And.Add("ProjectKey", condition["ProjectKey"]);

            filter.Wheres.And.Add("Type", "1");

            filter.Orders.Add("CreateTime");


            filter.PageSize = _pageSize;
            filter.Page = int.Parse(condition["Page"]);

            List<ExceptionLog> result = dao.GetItems(filter);

            return result;
        }

        public List<Entity.OperateLog> SearchOperateLog(Dictionary<string, string> condition)
        {
            throw new NotImplementedException();
        }

        public List<Entity.NormalLog> SearchNormalLog(Dictionary<string, string> condition)
        {
            throw new NotImplementedException();
        }

        public Entity.SystemLog GetSystemLog(string logId)
        {
            throw new NotImplementedException();
        }

        public Entity.OperateLog GetOperateLog(string logId)
        {
            throw new NotImplementedException();
        }

        public Entity.ExceptionLog GetExceptionLog(string logId)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            ExceptionLog log = helper.GetDocument<ExceptionLog>(logId);

            return log;
        }

        public Entity.NormalLog GetNormalLog(string logId)
        {
            throw new NotImplementedException();
        }


        public Info SearchInfo(Dictionary<string, string> condition)
        {
            ExceptionLogDAO dao = new ExceptionLogDAO();

            SqlFilter filter = new SqlFilter();

            filter.Wheres.And.Add("CreateTime", condition["CreateTime"]);

            filter.Wheres.And.Add("Keyword", condition["Keyword"]);

            filter.Wheres.And.Add("Subkey", condition["Subkey"]);

            filter.Wheres.And.Add("Level", condition["Level"]);

            filter.Wheres.And.Add("Type", "1");

            Info result = dao.GetItemsInfo(filter);

            return result;
        }


        public List<ExceptionLog> DefaultExecptionLogList(string projectKey)
        {
            ExceptionLogDAO dao = new ExceptionLogDAO();

            SqlFilter filter = new SqlFilter();

            filter.Selects.Add("List");

            filter.Limit = 20;

            filter.Wheres.And.Add("Type", "1");

            filter.Wheres.And.Add("ProjectKey", projectKey);

            filter.Orders.Add("CreateTime");

            List<ExceptionLog> result = dao.GetItems(filter);

            return result;
        }

        public List<OperateLog> DefaultOperateLogList(string projectKey)
        {
            throw new NotImplementedException();
        }

        public List<SystemLog> DefaultSystemLogList(string projectKey)
        {
            throw new NotImplementedException();
        }

        public List<NormalLog> DefaultNormalLogList(string projectKey)
        {
            throw new NotImplementedException();
        }


        public List<TrackLog> GetTimelineData(LogType type, Dictionary<string, string> condition)
        {
            List<TrackLog> final = new List<TrackLog>();

            switch (type)
            {
                case LogType.ExceptionLog:
                    ExceptionLogDAO dao = new ExceptionLogDAO();

                    SqlFilter filter = new SqlFilter();

                    filter.Selects.Add("TimeLine");

                    filter.Wheres.And.Add("CreateTime", condition["CreateTime"]);

                    filter.Wheres.And.Add("Keyword", condition["Keyword"]);

                    filter.Wheres.And.Add("Subkey", condition["Subkey"]);

                    filter.Wheres.And.Add("Level", condition["Level"]);

                    filter.Wheres.And.Add("ProjectKey", condition["ProjectKey"]);

                    filter.Wheres.And.Add("Type", "1");

                    filter.Orders.Add("CreateTime");


                    List<ExceptionLog> result = dao.GetItems(filter);

                    foreach (ExceptionLog log in result)
                    {
                        final.Add(log);
                    }

                    break;
            }



            return final;

        }
    }
}
