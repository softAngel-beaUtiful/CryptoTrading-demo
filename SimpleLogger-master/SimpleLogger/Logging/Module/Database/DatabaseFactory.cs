using MySql.Data.MySqlClient;
using System.Data.Common;

namespace SimpleLogger.Logging.Module.Database
{
    internal static class DatabaseFactory
    {
        internal static DbConnection GetConnection(DatabaseType databaseType, string connectionString)
        {            
            switch (databaseType)
            {                
                case DatabaseType.MySql:                   
                return new MySqlConnection(connectionString);                             
            }
            return null;
        }

        internal static DbCommand GetCommand(DatabaseType databaseType, string commandText, DbConnection connection)
        {
            switch (databaseType)
            {
                case DatabaseType.MySql:
                    return new MySqlCommand(commandText, connection as MySqlConnection);               
            }
            return null;
        }

        internal static string GetDatabaseName(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.MsSql:
                    return "MsSqlDatabaseLoggerModule";
                case DatabaseType.Oracle:
                    return "OracleDatabaseLoggerModule";
                case DatabaseType.MySql:
                    return "MySqlDatabaseLoggerModule";
            }
            return string.Empty;
        }

        internal static string GetCreateTableQuery(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.MsSql:
                    return @"create table [{0}]
                            (
	                            [Id] int not null primary key identity, 
                                [Text] nvarchar(4000) null, 
                                [DateTime] datetime null, 
                                [Log_Level] nvarchar(10) null, 
                                [CallingClass] nvarchar(500) NULL, 
                                [CallingMethod] nvarchar(500) NULL
                            );";
                case DatabaseType.Oracle:
                    return @"create table {0}
                                (
                                 Id int not null primary key, 
                                   Text varchar2(4000) null, 
                                   DateTime date null, 
                                   Log_Level varchar2(10) null, 
                                   CallingClass varchar2(500) NULL, 
                                   CallingMethod varchar2(500) NULL
                                );
                                create sequence seq_log nocache;";
                case DatabaseType.MySql:
                    return @"create table if not exists {0}
                            (	      
                                Id int not null auto_increment,
                                UserID varchar(32),
                                StrategyID varchar(64),
                                BackTestID varchar(32),
                                DataType varchar(30),
                                Data varchar(4000) null, 
                                DateTime datetime null,                                
                                CallingClass varchar(500) NULL, 
                                CallingMethod varchar(500) NULL,
                                PRIMARY KEY (Id),
                                INDEX idx_us(UserID, StrategyID, DataType)
                            );";
            }
            return string.Empty;
        }

        internal static string GetCheckIfShouldCreateTableQuery(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.MsSql:
                    return @"SELECT object_name(object_id) as table_name 
                               FROM sys.objects
                              WHERE type_desc LIKE '%USER_TABLE' 
                                AND lower(object_name(object_id)) like @tableName;";
                case DatabaseType.Oracle:
                    return @"SELECT TABLE_NAME 
                               FROM ALL_TABLES 
                              WHERE LOWER(TABLE_NAME) LIKE :tableName";
                case DatabaseType.MySql:
                    return @"SELECT table_name
                               FROM information_schema.tables
                              WHERE LOWER(table_name) = @tableName;";
            }

            return string.Empty;
        }

        internal static string GetInsertCommand(DatabaseType databaseType, string tableName)
        {
            switch (databaseType)
            {
                case DatabaseType.MsSql:
                    return string.Format(@"insert into {0} ([Data], [DateTime], [CallingClass], [CallingMethod]) 
                                           values (@text, @dateTime, @callingClass, @callingMethod);", tableName);
                case DatabaseType.Oracle:
                    return string.Format(@"insert into {0} (Id, Data, DateTime, CallingClass, CallingMethod) 
                                           values (seq_log.nextval, :text, :dateTime,  :callingClass, :callingMethod)", tableName);
                case DatabaseType.MySql:
                    return string.Format(@"insert into {0} (DataType, Data, DateTime, CallingClass, CallingMethod, UserID, StrategyID, BackTestID) 
                                           values (@Datatype, @Data, @DateTime, @CallingClass, @CallingMethod, @UserID, @StrategyID, @BackTestID);", tableName);
            }

            return string.Empty;
        }
    }
}
