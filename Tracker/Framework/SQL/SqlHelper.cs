using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Web;


namespace Framework.SQL
{
    public class SqlTransConn: IDisposable
    {
        public SqlConnection Conn = null;
        public SqlTransaction Trans = null;
        public int TransCount = 0;
        //public string ConnString = null;

        public SqlTransConn(string connectionString)
        {
            Conn = new SqlConnection(connectionString);
            Conn.Open();
            //ConnString = connectionString;
        }

        public SqlTransConn Commit()
        {
            TransCount--;
            if (TransCount == 0)
            {
                Trans.Commit();
            }
            return this;
        }

        public SqlTransConn Rollback()
        {
            if (Trans != null && TransCount > 0)
            {
                Trans.Rollback();
                Trans = null;
                TransCount = 0;
            }
            return this;
        }

        public SqlTransConn BeginTransaction()
        {
            if (TransCount == 0)
            {
                if (Conn.State == ConnectionState.Closed)
                {
                    Conn.Open();
                }
                Trans = Conn.BeginTransaction();
            }
            TransCount++;
            return this;
        }

        public SqlTransConn Close()
        {
            // do not close if transaction is still going on
            if (TransCount > 0) return this;

            if (Conn.State == ConnectionState.Open)
            {
                Conn.Close();
            }
            return this;
        }

        public SqlCommand CreateCommand()
        {
            SqlCommand cmd = Conn.CreateCommand();
            if (Trans != null) cmd.Transaction = Trans;
            return cmd;
        }

        public bool IsInTransaction()
        {
            return (TransCount > 0);
        }

        /// <summary>
        /// if this object is disposed in memory, close the underlining connection
        /// </summary>
        public void Dispose()
        {
            if (Conn!= null && Conn.State == ConnectionState.Open)
            {
                Conn.Dispose();
            }
        }
    }

    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, 
    /// scalable best practices for common uses of SqlClient.
    /// </summary>
    public abstract class SqlHelper
    {
        public static readonly string ConnStringLeap = ConfigurationManager.ConnectionStrings["Leap"] != null ? ConfigurationManager.ConnectionStrings["Leap"].ConnectionString : "";
        public static readonly string ConnStringQJ = ConfigurationManager.ConnectionStrings["QJ"] != null ? ConfigurationManager.ConnectionStrings["QJ"].ConnectionString : "";
        public static readonly string ConnStringYH = ConfigurationManager.ConnectionStrings["ConnectionString"] != null ? ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString : "";
        //public static readonly string ConnStringBackOffice = ConfigurationManager.ConnectionStrings["BackOffice"].ConnectionString;

        //public static readonly string ConnStringReadOnlyDB = ConfigurationManager.ConnectionStrings["ReadOnly"].ConnectionString ?? ConnStringLeap; //if readonly not found, use openrice instead

        //public static readonly string ConnStringMessage = ConfigurationManager.ConnectionStrings["Message"].ConnectionString;

        public static readonly int LongCommandTimeout = 1200; // in seconds, i.e. 10 minutes
        public static readonly int DefaultCommandTimeout = 30; // in seconds


        public static SqlTransaction SQLTrans
        {
            get
            {
                SqlConnection conn = new SqlConnection(ConnStringLeap);
                conn.Open();
                return conn.BeginTransaction();
            }
        }

