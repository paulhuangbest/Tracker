using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.SQL;
using Framework.Couchbase;
using Entity;

namespace Library.DAL.DAO
{
    public class ExceptionLogDAO : N1lqDAO<ExceptionLog>
    {
        public ExceptionLogDAO()
        {
            TableName = "default";

            SelectMethods.Add("List", new SelectFilterDelegate(this.Select_List));

            //================================================================================

            WhereMethods.Add("CreateTime", new FilterDelegate(this.Where_CreateTime));
            WhereMethods.Add("Keyword", new FilterDelegate(this.Where_Keyword));
            WhereMethods.Add("Subkey", new FilterDelegate(this.Where_Subkey));
            WhereMethods.Add("Type", new FilterDelegate(this.Where_Type));
            WhereMethods.Add("Level", new FilterDelegate(this.Where_Level));
            WhereMethods.Add("ProjectKey", new FilterDelegate(this.Where_ProjectKey));
        }


        protected void Select_List()
        {

            SelectPart.Add("r.logId,r.requestIP,r.tag,r.createTime,r.subKey");

        }

        protected void Where_CreateTime(string value)
        {

            if (!string.IsNullOrEmpty(value))
            {
                string[] dates = value.Split(',');

                if (!string.IsNullOrEmpty(dates[0]) && !string.IsNullOrEmpty(dates[1]))
                {
                    WherePart.Add("r.createTime > $date1 and r.createTime < $date2");
                    AddParameter("$date1", dates[0].Trim().Replace(" ", "T"));
                    AddParameter("$date2", dates[1].Trim().Replace(" ", "T"));
                }
            }
            

        }

        protected void Where_Keyword(string value)
        {

            if (!string.IsNullOrEmpty(value))
            {

                WherePart.Add("r.exceptionMessage like $keyword");
                AddParameter("$keyword", "%" + value + "%");
                
            }


        }

        protected void Where_Subkey(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {

                WherePart.Add("r.subKey = $subkey");
                AddParameter("$subkey", value);

            }
        }

        protected void Where_Type(string value)
        {
            WherePart.Add("r.type = $type");
            AddParameter("$type", value);

        }

        protected void Where_Level(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {

                WherePart.Add("r.tag = $level");
                AddParameter("$level", value);

            }
        }

        protected void Where_ProjectKey(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {

                WherePart.Add("r.projectKey = $projectkey");
                AddParameter("$projectkey", value);

            }
        }

        protected override void BuildOrderBy(List<string> orders)
        {
            foreach (string item in orders)
            {
                switch (item)
                {
                    case "CreateTime":
                        AddOrderBy("r", "createTime", OrderByType.ASC);
                        break;
                }
            }
        }
    }
}
