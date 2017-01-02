using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.SQL;

namespace Framework.Couchbase
{
    public class N1lqDAO<T> where T: new()
    {
        protected delegate void SelectFilterDelegate();
        protected delegate void FilterDelegate(string value);

        protected Dictionary<string, FilterDelegate> WhereMethods = new Dictionary<string, FilterDelegate>();
        protected Dictionary<string, SelectFilterDelegate> SelectMethods = new Dictionary<string, SelectFilterDelegate>();

        protected List<string> SelectPart = new List<string>();
        protected List<string> WherePart = new List<string>();
        protected Dictionary<string, object> Parameters = new Dictionary<string, object>();
        protected List<OrderByItem> OrderByPart = new List<OrderByItem>();
        protected List<GroupByItem> GroupByPart = new List<GroupByItem>();
        protected string tableName;


        protected Dictionary<string, SqlJoin> _tablejoin = new Dictionary<string, SqlJoin>();
        public Dictionary<string, SqlJoin> TableJoin
        {
            get { return _tablejoin; }
            set { _tablejoin = value; }
        }


        public void AddJoin(string tableName, string columnName, string targetTable, string targetColumn)
        {
            SqlJoin join = new SqlJoin { tablename = tableName, columname = columnName, targettable = targetTable, targetcolumn = targetColumn };

            if (!TableJoin.ContainsKey(join.key))
                TableJoin.Add(join.key, join);

        }

        public void AddOrderBy(string tableName, string columnName, OrderByType sortType)
        {
            OrderByItem orderBy = new OrderByItem { Table = tableName, Name = columnName, Type = sortType };

            OrderByPart.Add(orderBy);

        }

        public void AddGroupBy(string tableName, string columnName)
        {
            GroupByItem groupBy = new GroupByItem { Table = tableName, Name = columnName };

            GroupByPart.Add(groupBy);

        }

        public void AddGroupBy(string columnName)
        {
            GroupByItem groupBy = new GroupByItem { Table = "", Name = columnName };

            GroupByPart.Add(groupBy);

        }

        public void AddParameter(string parameterName, object value)
        {
            Parameters.Add(parameterName, value);
        }


        //build select

