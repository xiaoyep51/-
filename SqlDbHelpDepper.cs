using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CommonHelper
{
    public class SqlDbHelpDepper
    {
        // 1原房友数据库
        private static string Constr1 = ConfigurationManager.AppSettings["WuHanConstr"].ToString();
        private static string Constr2 = ConfigurationManager.AppSettings["WuHanJWebConstr"].ToString();
        protected string sqlconnection { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConType">0 微信数据库 1 原房友数据库</param>
        /// <returns></returns>
        public SqlConnection OpenConnection(int ConType)
        {
            if (ConType == 0)
            {
                sqlconnection = "";
            }
            else if (ConType == 1)
            {
                sqlconnection = Constr1;
            }
            else if (ConType == 2)
            {
                sqlconnection = Constr2;
            }
            SqlConnection connection = new SqlConnection(sqlconnection);  //这里sqlconnection就是数据库连接字符串
            connection.Open();
            return connection;
        }
    }
}