using UnityEngine;

namespace HikayeStudyo
{
    // Asama 13: gozyasi ve nefes buhari icin yuz noktalari.
    public partial class Sahne3D
    {
        public bool YuzNoktalari(string id, out Vector3 solGoz, out Vector3 sagGoz, out Vector3 agiz, out Vector3 ileri, out Vector3 yukari, out float boy)
        {
            solGoz = sagGoz = agiz = Vector3.zero;
            ileri = Vector3.back; yukari = Vector3.up; boy = 1f;
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null) return false;
            var kafa = Kemik(a, HumanBodyBones.Head);
            if (kafa == null) return false;
            var t = a.transform;
            ileri = t.forward; yukari = t.up; boy = t.lossyScale.y;
            var sol = Kemik(a, HumanBodyBones.LeftEye);
            var sag = Kemik(a, HumanBodyBones.RightEye);
            solGoz = sol != null ? sol.position : kafa.position + yukari * 0.06f * boy - t.right * 0.032f * boy;
            sagGoz = sag != null ? sag.position : kafa.position + yukari * 0.06f * boy + t.right * 0.032f * boy;
            // goz kemigi kafanin icindedir; yuz yuzeyine tasinir
            solGoz += ileri * 0.045f * boy;
            sagGoz += ileri * 0.045f * boy;
            agiz = (solGoz + sagGoz) * 0.5f - yukari * 0.065f * boy;
            return true;
        }

        // 3D dunya noktasinin 2D kanvastaki dunya konumu; kadraj disindaysa false
        public bool EkranNoktasi(Vector3 dunya, out Vector3 kanvas)
        {
            kanvas = Vector3.zero;
            if (Cam == null || image == null) return false;
            var v = Cam.WorldToViewportPoint(dunya);
            if (v.z <= 0f || v.x < -0.05f || v.x > 1.05f || v.y < -0.05f || v.y > 1.05f) return false;
            var r = image.rectTransform;
            kanvas = r.TransformPoint(new Vector2((v.x - 0.5f) * r.rect.width, (v.y - 0.5f) * r.rect.height));
            return true;
        }
    }
}