        public static SqlConnection GetSqlConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        // Hashtable to store cached parameters
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            //SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = GetSqlConnection(connectionString))
            {
                return ExecuteNonQuery(conn, cmdType, cmdText, commandParameters);
                //PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                //int val = cmd.ExecuteNonQuery();
                //cmd.Parameters.Clear();
                //return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandTimeOut">the command timeout</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText,int commandTimeOut, params SqlParameter[] commandParameters)
        {

            //SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = GetSqlConnection(connectionString))
            {
                return ExecuteNonQuery(conn, cmdType, cmdText, commandTimeOut, commandParameters);

                //cmd.CommandTimeout = commandTimeOut;
                //PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                //int val = cmd.ExecuteNonQuery();
                //cmd.Parameters.Clear();
                //return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandTimeOut">the command timeout</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, int commandTimeOut, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = commandTimeOut;
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        public static int ExecuteNonQuery(SqlTransConn transConn, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteNonQuery(transConn, cmdType, null, cmdText, commandParameters);
        }

        public static int ExecuteNonQuery(SqlTransConn transConn, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, transConn, cmdType, commandTimeOut, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteNonQuery(trans, cmdType, null, cmdText, commandParameters);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) using an existing SQL Transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">an existing sql transaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, commandTimeOut, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        public static DataTable FillDataTable(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return FillDataTable(connectionString, cmdType, null, cmdText, commandParameters);
        }

        public static DataTable FillDataTable(string connectionString, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();

            using (SqlConnection conn = GetSqlConnection(connectionString))
            {
                da.SelectCommand = cmd;
                PrepareCommand(cmd, conn, null, cmdType, commandTimeOut, cmdText, commandParameters);

                da.Fill(dt);
                cmd.Parameters.Clear();

                return dt;
            }

        }

        public static DataTable FillDataTable(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return FillDataTable(trans, cmdType, null, cmdText, commandParameters);
        }

        public static DataTable FillDataTable(SqlTransaction trans, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();

            da.SelectCommand = cmd;
            PrepareCommand(cmd, trans.Connection, trans, cmdType, commandTimeOut, cmdText, commandParameters);

            da.Fill(dt);
            cmd.Parameters.Clear();

            return dt;
            
        }

        public static DataTable FillDataTable(SqlTransConn transConn, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return FillDataTable(transConn, cmdType, null, cmdText, commandParameters);
        }

        public static DataTable FillDataTable(SqlTransConn transConn, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();

            da.SelectCommand = cmd;
            PrepareCommand(cmd, transConn, cmdType, commandTimeOut, cmdText, commandParameters);

            da.Fill(dt);
            cmd.Parameters.Clear();

            return dt;

        }

        public static DataSet FillDataSet(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return FillDataSet(connectionString, cmdType, null, cmdText, commandParameters);
        }

        public static DataSet FillDataSet(string connectionString, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();

            using (SqlConnection conn = GetSqlConnection(connectionString))
            {
                da.SelectCommand = cmd;
                PrepareCommand(cmd, conn, null, cmdType, commandTimeOut, cmdText, commandParameters);

                da.Fill(ds);
                cmd.Parameters.Clear();

                return ds;
            }

        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlConnection conn = GetSqlConnection(connectionString);
            return ExecuteReader(conn, cmdType, cmdText, commandParameters);

        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(SqlConnection conn, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw ex;
            }
        }

        public static SqlDataReader ExecuteReader(SqlTransConn transConn, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, transConn, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(!transConn.IsInTransaction() ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, int CommandTimeOut, params SqlParameter[] commandParameters)
        {
            SqlConnection conn = GetSqlConnection(connectionString);
            return ExecuteReader(conn, cmdType, cmdText, CommandTimeOut, commandParameters);


        }
        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(SqlConnection conn, CommandType cmdType, string cmdText, int CommandTimeOut, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = CommandTimeOut;
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw ex;
            }
        }

        public static SqlDataReader ExecuteReader(SqlTransConn transConn, CommandType cmdType, string cmdText, int CommandTimeOut, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = CommandTimeOut;
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, transConn, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(transConn.IsInTransaction() ? CommandBehavior.Default: CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
// no need to close, let outside handle
//                transConn.Close();
                throw ex;
            }
        }

        public static XmlDocument ExecuteXmlReader(SqlConnection conn, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteXmlReader(conn, cmdType, null, cmdText, commandParameters);
        }

        public static XmlDocument ExecuteXmlReader(SqlConnection conn, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            try
            {
                PrepareCommand(cmd, conn, null, cmdType, commandTimeOut, cmdText, commandParameters);
                XmlReader rdr = cmd.ExecuteXmlReader();

                XmlDocument doc = new XmlDocument();
                doc.Load(rdr);
                rdr.Close();
                cmd.Parameters.Clear();
                return doc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }


        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteScalar(connectionString, cmdType, null, cmdText, commandParameters);
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            //SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = GetSqlConnection(connectionString))
            {
                return ExecuteScalar(conn, cmdType, LongCommandTimeout, cmdText, commandParameters);
                //PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                //object val = cmd.ExecuteScalar();
                //cmd.Parameters.Clear();
                //return val;
            }
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteScalar(connection, cmdType, null, cmdText, commandParameters);
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, cmdType, commandTimeOut, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            //DebugLog.Log(DebugLogArea.SQL, cmdText);
            return val;
        }

        public static object ExecuteScalar(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteScalar(trans, cmdType, null, cmdText, commandParameters);
        }

        public static object ExecuteScalar(SqlTransaction trans, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, trans.Connection, trans, cmdType, commandTimeOut, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;

        }

        public static object ExecuteScalar(SqlTransConn transConn, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteScalar(transConn, cmdType, null, cmdText, commandParameters);
        }

        public static object ExecuteScalar(SqlTransConn transConn, CommandType cmdType, int? commandTimeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, transConn, cmdType, null, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;

        }

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="cacheKey">Key to the parameter cache</param>
        /// <param name="cmdParms">an array of SqlParamters to be cached</param>
        public static void CacheParameters(string cacheKey, params SqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve cached parameters
        /// </summary>
        /// <param name="cacheKey">key used to lookup parameters</param>
        /// <returns>Cached SqlParamters array</returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            SqlParameter[] clonedParms = new SqlParameter[cachedParms.Length];

            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            PrepareCommand(cmd, conn, trans, cmdType, null, cmdText, cmdParms);
        }

        /// <summary>
        /// Prepare a command for execution
        /// </summary>
        /// <param name="cmd">SqlCommand object</param>
        /// <param name="conn">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, int? cmdTimeout, string cmdText, SqlParameter[] cmdParms)
        {

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;

            if (cmdTimeout != null)
                cmd.CommandTimeout = cmdTimeout.Value;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        private static void PrepareCommand(SqlCommand cmd, SqlTransConn transConn, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            PrepareCommand(cmd, transConn, cmdType, null, cmdText, cmdParms);
        }

        private static void PrepareCommand(SqlCommand cmd, SqlTransConn transConn, CommandType cmdType, int? cmdTimeout, string cmdText, SqlParameter[] cmdParms)
        {

            SqlConnection conn = transConn.Conn;

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (transConn.Trans != null)
                cmd.Transaction = transConn.Trans;

            cmd.CommandType = cmdType;

            if (cmdTimeout != null)
                cmd.CommandTimeout = cmdTimeout.Value;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
    }
}