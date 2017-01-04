using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Serialization;


namespace Framework.SQL
{

    public class BaseDAO<T>  where T:BaseTO,new()
    {

        protected delegate void SelectFilterDelegate();
        protected delegate void FilterDelegate(string value);

        protected Dictionary<string, FilterDelegate> WhereMethods = new Dictionary<string, FilterDelegate>();
        protected Dictionary<string, SelectFilterDelegate> SelectMethods = new Dictionary<string, SelectFilterDelegate>();

        protected List<string> SelectPart = new List<string>();
        protected List<string> WherePart = new List<string>();
        protected Dictionary<string, SqlParameter> Parameters = new Dictionary<string, SqlParameter>();
        protected List<OrderByItem> OrderByPart = new List<OrderByItem>();
        protected List<GroupByItem> GroupByPart = new List<GroupByItem>();
        protected string tableName;


        protected Dictionary<string, SqlJoin> _tablejoin = new Dictionary<string, SqlJoin>();
        public Dictionary<string, SqlJoin> TableJoin
        {
            get { return _tablejoin; }
            set { _tablejoin = value; }
        }


        public void AddJoin(string tableName, string columnName, string targetTable,string targetColumn)
        {
            SqlJoin join = new SqlJoin { tablename = tableName, columname = columnName, targettable = targetTable, targetcolumn = targetColumn };

            if (!TableJoin.ContainsKey(join.key))
                TableJoin.Add(join.key,join);
            
        }

        public void AddOrderBy(string tableName, string columnName, OrderByType sortType)
        {
            OrderByItem orderBy = new OrderByItem { Table = tableName, Name = columnName, Type=sortType };

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

        public void AddParameter(string parameterName, SqlDbType dbtype, object value)
        {
            AddParameter(parameterName, dbtype, null, value);
        }

        public void AddParameter(string parameterName, SqlDbType dbtype, int? size, object value)
        {
            SqlParameter p = new SqlParameter(parameterName, dbtype);
            p.Value = value;

            if (size != null)
                p.Size = size.Value;

            Parameters.Add(parameterName, p);
            
        }
        //build select

        protected string TableName
        {
            get {
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

        protected virtual void LateBind(List<string> binds,T to)
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

            if (filter.Limit > 0 && filter.Page == 0)
                sql.Append(" top " + filter.Limit.ToString() + " ");


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

            return sql.ToString();
        }


        private string _buildHasPage(SqlFilter filter)
        {
            foreach (string select in filter.Selects)
            {
                SelectMethods[select]();
            }

            BuildOrderBy(filter.Orders);

            string strWhere = "", strFrom = "", orderPart = "";

            StringBuilder sql = new StringBuilder();

            sql.Append("with result as ( ");

            sql.Append("select ");

            if (filter.Limit > 0 && filter.Page > 0)
            {
                if (OrderByPart.Count > 0)
                {
                    sql.Append(" ROW_NUMBER() OVER (ORDER BY ");

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

                    sql.Append(" ) AS 'RowNumber' ");
                }
            }

            sql.Append(string.Join(",", SelectPart.ToArray()));

            _buildFromAndWhere(filter, out strFrom, out strWhere);

            sql.Append(strFrom).Append(strWhere);

            sql.Append(" ); "); //end with result
            sql.Append(" select * from result where RowNumber between ").Append(((filter.Page - 1) * 15) + 1).Append(" and ").Append(filter.Page * 15);
            return sql.ToString();
        }

        public string BuildSelectSql(SqlFilter filter)
        {
            SelectPart.Clear();
            WherePart.Clear();
            TableJoin.Clear();
            OrderByPart.Clear();
            Parameters.Clear();

            if (filter.SP.Count>0)
            {
                if (filter.Parmenters.Count > 0 && !string.IsNullOrEmpty(filter.SP.First().Value))
                {
                    string[] parms = filter.SP.First().Value.Split(',');

                    foreach (string parm in parms)
                    {
                        if (filter.Parmenters.ContainsKey("@"+parm))
                        {
                            Parameters["@" + parm] = new SqlParameter("@" + parm, filter.Parmenters["@" + parm]);
                        }
                    }
                }

                return filter.SP.First().Key;
            }


            string sql = "";

            if (filter.Page > 0)
                sql = _buildHasPage(filter);
            else
                sql = _buildNonePage(filter);

            return sql.ToString();

        }


        public List<T> GetItems(SqlFilter filter)
        {
            string sql = BuildSelectSql(filter);

            SqlParameter[] arrayParameter = Parameters.Values.ToArray<SqlParameter>();

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
            
            
            List<T> list = null;

            //if (!string.IsNullOrEmpty(filter.CacheKey))
            //    key = filter.CacheKey;

            //if (client.KeyExists(key))
            //    list = client.Get<List<T>>(key);
            //else
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringYH, CommandType.Text, sql, arrayParameter);

                if (reader.HasRows)
                {
                    list = new List<T>();

                    while (reader.Read())
                    {
                        T to = new T();
                        to.Initialize(reader);

                        list.Add(to);
                    }
                }

                reader.Close();
                reader.Dispose();

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

            }

            return list;
        }

        public T GetItem(SqlFilter filter)
        {
            filter.Limit = 1;
            List<T> arr = GetItems(filter);
            T to = null;
            if (arr.Count == 1)
            {
                to = arr[0];

            }
            return to;
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

        private DateTime TransCacheLevel(int level)
        {
            switch (level)
            {
                default:
                case 1:
                    return DateTime.Now.AddMinutes(3);
                    break;

                case 2:
                    return DateTime.Now.AddMinutes(5);
                    break;

                case 3:
                    return DateTime.Now.AddMinutes(10);
                    break;

                case 4:
                    return DateTime.Now.AddMinutes(30);
                    break;

                case 5:
                    return DateTime.Now.AddMinutes(60);
                    break;

                case 6:
                    return DateTime.Now.AddHours(5);
                    break;

                case 7:
                    return DateTime.Now.AddHours(10);
                    break;
            }

           
        }
    }
}
