using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace TradingPlatform
{
    class Logger
    {
        private readonly string info = "INFO";
        private readonly string warning = "WARN";
        private readonly string error = "ERROR";
        private readonly string connectStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public void LogInfo(string msg)
        {
            using(var connection = new SqlConnection(connectStr))
            {
                connection.Open();
                var cmd = new SqlCommand("INSERT INTO ProgramLog (severity, message, time_happened) " +
                    $"VALUES (@lv, @msg, @dt)", connection);
                cmd.Parameters.AddWithValue("@lv", info);
                cmd.Parameters.AddWithValue("@msg", msg);
                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
        public void LogError(Exception ex)
        {
            using (var connection = new SqlConnection(connectStr))
            {
                connection.Open();
                var cmd = new SqlCommand("INSERT INTO ProgramLog (severity, message, time_happened) " +
                    $"VALUES (@lv, @msg, @dt)", connection);
                cmd.Parameters.AddWithValue("@lv", error);
                cmd.Parameters.AddWithValue("@msg", ex.Message);
                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
        public void LogWarning(string msg)
        {
            using (var connection = new SqlConnection(connectStr))
            {
                connection.Open();
                var cmd = new SqlCommand("INSERT INTO ProgramLog (severity, message, time_happened) " +
                    $"VALUES (@lv, @msg, @dt)", connection);
                cmd.Parameters.AddWithValue("@lv", warning);
                cmd.Parameters.AddWithValue("@msg", msg);
                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void LogDealHappened(string sellerUsername, string receiverUsername, string productName, decimal productPrice)
        {
            using (var connection = new SqlConnection(connectStr))
            {
                connection.Open();
                var cmd = new SqlCommand("INSERT INTO DealLog (seller_username, receiver_username, product_name, product_price, time_happened) " +
                    $"VALUES (@su, @ru, @pn, @pp, @th)", connection);
                cmd.Parameters.AddWithValue("@su", sellerUsername);
                cmd.Parameters.AddWithValue("@ru", receiverUsername);
                cmd.Parameters.AddWithValue("@pn", productName);
                cmd.Parameters.AddWithValue("@pp", productPrice);
                cmd.Parameters.AddWithValue("@th", DateTime.Now);

                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }

    class LogManager
    {
        private static Logger LoggerInstance { get; set; } = null;
        public static Logger getInstance()
        {
            if (LoggerInstance == null)
            {
                LoggerInstance = new Logger();
            }
            return LoggerInstance;
        }
    }
}
