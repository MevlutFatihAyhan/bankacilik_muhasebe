using System.Collections.Generic;

namespace BankAPI.Models
{
    public class DashboardSummary
    {
        public MusteriIstatistik MusteriIstatistikleri { get; set; } = new MusteriIstatistik();
        public List<HesapIstatistik> HesapIstatistikleri { get; set; } = new List<HesapIstatistik>();
        public List<HacimIstatistik> HacimIstatistikleri { get; set; } = new List<HacimIstatistik>();
        public List<SonIslem> SonIslemler { get; set; } = new List<SonIslem>();
    }

    public class MusteriIstatistik
    {
        public int AktifMusteriSayisi { get; set; }
        public int BireyselSayisi { get; set; }
        public int TuzelSayisi { get; set; }
        public int ToplamMusteri { get; set; }
    }

    public class HesapIstatistik
    {
        public string DovizCinsi { get; set; }
        public decimal ToplamBakiye { get; set; }
        public int HesapSayisi { get; set; }
        public int VadesizSayisi { get; set; }
        public int VadeliSayisi { get; set; }
    }

    public class HacimIstatistik
    {
        public string DovizCinsi { get; set; }
        public string IslemYonu { get; set; }
        public decimal ToplamHacim { get; set; }
        public int IslemSayisi { get; set; }
    }

    public class SonIslem
    {
        public string HesapNo { get; set; }
        public string IslemYonu { get; set; }
        public decimal IslemTutari { get; set; }
        public string DovizCinsi { get; set; }
        public string Aciklama { get; set; }
        public System.DateTime IslemTarihi { get; set; }
    }
}
