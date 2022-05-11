using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickQuant.Common;

namespace MajorControl
{
    public class MajorExchangeSymbol
    {
        public string ExchangeID { get; set; }
        public string Symbol { get; set; }
    }
    public static class MYSQLHELPER
    {
        static MysqlConfig mysqlConfig;
        //static MySqlConnection conn;
       
        static MYSQLHELPER()
        {
            if (!File.Exists("MarketConfig.json"))
            {
                throw new Exception("file config.json doesnot exist");
            }
            StreamReader streamReader = new StreamReader("MarketConfig.json");
            var jsonFile = streamReader.ReadToEnd();
            mysqlConfig = JToken.Parse(jsonFile)["mysql"].ToObject<MysqlConfig>();
            //conn = new MySqlConnection(GetConnectionString("histpair"));
            //conn.Open();
        }

        static List<MajorExchangeSymbol> QueryExchangeSymbols(string exchangeid)
        {
            string tableName = "exchangesymbol";
            List<MajorExchangeSymbol> exchangesymbols = new List<MajorExchangeSymbol>();
            string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
            string connectionString = string.Format(InitconnectionString, mysqlConfig.Ip, "setting", mysqlConfig.User, mysqlConfig.PassWord);
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string cmdStr = $"SELECT * FROM {tableName} where exchangeid= \"{exchangeid}\";"; // +exchangeid;
                    MySqlCommand candleCmd = new MySqlCommand(cmdStr, conn);
                    MySqlDataReader reader = candleCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MajorExchangeSymbol bar = new MajorExchangeSymbol()
                        {
                            ExchangeID = Convert.ToString(reader["exchangeid"]),
                            Symbol = Convert.ToString(reader["symbol"])
                        };
                        exchangesymbols.Add(bar);
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return exchangesymbols;
        }               

       
        static string GetConnectionString(string dbName)
        {
            //获取Configuration对象            
            string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
            string UserId = mysqlConfig.User;
            string PassWord = mysqlConfig.PassWord;
            string Mysql_IP = mysqlConfig.Ip;
            return string.Format(InitconnectionString, Mysql_IP, dbName, UserId, PassWord);
        }

