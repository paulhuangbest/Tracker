﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Interface;
using Library.Common;
using Entity;

namespace Library.DAL
{
    public class WatcherDAL : IWatcherDAL
    {
        public List<TotalDTO> GetTotalWithAllType(DateTime currentDate)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select type LogType, count(*) total from default 
                            where createTime like '2016-11-08%'
                            group by type
                            order by type";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql);

            return result;
        }


        public List<TotalDTO> GetTotalWithAllTypeMonth(DateTime currentDate)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select type LogType, count(*) total from default 
                            where createTime like '2016-11%'
                            group by type
                            order by type";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql);

            return result;
        }

        public List<TotalDTO> GetSystemTotalByDate(DateTime currentDate)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select DATE_PART_STR(createTime,'day') day , count(*) total from default 
                            where createTime like '2016-11%' and type = '3' and status like '2%'
                            group by DATE_PART_STR(createTime,'day')
                            order by DATE_PART_STR(createTime,'day')";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql);

            return result;
        }

        public List<TotalDTO> GetExceptionTotalByDate(DateTime currentDate)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select DATE_PART_STR(createTime,'day') day , count(*) total from default 
                            where createTime like '2016-11%' and type = '1'
                            group by DATE_PART_STR(createTime,'day')
                            order by DATE_PART_STR(createTime,'day')";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql);

            return result;
        }

        public List<TotalDTO> GetOperateTotalByDate(DateTime currentDate)
        {
            CouchbaseHelper helper = new CouchbaseHelper("default");

            string n1ql = @"select DATE_PART_STR(createTime,'day') day , count(*) total from default 
                            where createTime like '2016-11%' and type = '2' and action = 'test'
                            group by DATE_PART_STR(createTime,'day')
                            order by DATE_PART_STR(createTime,'day')";

            List<TotalDTO> result = helper.Query<TotalDTO>(n1ql);

            return result;
        }

        public List<TotalDTO> GetNormalTotalByDate(DateTime currentDate)
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Entity.NormalLog GetNormalLog(string logId)
        {
            throw new NotImplementedException();
        }


    }
}
