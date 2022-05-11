using System;
using System.Reflection;
using Newtonsoft.Json;
using SimpleLogger.Logging.Handlers;
using SimpleLogger.Logging.Module;
using SimpleLogger.Logging.Module.Database;

namespace SimpleLogger.Sample
{
    class MyClass
    {
        public string ID;
        public string Password;
    }
    class MyException
    {
        public MyException()
        {
            Logger.LoggerHandlerManager
                .AddHandler(new ConsoleLoggerHandler())
                .AddHandler(new FileLoggerHandler())
                .AddHandler(new DebugConsoleLoggerHandler());
            // Installing Module to save info through Module for example, Redis, MySql
            Logger.Modules.Install(new RedisLoggerModule(new RedisConfiguration("MajorShi", "112233444", "52.77.238.82", 6379, "518052")));
            string connectionstring = string.Format("Server=52.77.238.82;port=3306;database={0}; UID=Major; password=518052", "log");
            Logger.Modules.Install(new DatabaseLoggerModule(DatabaseType.MySql, connectionstring ));
            try
            {
                // Simulation of exceptions
                //throw new System.Exception();
            }
            catch (System.Exception exception)
            {
                // Logging exceptions
                // Automatical adjustment of specific log into the Error and adding of StackTrace
                //Logger.Log(exception);
                Logger.Log(exception, MethodBase.GetCurrentMethod().Name, this.GetType().Name );
            }
            var logmessage = new LogMessage();
            logmessage.CallingClass = this.GetType().Name;
            logmessage.CallingMethod = MethodBase.GetCurrentMethod().Name;
            logmessage.DataType = typeof(MyClass).Name;
            var my = new MyClass() { ID = "asgdasg", Password = "asdgas" };
            logmessage.Data = JsonConvert.SerializeObject(my);
            logmessage.DateTime = DateTime.UtcNow;
            Logger.Log(logmessage);
            Logger.Log(logmessage, MethodBase.GetCurrentMethod().Name, this.GetType().Name);
            Logger.Log(new MyClass() { ID="Major", Password="mAJORSHI"}, MethodBase.GetCurrentMethod().Name, this.GetType().Name);
            Console.ReadKey();
        }
        public static void Main()
        {
            var yy = new MyException();

            // Adding handler - to show log messages (ILoggerHandler)
            
        }

        private static void MySqlDatabaseLoggerModuleSample()
        {
            // Just add the module and it works! 
            Logger.Modules.Install(new DatabaseLoggerModule(DatabaseType.MySql, "Your connection string here!"));
            //Logger.Log("My first database log! ");
        }
              
        public static void RedisModuleSample()
        {
            //var redisconnection = new Redis
        }
        public static void EmailModuleSample()
        {
            // Configuring smtp server
            var smtpServerConfiguration = new SmtpServerConfiguration("userName", "password", "smtp.gmail.com", 587);

            // Creating a module
            var emailSenderLoggerModule = new EmailSenderLoggerModule(smtpServerConfiguration)
            {
                EnableSsl = true,
                Sender = "sender-email@gmail.com"
            };

            // Add the module and it works
            emailSenderLoggerModule.Recipients.Add("recipients@gmail.com");

            //  // Simulation of exceptions
            Logger.Modules.Install(emailSenderLoggerModule);

            try
            {
                // Simulation of exceptions
                throw new NullReferenceException();
            }
            catch (System.Exception)
            {
                // Log exception
                // If you catch an exception error -> will be sent an email with a list of log message.
                //Logger.Log(exception);
            }
        }
    }
}
