using UnityEngine;

namespace HikayeStudyo
{
    // Oturma klipleri dizleri acik tutuyor; etekli karakterde kamera etegin altini gosteriyordu.
    // Oturma klibi oynarken her karede dizler ve ayaklar birbirine yaklastirilir.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(550)]   // YuzCanlilik'ten (500) sonra, KolPozu'dan (600) once
    public class DizKapat : MonoBehaviour
    {
        const float DizArasi = 0.045f;     // orta cizgiden her dizin uzakligi (boy 1 icin)

        Karakter3D karakter;
        Transform solUst, solAlt, solAyak, sagUst, sagAlt, sagAyak;

        void Start()
        {
            karakter = GetComponent<Karakter3D>();
            var an = KolPozu.InsanAnimator(gameObject);
            if (an == null) { enabled = false; return; }
            solUst = an.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            solAlt = an.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            solAyak = an.GetBoneTransform(HumanBodyBones.LeftFoot);
            sagUst = an.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            sagAlt = an.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            sagAyak = an.GetBoneTransform(HumanBodyBones.RightFoot);
            if (solUst == null || solAlt == null || solAyak == null || sagUst == null || sagAlt == null || sagAyak == null) enabled = false;
        }

        static bool Oturuyor(string klip)
        {
            if (string.IsNullOrEmpty(klip)) return false;
            var k = klip.ToLowerInvariant();
            return k.Contains("sitting") || k.Contains("driving");
        }

        void LateUpdate()
        {
            if (karakter == null || !Oturuyor(karakter.CurrentClip)) return;
            Vector3 orta = (solUst.position + sagUst.position) * 0.5f;
            Vector3 yan = sagUst.position - solUst.position;
            yan.y = 0f;
            if (yan.sqrMagnitude < 0.0001f) return;
            yan.Normalize();
            float ara = DizArasi * transform.lossyScale.y;
            Bacak(solUst, solAlt, solAyak, orta, yan, -ara);
            Bacak(sagUst, sagAlt, sagAyak, orta, yan, ara);
        }

        static void Bacak(Transform ust, Transform alt, Transform ayak, Vector3 orta, Vector3 yan, float hedef)
        {
            Yaklastir(ust, alt, orta, yan, hedef);      // uyluk: diz ortaya
            Yaklastir(alt, ayak, orta, yan, hedef);     // baldir: ayak ortaya
        }

        // 'kemik'i, 'uc' orta duzlemden 'hedef' uzakliga gelecek sekilde dondurur
        static void Yaklastir(Transform kemik, Transform uc, Vector3 orta, Vector3 yan, float hedef)
        {
            Vector3 simdi = uc.position - kemik.position;
            float fark = hedef - Vector3.Dot(uc.position - orta, yan);
            Vector3 istenen = simdi + yan * fark;
            if (istenen.sqrMagnitude < 0.0001f) return;
            kemik.rotation = Quaternion.FromToRotation(simdi, istenen) * kemik.rotation;
        }
    }
}
