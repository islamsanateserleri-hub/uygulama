using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Bağımsız uygulama ile Editör aracı aynı çekirdeği kullansın diye varlık ve kayıt erişimi arayüzlerin arkasında.
    // Çekirdek / uygulama kodu Editör sınıflarını ASLA doğrudan çağırmaz.
    public interface IVarlikKatalogu
    {
        IReadOnlyList<string> KarakterKimlikleri { get; }
        GameObject KarakterPrefab(string kimlik);
    }

    public interface IVideoKaydedici
    {
        bool Kaydediyor { get; }
        // genislik/yukseklik/fps: dikey TikTok için 1080x1920, 30
        bool Baslat(string cikisYolu, int genislik, int yukseklik, int fps, out string hata);
        void Durdur();
    }
}
