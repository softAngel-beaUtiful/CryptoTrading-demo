using SimpleLogger.Logging.Module.Database;
using System.Data;
using System.Data.Common;

namespace SimpleLogger.Logging.Module
{
    public class DatabaseLoggerModule : LoggerModule
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly DatabaseType _databaseType;

        public DatabaseLoggerModule(DatabaseType databaseType, string connectionString) 
            : this(databaseType, connectionString, "TradeLog") { }

        public DatabaseLoggerModule(DatabaseType databaseType, string connectionString, string tableName)
        {
            _databaseType = databaseType;
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public override void Initialize()
        {
            CreateTable();
        }

        public override string Name
        {
            get { return "MySqlModule"; }
        }

        private DbParameter GetParameter(DbCommand command, string name, object value, DbType type)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = type;
            parameter.ParameterName = (_databaseType.Equals(DatabaseType.Oracle) ? ":" : "@") + name;
            parameter.Value = value;
            return parameter;
        }

        private void AddParameter(DbCommand command, string name, object value, DbType type)
        {
            command.Parameters.Add(GetParameter(command, name, value, type));
        }

        public override void AfterLog(LogMessage logMessage)
        {
            using (var connection = DatabaseFactory.GetConnection(_databaseType, _connectionString))
            {
                connection.Open();
                var commandText = DatabaseFactory.GetInsertCommand(_databaseType, _tableName);
                var sqlCommand = DatabaseFactory.GetCommand(_databaseType, commandText, connection);
                AddParameter(sqlCommand, "Datatype", logMessage.DataCategory, DbType.String);
                AddParameter(sqlCommand, "Data", logMessage.Data, DbType.String);
                AddParameter(sqlCommand, "DateTime", logMessage.DateTime, DbType.Date);               
                AddParameter(sqlCommand, "CallingClass", logMessage.CallingClass, DbType.String);
                AddParameter(sqlCommand, "CallingMethod", logMessage.CallingMethod, DbType.String);
                AddParameter(sqlCommand, "UserID", logMessage.UserID, DbType.String);
                AddParameter(sqlCommand, "StrategyID", logMessage.StrategyID, DbType.String);
                AddParameter(sqlCommand, "BackTestID", logMessage.BackTestID, DbType.String);

                sqlCommand.ExecuteNonQuery();
            }
        }

        private void CreateTable()
        {
            var connection = DatabaseFactory.GetConnection(_databaseType, _connectionString);

            using (connection)
            {
                connection.Open();
                var commandTe = string.Format(DatabaseFactory.GetCreateTableQuery(_databaseType), _tableName);
                var command = DatabaseFactory.GetCommand(_databaseType, commandTe, connection);
                command.ExecuteNonQuery();  

                var sqlCommand = DatabaseFactory.GetCommand
                (
                    _databaseType,
                    DatabaseFactory.GetCheckIfShouldCreateTableQuery(_databaseType), 
                    connection
                );
                AddParameter(sqlCommand, "TableName", _tableName.ToLower(), DbType.String);
                
                var result = sqlCommand.ExecuteScalar();

                if (result == null)
                {
                    var commandText = string.Format(DatabaseFactory.GetCreateTableQuery(_databaseType), _tableName);
                    sqlCommand = DatabaseFactory.GetCommand(_databaseType, commandText, connection);
                    sqlCommand.ExecuteMultipleNonQuery();
                }
            }
        }
    }
}
