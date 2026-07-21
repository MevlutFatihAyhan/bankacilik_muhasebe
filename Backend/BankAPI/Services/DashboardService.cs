using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using BankAPI.Models;

namespace BankAPI.Services
{
    public class DashboardService
    {
        private readonly string _connectionString;

        public DashboardService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }

        public DashboardSummary GetSummary()
        {
            DashboardSummary summary = new DashboardSummary();

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand("PKG_DASHBOARD.PRC_GET_SUMMARY", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;

                    // Output parameters (Cursors)
                    OracleParameter pMusteriStats = new OracleParameter("P_MUSTERI_STATS", OracleDbType.RefCursor);
                    pMusteriStats.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pMusteriStats);

                    OracleParameter pHesapStats = new OracleParameter("P_HESAP_STATS", OracleDbType.RefCursor);
                    pHesapStats.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pHesapStats);

                    OracleParameter pHacimStats = new OracleParameter("P_HACIM_STATS", OracleDbType.RefCursor);
                    pHacimStats.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pHacimStats);

                    OracleParameter pSonIslemler = new OracleParameter("P_SON_ISLEMLER", OracleDbType.RefCursor);
                    pSonIslemler.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pSonIslemler);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        // 1. Cursor: Müşteri İstatistikleri
                        if (reader.Read())
                        {
                            summary.MusteriIstatistikleri.AktifMusteriSayisi = reader["AKTIF_MUSTERI_SAYISI"] != DBNull.Value ? Convert.ToInt32(reader["AKTIF_MUSTERI_SAYISI"]) : 0;
                            summary.MusteriIstatistikleri.BireyselSayisi = reader["BIREYSEL_SAYISI"] != DBNull.Value ? Convert.ToInt32(reader["BIREYSEL_SAYISI"]) : 0;
                            summary.MusteriIstatistikleri.TuzelSayisi = reader["TUZEL_SAYISI"] != DBNull.Value ? Convert.ToInt32(reader["TUZEL_SAYISI"]) : 0;
                            summary.MusteriIstatistikleri.ToplamMusteri = reader["TOPLAM_MUSTERI"] != DBNull.Value ? Convert.ToInt32(reader["TOPLAM_MUSTERI"]) : 0;
                        }

                        // 2. Cursor: Hesap İstatistikleri
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                HesapIstatistik stat = new HesapIstatistik
                                {
                                    DovizCinsi = reader["DOVIZ_CINSI"]?.ToString(),
                                    ToplamBakiye = reader["TOPLAM_BAKIYE"] != DBNull.Value ? Convert.ToDecimal(reader["TOPLAM_BAKIYE"]) : 0,
                                    HesapSayisi = reader["HESAP_SAYISI"] != DBNull.Value ? Convert.ToInt32(reader["HESAP_SAYISI"]) : 0,
                                    VadesizSayisi = reader["VADESIZ_SAYISI"] != DBNull.Value ? Convert.ToInt32(reader["VADESIZ_SAYISI"]) : 0,
                                    VadeliSayisi = reader["VADELI_SAYISI"] != DBNull.Value ? Convert.ToInt32(reader["VADELI_SAYISI"]) : 0
                                };
                                summary.HesapIstatistikleri.Add(stat);
                            }
                        }

                        // 3. Cursor: Hacim İstatistikleri
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                HacimIstatistik stat = new HacimIstatistik
                                {
                                    DovizCinsi = reader["DOVIZ_CINSI"]?.ToString(),
                                    IslemYonu = reader["ISLEM_YONU"]?.ToString(),
                                    ToplamHacim = reader["TOPLAM_HACIM"] != DBNull.Value ? Convert.ToDecimal(reader["TOPLAM_HACIM"]) : 0,
                                    IslemSayisi = reader["ISLEM_SAYISI"] != DBNull.Value ? Convert.ToInt32(reader["ISLEM_SAYISI"]) : 0
                                };
                                summary.HacimIstatistikleri.Add(stat);
                            }
                        }

                        // 4. Cursor: Son İşlemler
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                SonIslem islem = new SonIslem
                                {
                                    IslemId = reader["ISLEM_ID"] != DBNull.Value ? Convert.ToDecimal(reader["ISLEM_ID"]) : 0,
                                    HesapNo = reader["HESAP_NO"]?.ToString(),
                                    IslemYonu = reader["ISLEM_YONU"]?.ToString()?.Trim(),
                                    IslemTutari = reader["ISLEM_TUTARI"] != DBNull.Value ? Convert.ToDecimal(reader["ISLEM_TUTARI"]) : 0,
                                    DovizCinsi = reader["DOVIZ_CINSI"]?.ToString()?.Trim(),
                                    YeniBakiye = reader["YENI_BAKIYE"] != DBNull.Value ? Convert.ToDecimal(reader["YENI_BAKIYE"]) : 0,
                                    Aciklama = reader["ACIKLAMA"]?.ToString(),
                                    IslemTarihi = Convert.ToDateTime(reader["ISLEM_TARIHI"])
                                };
                                summary.SonIslemler.Add(islem);
                            }
                        }
                    }
                }
            }

            return summary;
        }
    }
}
