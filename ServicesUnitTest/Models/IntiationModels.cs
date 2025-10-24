using NSCore.DatabaseProviders;

namespace ServicesUnitTest.Models
{
    public static class IntiationModels
    {
        // public static LogStartUp _log = new LogStartUp
        // {
        //     LogFolderName = "logs",
        //     NeedSeparateFailureLog = true,
        //     MaxLogSize = 10,
        //     ExpireDays = 10
        // };

        // public static LogOptions _log = new LogOptions
        // {
        //     DateFormat = "yyyy-MM-dd",
        //     FlowDirection = LogFlow.TopToBottom,
        //     EnableColors = true,
        //     ArchivePath = "./log/archive",
        //     ArchiveThreshold = TimeSpan.FromDays(1),
        // };

        // public static LogEntry _log = new LogEntry
        // {
        //     Message = "Log message",
        //     Level = LogHandler.Core.LogLevel.Info,
        //     TimeStamp = DateTime.UtcNow,
        // };


        // PostgreSQL database instances
        public static PSQLDb _psqlModel = new PSQLDb
        {
            DatabaseName = "ticket_manager_pg",
            ServerName = "localhost",
            Port = "5432",
            UserName = "postgres",
            Password = "jagoro"
        };

        // SqlServer database instances
        // public static SQLDb _sqlModel = new SQLDb
        // {
        //     DatabaseName = "demo",
        //     ServerName = "localhost",
        // };

        // MySQL database instances
        public static MySQLDb _mySqlModel = new MySQLDb
        {
            DatabaseName = "demo",
            ServerName = "localhost",
            Port = "3306",
            UserName = "root",
            Password = "onyildev@sf254"
        };
    }
}