        protected string TableName
        {
            get
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    tableName = typeof(T).Name;
                    tableName = tableName.Substring(0, tableName.Length - 2);

                    return tableName;
                }
                else
                    return tableName;
            }
            set
            {
                tableName = value;
            }
        }

        protected virtual void BuildOrderBy(List<string> order)
        { }

        protected virtual void BuildGroupBy(List<string> group)
        { }

        protected virtual void BuildJoin(List<string> join)
        { }

        protected virtual void LateBind(List<string> binds, T to)
        { }


        private void _buildFromAndWhere(SqlFilter filter, out string strFrom, out string strWhere)
        {
            strWhere = " where ";

            if (filter.Wheres.And.Count > 0)
            {
                foreach (KeyValuePair<string, string> where in filter.Wheres.And)
                {
                    if (where.Value != null && where.Value.Contains("@"))
                        WhereMethods[where.Key](filter.Parmenters[where.Value]);
                    else
                        WhereMethods[where.Key](where.Value);
                }

                if (WherePart.Count > 0)
                {
                    strWhere += string.Join(" and ", WherePart.ToArray());
                }
            }


            bool flag = WherePart.Count > 0;

            if (filter.Wheres.OR.Count > 0)
            {
                WherePart.Clear();

                foreach (KeyValuePair<string, string> where in filter.Wheres.OR)
                {
                    WhereMethods[where.Key](where.Value);
                }

                if (WherePart.Count > 0)
                {
                    if (flag)
                        strWhere += " and ( ";

                    strWhere += string.Join(" or ", WherePart.ToArray());

                    if (flag)
                        strWhere += " )";
                }
            }

            if (strWhere == " where ")
                strWhere = "";

            strFrom = " from ";
            strFrom += TableName + " r ";

            foreach (KeyValuePair<string, SqlJoin> join in TableJoin)
            {
                string format = string.Format("inner join {0} on r.{1} = {0}.{2}", join.Value.targettable, join.Value.columname, join.Value.targetcolumn);
                strFrom += format;
            }
        }

        private string _buildNonePage(SqlFilter filter)
        {
            foreach (string select in filter.Selects)
            {
                SelectMethods[select]();
            }

            BuildOrderBy(filter.Orders);

            BuildGroupBy(filter.Groups);

            BuildJoin(filter.Joins);

            string strWhere = "", strFrom = "";


            StringBuilder sql = new StringBuilder();

            sql.Append("select ");
            

            sql.Append(string.Join(",", SelectPart.ToArray()));


            _buildFromAndWhere(filter, out strFrom, out strWhere);


            sql.Append(strFrom).Append(strWhere);


            if (GroupByPart.Count > 0)
            {
                sql.Append(" group by ");

                for (int i = 0; i < GroupByPart.Count; i++)
                {
                    GroupByItem item = GroupByPart[i];

                    if (i > 0)
                        sql.Append(",");

                    if (!string.IsNullOrEmpty(item.Table))
                        sql.Append(item.Table).Append(".").Append(item.Name);
                    else
                        sql.Append(item.Name);

                }
            }

            if (OrderByPart.Count > 0)
            {
                sql.Append(" order by ");

                for (int i = 0; i < OrderByPart.Count; i++)
                {
                    OrderByItem item = OrderByPart[i];

                    if (i > 0)
                        sql.Append(",");

                    switch (item.Type)
                    {
                        case OrderByType.ASC:
                            sql.Append(item.Table).Append(".").Append(item.Name);
                            sql.Append(" asc ");
                            break;

                        case OrderByType.DESC:
                            sql.Append(item.Table).Append(".").Append(item.Name);
                            sql.Append(" desc ");
                            break;

                        case OrderByType.RANDOM:
                            sql.Append(" newid() ");
                            break;
                    }
                }
            }

            if (filter.Limit > 0)
                sql.Append(" limit " + filter.Limit.ToString() + " ");


            return sql.ToString();
        }


        private string _buildHasPage(SqlFilter filter)
        {

            foreach (string select in filter.Selects)
            {
                SelectMethods[select]();
            }

            BuildOrderBy(filter.Orders);

            BuildGroupBy(filter.Groups);

            BuildJoin(filter.Joins);

            string strWhere = "", strFrom = "";


            StringBuilder sql = new StringBuilder();

            sql.Append("select ");


            sql.Append(string.Join(",", SelectPart.ToArray()));


            _buildFromAndWhere(filter, out strFrom, out strWhere);


            sql.Append(strFrom).Append(strWhere);


            if (GroupByPart.Count > 0)
            {
                sql.Append(" group by ");

                for (int i = 0; i < GroupByPart.Count; i++)
                {
                    GroupByItem item = GroupByPart[i];

                    if (i > 0)
                        sql.Append(",");

                    if (!string.IsNullOrEmpty(item.Table))
                        sql.Append(item.Table).Append(".").Append(item.Name);
                    else
                        sql.Append(item.Name);

                }
            }

            if (OrderByPart.Count > 0)
            {
                sql.Append(" order by ");

                for (int i = 0; i < OrderByPart.Count; i++)
                {
                    OrderByItem item = OrderByPart[i];

                    if (i > 0)
                        sql.Append(",");

                    switch (item.Type)
                    {
                        case OrderByType.ASC:
                            sql.Append(item.Table).Append(".").Append(item.Name);
                            sql.Append(" asc ");
                            break;

                        case OrderByType.DESC:
                            sql.Append(item.Table).Append(".").Append(item.Name);
                            sql.Append(" desc ");
                            break;

                        case OrderByType.RANDOM:
                            sql.Append(" newid() ");
                            break;
                    }
                }
            }

            if (filter.Page > 0)
                sql.Append(" limit " + filter.PageSize.ToString() + " ");

            sql.Append(" offset ").Append(((filter.Page - 1) * filter.PageSize));

            return sql.ToString();
        }

        private string _buildCount(SqlFilter filter)
        {
            //foreach (string select in filter.Selects)
            //{
            //    SelectMethods[select]();
            //}

            //BuildOrderBy(filter.Orders);

            BuildGroupBy(filter.Groups);

            BuildJoin(filter.Joins);

            string strWhere = "", strFrom = "";


            StringBuilder sql = new StringBuilder();

            
            sql.Append("select count(*) total");

            if (filter.Limit > 0 && filter.Page == 0)
                sql.Append(" top " + filter.Limit.ToString() + " ");


            //sql.Append(string.Join(",", SelectPart.ToArray()));


            _buildFromAndWhere(filter, out strFrom, out strWhere);


            sql.Append(strFrom).Append(strWhere);


            if (GroupByPart.Count > 0)
            {
                sql.Append(" group by ");

                for (int i = 0; i < GroupByPart.Count; i++)
                {
                    GroupByItem item = GroupByPart[i];

                    if (i > 0)
                        sql.Append(",");

                    if (!string.IsNullOrEmpty(item.Table))
                        sql.Append(item.Table).Append(".").Append(item.Name);
                    else
                        sql.Append(item.Name);

                }
            }
            

            return sql.ToString();
        }

        public string BuildSelectSql(SqlFilter filter)
        {
            SelectPart.Clear();
            WherePart.Clear();
            TableJoin.Clear();
            OrderByPart.Clear();
            Parameters.Clear();

            //if (filter.SP.Count > 0)
            //{
            //    if (filter.Parmenters.Count > 0 && !string.IsNullOrEmpty(filter.SP.First().Value))
            //    {
            //        string[] parms = filter.SP.First().Value.Split(',');

            //        foreach (string parm in parms)
            //        {
            //            if (filter.Parmenters.ContainsKey("@" + parm))
            //            {
            //                Parameters["@" + parm] = new SqlParameter("@" + parm, filter.Parmenters["@" + parm]);
            //            }
            //        }
            //    }

            //    return filter.SP.First().Key;
            //}


            string sql = "";

            if (filter.Page > 0)
                sql = _buildHasPage(filter);
            else
                sql = _buildNonePage(filter);

            return sql.ToString();

        }

        public string BuildSelectCountSql(SqlFilter filter)
        {
            SelectPart.Clear();
            WherePart.Clear();
            TableJoin.Clear();
            OrderByPart.Clear();
            Parameters.Clear();


            string sql = "";

            sql = _buildCount(filter);

            return sql.ToString();

        }

        public List<T> GetItems(SqlFilter filter)
        {
            string sql = BuildSelectSql(filter);

            //KeyValuePair<string, object>[] arrayParameter = Parameters.ToArray();

            //build couchbase key
            //string[] strParms = Parameters.Values.Select(p => { 
            //    if (p.SqlDbType == SqlDbType.Int)
            //        return p.ParameterName + "=" + p.SqlValue.ToString();
            //    else
            //        return p.ParameterName + "=\"" + p.SqlValue.ToString()+"\"";
            //}).ToArray();


            //string key = !string.IsNullOrEmpty(filter.Identiy) ? filter.Identiy : typeof(T).Name + "_" + MD5Class.GetMD5ValueUTF8(sql) + "_" + string.Join(",", strParms);

            //CouchbaseClient client = CouchbaseHelper.CreateInstance();

            //if (filter.ForceRefresh || (!string.IsNullOrEmpty(RequestContext.Current.request["reloadcache"]) && RequestContext.Current.request["reloadcache"] == "1"))
            //{
            //    client.Remove(key);
            //}



            //if (!string.IsNullOrEmpty(filter.CacheKey))
            //    key = filter.CacheKey;

            //if (client.KeyExists(key))
            //    list = client.Get<List<T>>(key);
            //else

            CouchbaseHelper helper = new CouchbaseHelper("default");
            List<T> list = helper.Query<T>(sql, Parameters);



            //if (list != null)
            //{
            //    foreach (T to in list)
            //    {
            //        LateBind(filter.Binds, to);
            //    }

            //    if (filter.CacheLevel >0)
            //    {

            //        client.Store(StoreMode.Set, key, list, TransCacheLevel(filter.CacheLevel));
            //    }
            //}



            return list;
        }

        public T GetItem(SqlFilter filter)
        {
            filter.Limit = 1;
            List<T> arr = GetItems(filter);
            T to = default(T);
            if (arr.Count == 1)
            {
                to = arr[0];

            }
            return to;
        }

        public Info GetItemsInfo(SqlFilter filter)
        {
            string sql = BuildSelectCountSql(filter);

            CouchbaseHelper helper = new CouchbaseHelper("default");
            List<Info> list = helper.Query<Info>(sql, Parameters);

            Info info = list[0];

            return info;
        }

        //build insert
        //public int AutoInsert(T to)
        //{
        //    SqlParameter[] parameters = null;// new SqlParameter[0];
        //    string sql = SqlActionBuilder<T>.BuildInsertSql(to, ref parameters);

        //    int result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringLeap, CommandType.Text, sql, parameters);

        //    return result;
        //}


        //public int AutoInsertReturnId(T to)
        //{
        //    SqlParameter[] parameters = null;// new SqlParameter[0];
        //    string sql = SqlActionBuilder<T>.BuildInsertSqlReturnId(to, ref parameters);

        //    int result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringLeap, CommandType.Text, sql, parameters);

        //    return result;
        //}

        ////build update

        //public int AutoUpdate(T to)
        //{
        //    SqlParameter[] parameters = null;// new SqlParameter[0];
        //    string sql = SqlActionBuilder<T>.BuildUpdateSql(to, ref parameters);

        //    int result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringLeap, CommandType.Text, sql, parameters);

        //    return result;
        //}


        //public int AutoUpdate(T to,string filterColumns)
        //{
        //    SqlParameter[] parameters = null;// new SqlParameter[0];
        //    string sql = SqlActionBuilder<T>.BuildUpdateSql(to, filterColumns, ref parameters);

        //    int result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringLeap, CommandType.Text, sql, parameters);

        //    return result;
        //}


        ////build delete

        //public int AutoDelete(T to)
        //{
        //    string sql = SqlActionBuilder<T>.BuildDeleteSql(to);

        //    int result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringLeap, CommandType.Text, sql, null);

        //    return result;
        //}



    }

    public class Info
    {
        public int total { get; set; }
    }
}
