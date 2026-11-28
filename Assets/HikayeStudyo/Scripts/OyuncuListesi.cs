using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // 3D oyuncu prefablarının listesi. Derlenen EXE'de sahnedeki StoryPlayer.oyuncular listesi olmadığı için
    // "Hikaye > Windows Derlemesi" bu varlığı Assets/Resources/OyuncuListesi.asset olarak yeniler;
    // KayitBootstrap onu okuyup StoryPlayer'a verir.
    public class OyuncuListesi : ScriptableObject
    {
        public List<GameObject> prefablar = new List<GameObject>();
    }
}
