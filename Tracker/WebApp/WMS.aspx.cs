﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using TrackerClient;

namespace WebApp
{
    public partial class WMS : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Client.ProjectKey = "WMS";
        }


        protected void btnOperate_Click(object sender, EventArgs e)
        {
            Client.OperateLog("paulhuang", "test", "f1");
        }

        protected void btnException_Click(object sender, EventArgs e)
        {
            try
            {
                throw new Exception("test");
            }
            catch(Exception ex)
            {
                Client.ExceptionLog(ex);
            }
            
        }


    }
}