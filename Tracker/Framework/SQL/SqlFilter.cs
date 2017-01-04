using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.SQL
{
    public class SqlFilter
    {
        private List<string> _selects = new List<string>();
        public List<string> Selects
        {
            get { return _selects; }
            set { _selects = value; }
        }

        private Where _wheres = new Where();
        public Where Wheres
        {
            get { return _wheres; }
            set { _wheres = value; }
        }


        private List<string> _orders = new List<string>();
        public List<string> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        }


        private List<string> _groups = new List<string>();
        public List<string> Groups
        {
            get { return _groups; }
            set { _groups = value; }
        }

        private List<string> _joins = new List<string>();
        public List<string> Joins
        {
            get { return _joins; }
            set { _joins = value; }
        }

        private Dictionary<string, string> _parmenters = new Dictionary<string, string>();
        public Dictionary<string, string> Parmenters
        {
            get { return _parmenters; }
            set { _parmenters = value; }
        }


        private List<string> _binds = new List<string>();
        public List<string> Binds
        {
            get { return _binds; }
            set { _binds = value; }
        }


        public int Limit { get; set; }
       
        public int Page { get; set; }

        public int PageSize { get; set; }

        private Dictionary<string, string> _sp = new Dictionary<string, string>();
        public Dictionary<string, string> SP
        {
            get { return _sp; }
            set { _sp = value; }
        }

        public string Identiy { get; set; }

        public bool ForceRefresh { get; set; }

        public int CacheLevel { get; set; }

        public string CacheKey { get; set; }
    }

    public struct SqlJoin
    {
        public string tablename;
        public string columname;
        public string targettable;
        public string targetcolumn;

        public string key
        {
            get { return (targettable + "," + targetcolumn).Trim().ToLower(); }
        }


    }


    public struct OrderByItem
    {
        public bool IsAlias;
        public string Name;
        public OrderByType Type;
        public string Table;

        public OrderByItem(string name, OrderByType type, string table)
        {
            this.Name = name;
            this.Table = table;
            this.Type = type;
            this.IsAlias = false;
        }

        public OrderByItem(string name, OrderByType type)
        {
            this.Type = type;
            this.Name = name;
            this.Table = string.Empty;
            this.IsAlias = true;
        }
    }

    public struct GroupByItem
    {        
        public string Name;        
        public string Table;

        public GroupByItem(string name, string table)
        {
            this.Name = name;
            this.Table = table;
            
        }

        public GroupByItem(string name)
        {            
            this.Name = name;
            this.Table = string.Empty;
            
        }
    }

    public class Where
    {
        private Dictionary<string, string> _and = new Dictionary<string, string>();
        public Dictionary<string, string> And
        {
            get { return _and; }
            set { _and = value; }
        }

        private Dictionary<string, string> _or = new Dictionary<string, string>();
        public Dictionary<string, string> OR
        {
            get { return _or; }
            set { _or = value; }
        }
    }

    public enum OrderByType
    {
        ASC = 0,
        DESC,
        RANDOM
    }  
}
