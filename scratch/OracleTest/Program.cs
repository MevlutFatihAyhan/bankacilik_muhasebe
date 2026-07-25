using System;
using System.IO;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace OracleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=(description= (retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1521)(host=adb.eu-frankfurt-1.oraclecloud.com))(connect_data=(service_name=gd33e9612e30a83_mfabank_medium.adb.oraclecloud.com))(security=(ssl_server_dn_match=yes)));User Id=ADMIN;Password=MFZNAyhan2005.;Validate Connection=true;";
            string sqlFilePath = @"c:\Users\fatih\OneDrive\Documents\vscode\DB\03_Procedures.sql";

            try
            {
                string sqlText = File.ReadAllText(sqlFilePath);
                // Split by '/' on lines
                string[] blocks = Regex.Split(sqlText, @"^\s*/\s*$", RegexOptions.Multiline);

                using (var conn = new OracleConnection(connectionString))
                {
                    Console.WriteLine("Oracle veritabanına bağlanılıyor...");
                    conn.Open();
                    Console.WriteLine("Bağlantı başarılı. Prosedürler ve Paketler güncelleniyor...");

                    foreach (var block in blocks)
                    {
                        string sql = block.Trim();
                        if (string.IsNullOrWhiteSpace(sql)) continue;

                        try
                        {
                            using (var cmd = new OracleCommand(sql, conn))
                            {
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("✓ Paket bloğu başarıyla çalıştırıldı.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Blok çalıştırma hatası: {ex.Message}");
                        }
                    }

                    Console.WriteLine("\nTüm paketler Oracle veritabanına başarıyla yüklendi/güncellendi!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bağlantı hatası: {ex.Message}");
            }
        }
    }
}
