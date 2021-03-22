using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement.Kdf;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Crypto
{
    public abstract class AssertionCrypto
    {
        public class RequiredParametersBase
        {
            // Elliptic curve OID
            public string NamedCurveOid;
            // Parametr KDF - algorithmID
            public string AlgorithmID;
            // Parametr KDF - partyUInfo, identyfikator nadawcy
            public string PartyUInfo;
            // Parametr KDF - partyVInfo, identyfikator odbiorcy
            public string PartyVInfo;
            // Algorytm uzyty do zaszyfrowania klucza
            public string KeyEncryptionMethod;
            // Identyfikator funkcji skrotu w operacji KDF
            public string DigestMethodString;
        }

        public ECDomainParameters GetCurveParameters(string oid)
        {
            X9ECParameters ecP = ECNamedCurveTable.GetByOid(new DerObjectIdentifier(oid));

            if (ecP == null)
                throw new Exception("Unknown curve oid: " + oid);

            return new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H, ecP.GetSeed());
        }

        public byte[] Concatenate(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public byte[] DeriveKey(
            AssertionCrypto.RequiredParametersBase parameters,
            byte[] sharedSecretBytes,
            IDigest dm)
        {
            // Wyznaczenie rozmiaru klucza do odwrappowania
            // metoda uproszczona majaca pokazac ogolny mechanizm
            int wrappedKeyArraySize = -1;
            if (parameters.KeyEncryptionMethod.Contains("kw-aes256"))
            {
                wrappedKeyArraySize = 256 / 8;
            }
            else if (parameters.KeyEncryptionMethod.Contains("kw-aes128"))
            {
                wrappedKeyArraySize = 128 / 8;
            }
            else if (parameters.KeyEncryptionMethod.Contains("kw-aes192"))
            {
                wrappedKeyArraySize = 192 / 8;
            }

            // wartosc wynika z zastosowanej dlugosci klucza w algorytmie KeyWrapping
            byte[] wrappedKeyBytes = new byte[wrappedKeyArraySize];

            ConcatenationKdfGenerator ckdf = new ConcatenationKdfGenerator(dm);

            byte[] algid = Hex.Decode(parameters.AlgorithmID);
            byte[] uinfo = Hex.Decode(parameters.PartyUInfo);
            byte[] vinfo = Hex.Decode(parameters.PartyVInfo);

            ckdf.Init(new KdfParameters(sharedSecretBytes, this.Concatenate(algid, uinfo, vinfo)));
            ckdf.GenerateBytes(wrappedKeyBytes, 0, wrappedKeyArraySize);

            return wrappedKeyBytes;
        }

    }
}