        static string GetDataTableName(TableType tableType, string exchangeID, string symbol, string category = null)
        {
            if (tableType == TableType.metadata)
            {
                return "marketsymbolmetadata";
            }
            if (category == null)
            {
                return $"{tableType}_{exchangeID}_{symbol}".Replace('-', '_').ToLower();
            }
            else
            {
                return $"{tableType}_{exchangeID}_{symbol}_{category}".Replace('-', '_').ToLower();
            }

        }
        public static List<ArbitrageCandle> QueryKlineFromTable(string databasename, string inst, int quant)
        { 
            List<ArbitrageCandle> cs = new List<ArbitrageCandle>();
            string tableName;
            if (databasename == "histpair")
                tableName = "kline_" + inst.Replace("-", "_").ToLower().Replace(" ", "").Replace(":", "_");
            else
                tableName = "kline_" + inst.Replace("-", "_").ToLower().Replace(" ", "").Replace(":", "_") + "_m";


            string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
            string connectionString = string.Format(InitconnectionString, mysqlConfig.Ip, databasename, mysqlConfig.User, mysqlConfig.PassWord);
            try
            {
                if (!CreateTableIfNotExists(databasename, tableName, inst))
                    throw new Exception("unavailable to create table");

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    
                    string cmdStr = $"SELECT * FROM {tableName} ORDER BY ID DESC LIMIT {quant};"; 
                    MySqlCommand candleCmd = new MySqlCommand(cmdStr, conn);
                    MySqlDataReader reader = candleCmd.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        ArbitrageCandle bar;
                        if (databasename == "histpair")
                        {
                            bar = new ArbitrageCandle()
                            {
                                Close = Convert.ToDecimal(reader["Close"]),
                                SClose = Convert.ToDecimal(reader["SClose"]),
                                High = Convert.ToDecimal(reader["High"]),
                                Low = Convert.ToDecimal(reader["Low"]),
                                Open = Convert.ToDecimal(reader["Open"]),
                                SLow = Convert.ToDecimal(reader["SLow"]),
                                SHigh = Convert.ToDecimal(reader["SHigh"]),
                                SOpen = Convert.ToDecimal(reader["SOpen"]),
                                Datetime = Convert.ToDateTime(reader["timestamp"])
                            };
                        }else
                        {
                            bar = new ArbitrageCandle()
                            {
                                Close = Convert.ToDecimal(reader["Close"]),                              
                                High = Convert.ToDecimal(reader["High"]),
                                Low = Convert.ToDecimal(reader["Low"]),
                                Open = Convert.ToDecimal(reader["Open"]),                               
                                Datetime = Convert.ToDateTime(reader["timestamp"])
                            };
                        }
                        cs.Add(bar);
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            cs.Reverse();
            return cs;
        }
        public static void InsertKlineIntoTable(string inst, ArbitrageCandle candleStick)
        {

            string tableName = "kline_" + inst.Replace("-", "_").ToLower().Replace(" ", "").Replace(":", "_");
            string field = "Open,Close,High,Low,Volume,SOpen,SClose,SHigh,SLow,`timestamp`";
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO {table}({field}) ");
            sb.Append("VALUES");
            sb.Append(string.Format("({0},{1},{2},{3},{4},{5},{6},{7},{8},'{9}')", candleStick.Open, candleStick.Close, candleStick.High, candleStick.Low,
                0, candleStick.SOpen, candleStick.SClose, candleStick.SHigh, candleStick.SLow, candleStick.Datetime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")));
            string insertcmd = sb.ToString().Replace("{table}", tableName).Replace("{field}", field);
            using (MySqlConnection conn = new MySqlConnection(GetConnectionString("histpair")))
            {
                try
                {
                    conn.Open();                  
                    var insert = new MySqlCommand(insertcmd, conn);
                    int d = insert.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                { Trace.WriteLine(ex.Message); }
                Trace.WriteLine(insertcmd);
            }
        }
        static bool CreateTableIfNotExists(string dbName, string TableName, string inst)
        {
            StringBuilder createklinetable = new StringBuilder();
            createklinetable.Append(@"id BIGINT(20) NOT NULL AUTO_INCREMENT,
                     `Open` DECIMAL(20,8),
                     `Close` DECIMAL(20,8),
                     `High` DECIMAL(20,8),
                     `Low` DECIMAL(20,8),
                     `Volume` DECIMAL(20,4),
                     `SOpen` DECIMAL(20,8),
                     `SClose` DECIMAL(20,8),
                     `SHigh` DECIMAL(20,8),
                     `SLow` DECIMAL(20,8),
                     `timestamp` DATETIME,
                      PRIMARY KEY (id),
                      UNIQUE (`timestamp`)");

            StringBuilder stringbuilder = new StringBuilder();
            stringbuilder.Append("CREATE TABLE IF NOT EXISTS {table}");
            stringbuilder.Append("(");
            stringbuilder.Append("{field}");
            stringbuilder.Append(") ENGINE=myisam  DEFAULT CHARSET=utf8;");
            string sqlcmd = stringbuilder.ToString().Replace("{table}", TableName).Replace("{field}", createklinetable.ToString());
            int kk;
            using (MySqlConnection conn = new MySqlConnection(GetConnectionString(dbName)))
            {
                try
                {
                    conn.Open();
                    var createtablecmd = new MySqlCommand(sqlcmd, conn);                                                         
                    createtablecmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                { Trace.WriteLine(ex.Message);
                    return false;
                }
                //Trace.WriteLine(cmd);
                return true;
            }

        }
        static string CreateTableSQL(string dbName, string tableName, string field)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE IF NOT EXISTS {table}");
            sb.Append("(");
            sb.Append("{field}");
            sb.Append(") ENGINE=myisam  DEFAULT CHARSET=utf8;");
            string sqlcmd = sb.ToString().Replace("{table}", tableName).Replace("{field}", field);
            //  创建数据库表
            return sqlcmd;
        }
        static void InsertNewSymbolIntoDataBase(List<MajorExchangeSymbol> selected)
        {
            if (selected.Count == 0)
                return;
            string tableName = "exchangesymbol";

            string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
            string connectionString = string.Format(InitconnectionString, mysqlConfig.Ip, "setting", mysqlConfig.User, mysqlConfig.PassWord);
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (var v in selected)
                    {
                        string cmdStr = $"INSERT IGNORE INTO {tableName}  (exchangeid,symbol) VALUES (\"{v.ExchangeID}\",\"{v.Symbol}\");";
                        MySqlCommand candleCmd = new MySqlCommand(cmdStr, conn);
                        int i = candleCmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /*internal static string QueryTable(string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT {tableName} FROM {database} ");
            sb.Append("WHERE TABLE_SCHEMA=DATABASE()");
            sb.Append("AND");
            sb.Append("TABLE_NAME= {tableName}");
            string sqlcmd = sb.ToString().Replace("{database}", "hispair").Replace("{tableName}", tableName);
            return sqlcmd;
        }*/

        /*internal static void InsertKLineData(CandleStick v, string inst)
        {
            try
            {
                CreateKlineTable(inst);

                string tableName = "kline_" + inst.Replace("-", "_").ToLower().Replace(" ", "");

                string cmdStr = $"INSERT IGNORE INTO {tableName} (exchangeid,symbol) VALUES (\"Combo\",\"{inst}\");";

                MySqlCommand candleCmd = new MySqlCommand(cmdStr, conn);
                //int i = candleCmd.ExecuteNonQuery();

            }
            catch (Exception e)
            { }
        }*/
    }
    public static class MySQLHelper
    {
        //数据库连接字符串
        public static readonly string InitconnectionString = "Server={0};port=3306;database={1};UID={2};Password={3}";
        // 用于缓存参数的HASH表
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        private static string GetConnectionString(string dbName)
        {
            //获取Configuration对象
            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var mysqlConfig = Config.GetMysqlConfig();
            string UserId = mysqlConfig.User;
            string PassWord = mysqlConfig.PassWord;
            string Mysql_IP = mysqlConfig.Ip;
            return string.Format(InitconnectionString, Mysql_IP, dbName, UserId, PassWord);
        }

        public static int ExecuteNonQuery(string dbName, string cmdText, CommandType cmdType = CommandType.Text, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();

            string connectionString = GetConnectionString(dbName);
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            { return 0; }
            finally
            {
                conn.Close();
            }
        }
        /*public static async Task<int> ExecuteNonQueryAsync(string dbName, string cmdText, CommandType cmdType = CommandType.Text, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();

            string connectionString = GetConnectionString(dbName);
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                Task<int> val = cmd.ExecuteNonQueryAsync();
                await val;
                cmd.Parameters.Clear();
                return val.Result;
            }
            finally
            {
                conn.Close();
            }
        }*/
        public static MySqlDataReader ExecuteReader(string dbName, string cmdText, CommandType cmdType = CommandType.Text, params MySqlParameter[] commandParameters)
        {
            //创建一个MySqlCommand对象
            MySqlCommand cmd = new MySqlCommand();
            //创建一个MySqlConnection对象
            string connectionString = GetConnectionString(dbName);
            MySqlConnection conn = new MySqlConnection(connectionString);


            //在这里我们用一个try/catch结构执行sql文本命令/存储过程，因为如果这个方法产生一个异常我们要关闭连接，因为没有读取器存在，
            //因此commandBehaviour.CloseConnection 就不会执行
            try
            {
                //调用 PrepareCommand 方法，对 MySqlCommand 对象设置参数
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                //调用 MySqlCommand  的 ExecuteReader 方法
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                //清除参数
                cmd.Parameters.Clear();
                return reader;
            }
            catch
            {
                //关闭连接，抛出异常
                conn.Close();
                throw;
            }
        }


        public static DataSet GetDataSet(string dbName, string cmdText, CommandType cmdType = CommandType.Text, params MySqlParameter[] commandParameters)
        {
            //创建一个MySqlCommand对象
            MySqlCommand cmd = new MySqlCommand();
            //创建一个MySqlConnection对象
            string connectionString = GetConnectionString(dbName);
            MySqlConnection conn = new MySqlConnection(connectionString);

            //在这里我们用一个try/catch结构执行sql文本命令/存储过程，因为如果这个方法产生一个异常我们要关闭连接，因为没有读取器存在，
            try
            {
                //调用 PrepareCommand 方法，对 MySqlCommand 对象设置参数
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                //调用 MySqlCommand  的 ExecuteReader 方法
                MySqlDataAdapter adapter = new MySqlDataAdapter
                {
                    SelectCommand = cmd
                };
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                //清除参数
                cmd.Parameters.Clear();
                conn.Close();
                return ds;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static object ExecuteScalar(string dbName, string cmdText, CommandType cmdType = CommandType.Text, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();

            string connectionString = GetConnectionString(dbName);
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        public static void CacheParameters(string cacheKey, params MySqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        public static MySqlParameter[] GetCachedParameters(string cacheKey)
        {
            MySqlParameter[] cachedParms = (MySqlParameter[])parmCache[cacheKey];


            if (cachedParms == null)
                return null;


            MySqlParameter[] clonedParms = new MySqlParameter[cachedParms.Length];


            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (MySqlParameter)((ICloneable)cachedParms[i]).Clone();


            return clonedParms;
        }

        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, CommandType cmdType, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;


            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;


            if (cmdParms != null && cmdParms.Length > 0)
            {
                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
    }
    public enum TableType
    {
        ticker,
        depth,
        kline,
        metadata
    }
}
