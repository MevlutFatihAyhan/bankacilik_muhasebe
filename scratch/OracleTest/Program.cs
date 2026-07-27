using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace OracleTest
{
    // DB/*.sql betiklerini Oracle'a yükleyen yardımcı araç.
    // Kullanım (scratch/OracleTest dizininden):
    //     dotnet run -- ../../DB/03_Procedures.sql [baska.sql ...]
    // Dosya verilmezse varsayılan olarak DB/03_Procedures.sql yüklenir.
    // Bağlantı dizesi Backend/BankAPI/appsettings.json içindeki
    // ConnectionStrings:OracleConnection değerinden okunur.
    class Program
    {
        static int Main(string[] args)
        {
            string repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

            string connectionString;
            try
            {
                connectionString = ConnectionStringOku(repoRoot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bağlantı dizesi okunamadı: {ex.Message}");
                return 1;
            }

            List<string> sqlFiles = new List<string>();
            foreach (string arg in args)
            {
                sqlFiles.Add(Path.GetFullPath(arg));
            }

            if (sqlFiles.Count == 0)
            {
                sqlFiles.Add(Path.Combine(repoRoot, "DB", "03_Procedures.sql"));
            }

            int hataSayisi = 0;

            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    Console.WriteLine("Oracle veritabanına bağlanılıyor...");
                    conn.Open();
                    Console.WriteLine($"Bağlantı başarılı. Şema: {conn.DatabaseName}\n");

                    foreach (string sqlFilePath in sqlFiles)
                    {
                        Console.WriteLine($"--- {sqlFilePath} ---");
                        if (!File.Exists(sqlFilePath))
                        {
                            Console.WriteLine("❌ Dosya bulunamadı.\n");
                            hataSayisi++;
                            continue;
                        }

                        string sqlText = File.ReadAllText(sqlFilePath);
                        // Bloklar SQL*Plus'taki gibi tek başına '/' satırlarıyla ayrılır.
                        string[] blocks = Regex.Split(sqlText, @"^\s*/\s*$", RegexOptions.Multiline);

                        int blokNo = 0;
                        foreach (var block in blocks)
                        {
                            // ODP.NET SQL*Plus komutlarını (SET SERVEROUTPUT, SPOOL, PROMPT...)
                            // anlamaz; bunlar atılır. Blok sonundaki ';' PL/SQL için gereklidir,
                            // bu yüzden kırpılmaz.
                            string sql = SqlPlusKomutlariniTemizle(block).Trim();
                            if (string.IsNullOrWhiteSpace(sql)) continue;

                            blokNo++;
                            string basligi = Ozet(sql);

                            try
                            {
                                using (var cmd = new OracleCommand(sql, conn))
                                {
                                    cmd.ExecuteNonQuery();
                                    Console.WriteLine($"✓ Blok {blokNo}: {basligi}");
                                }
                            }
                            catch (Exception ex)
                            {
                                hataSayisi++;
                                Console.WriteLine($"❌ Blok {blokNo}: {basligi}\n   {ex.Message}");
                            }
                        }
                        Console.WriteLine();
                    }

                    hataSayisi += GecersizNesneleriYaz(conn);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bağlantı hatası: {ex.Message}");
                return 1;
            }

            Console.WriteLine(hataSayisi == 0
                ? "Tüm bloklar sorunsuz çalıştı ve geçersiz nesne yok."
                : $"{hataSayisi} sorun raporlandı.");

            return hataSayisi == 0 ? 0 : 1;
        }

        private static string ConnectionStringOku(string repoRoot)
        {
            string appSettingsPath = Path.Combine(repoRoot, "Backend", "BankAPI", "appsettings.json");
            using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(appSettingsPath)))
            {
                return doc.RootElement
                          .GetProperty("ConnectionStrings")
                          .GetProperty("OracleConnection")
                          .GetString();
            }
        }

        // Derlenirken hata alan (INVALID) paket/prosedür/trigger'ları listeler.
        private static int GecersizNesneleriYaz(OracleConnection conn)
        {
            int satirSayisi = 0;
            const string sql = @"SELECT NAME, TYPE, LINE, POSITION, TEXT
                                   FROM USER_ERRORS
                                  ORDER BY NAME, SEQUENCE";

            using (var cmd = new OracleCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (satirSayisi == 0)
                    {
                        Console.WriteLine("--- Derleme hataları (USER_ERRORS) ---");
                    }
                    satirSayisi++;
                    Console.WriteLine($"❌ {reader["TYPE"]} {reader["NAME"]} " +
                                      $"({reader["LINE"]}:{reader["POSITION"]}): {reader["TEXT"]?.ToString()?.Trim()}");
                }
            }

            if (satirSayisi == 0)
            {
                Console.WriteLine("--- Derleme hatası yok (USER_ERRORS boş) ---");
            }

            return satirSayisi;
        }

        // SQL*Plus komutları yalnızca bloğun başındaki yorum/boşluk bölgesinde aranır.
        // Aksi halde "UPDATE ... SET ..." gibi gerçek SQL satırları da silinirdi.
        private static string SqlPlusKomutlariniTemizle(string block)
        {
            string[] satirlar = block.Split('\n');
            var temiz = new List<string>(satirlar.Length);
            bool baslangicBolgesi = true;

            foreach (string satir in satirlar)
            {
                string kirpik = satir.Trim();

                if (baslangicBolgesi)
                {
                    if (kirpik.Length == 0 || kirpik.StartsWith("--"))
                    {
                        temiz.Add(satir);
                        continue;
                    }

                    if (Regex.IsMatch(kirpik, @"^(SET|SPOOL|PROMPT|EXIT|QUIT|WHENEVER|SHOW|@@?)\b",
                                      RegexOptions.IgnoreCase))
                    {
                        continue; // SQL*Plus komutu — ODP.NET çalıştıramaz, atlanır
                    }

                    baslangicBolgesi = false;
                }

                temiz.Add(satir);
            }

            return string.Join("\n", temiz);
        }

        private static string Ozet(string sql)
        {
            string tekSatir = Regex.Replace(sql, @"\s+", " ").Trim();
            return tekSatir.Length > 70 ? tekSatir.Substring(0, 70) + "..." : tekSatir;
        }
    }
}
