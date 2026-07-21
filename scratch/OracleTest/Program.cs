using System;
using Oracle.ManagedDataAccess.Client;

namespace OracleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=(description= (retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1521)(host=adb.eu-frankfurt-1.oraclecloud.com))(connect_data=(service_name=gd33e9612e30a83_mfabank_medium.adb.oraclecloud.com))(security=(ssl_server_dn_match=yes)));User Id=ADMIN;Password=MFZNAyhan2005.;Validate Connection=true;";
            
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    Console.WriteLine("Bağlanıyor...");
                    conn.Open();
                    Console.WriteLine("Connected to Oracle database.");
                    using (var cmd = new OracleCommand("SELECT * FROM dual", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string result = reader.GetString(0);
                                Console.WriteLine($"Query Result: {result}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to Oracle database: {ex.Message}");
            }
        }
    }
}
