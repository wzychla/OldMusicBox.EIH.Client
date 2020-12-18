﻿using OldMusicBox.EIH.Client.Constants;
using OldMusicBox.EIH.Client.Model;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement.Kdf;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Xml;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Decryption
{
    /// <summary>
    /// Electronic Identification Hub (Węzeł Krajowy) assertion decryptor
    /// </summary>
    public class AssertionDecryptor
    {
        /*
		<saml2:EncryptedAssertion>
			<xenc:EncryptedData Id="_789e7867f334062e4e08b7eee35db518" Type="http://www.w3.org/2001/04/xmlenc#Element">
				<xenc:EncryptionMethod Algorithm="http://www.w3.org/2009/xmlenc11#aes256-gcm">
					<ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
				</xenc:EncryptionMethod>
				<ds:KeyInfo>
					<xenc:EncryptedKey Id="_6b6de1b238e2b020bac91baeec66b645">
						<xenc:EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#kw-aes256"/>
						<ds:KeyInfo>
							<xenc:AgreementMethod Algorithm="http://www.w3.org/2009/xmlenc11#ECDH-ES">
								<xenc11:KeyDerivationMethod Algorithm="http://www.w3.org/2009/xmlenc11#ConcatKDF">
									<xenc11:ConcatKDFParams AlgorithmID="0000002A687474703A2F2F7777772E77332E6F72672F323030312F30342F786D6C656E63236B772D616573323536" PartyUInfo="000000136C6F63616C686F73742E776B2E676F762E706C" PartyVInfo="00000016687474703A2F2F6C6F63616C686F73742E73702E706C">
										<ds:DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
									</xenc11:ConcatKDFParams>
								</xenc11:KeyDerivationMethod>
								<xenc:OriginatorKeyInfo>
									<ds:KeyValue>
										<dsig11:ECKeyValue>
											<dsig11:NamedCurve URI="urn:oid:1.2.840.10045.3.1.7"/>
											<dsig11:PublicKey>BCeudwbIlDrRHcvu2VUrhtUozbu2msFUdedOut4Lr3tDgq1yg5u9rE8pwGz9PWPgj9cG4CbXWqBL0epJdxqnbqg=</dsig11:PublicKey>
										</dsig11:ECKeyValue>
									</ds:KeyValue>
								</xenc:OriginatorKeyInfo>
							</xenc:AgreementMethod>
						</ds:KeyInfo>
						<xenc:CipherData>
							<xenc:CipherValue>ttwO7IqRHosYe80qqgrVgnCfHPSWpe5Ml8vOVuCmaUTpfyF6I+ZH0w==</xenc:CipherValue>
						</xenc:CipherData>
					</xenc:EncryptedKey>
				</ds:KeyInfo>
				<xenc:CipherData>
					<xenc:CipherValue>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</xenc:CipherValue>
				</xenc:CipherData>
			</xenc:EncryptedData>
		</saml2:EncryptedAssertion>
        */
        public class RequiredParameters
        {
            // Parametr KDF - algorithmID
            public string AlgorithmID;
            // Parametr KDF - partyUInfo, identyfikator nadawcy
            public string PartyUInfo;
            // Parametr KDF - partyVInfo, identyfikator odbiorcy
            public string PartyVInfo;
            // Identyfikator funkcji skrotu w operacji KDF
            public string DigestMethodString;
            // Identyfikator krzywej
            public string NamedCurveOid;
            // Dane klucza publicznego efemerycznego nadawcy
            public byte[] PublicKeyBytes;
            // Zaszyfrowane dane
            public string EncryptedDataCipher;
            // Zaszyfrowany klucz
            public string EncryptedKeyCipher;
            // Algorytm uzyty do zaszyfrowania klucza
            public string KeyEncryptionMethod;
        }

        public Assertion Decrypt(
            Saml2SecurityToken token,
            AsymmetricKeyParameter privateKey)
        {
            // Odczytanie istnotnych parametrow z odczytanego komunikatu ArtifactResponse (w szczegolnosci z elementu EncryptedAssertion)
            var parameters = ExtractRequiredParameters(token);

            // Odtworzenie publicznego klucza efemerycznego z postaci przekazanej w zaszyfrowanej asercji
            // Procedura bazuje na paraemtrach odczytanych z danych od dostawcy tozsamosci 'OriginatorKeyInfo'
            ECDomainParameters ecNamedCurveParameterSpec = GetCurveParameters(parameters.NamedCurveOid);

            ECCurve curve = ecNamedCurveParameterSpec.Curve;
            ECPoint ecPoint = curve.DecodePoint(parameters.PublicKeyBytes);

            // klucz publiczny efemeryczny nadawcy
            AsymmetricKeyParameter ecPublicKey = new ECPublicKeyParameters(ecPoint, ecNamedCurveParameterSpec);

            // Wykonanie operacji KeyAgreement
            IBasicAgreement keyAgreement = AgreementUtilities.GetBasicAgreement("ECDH");
            keyAgreement.Init(privateKey);

            // zrzucenie efektu key agreement do tablicy
            byte[] sharedSecret = keyAgreement.CalculateAgreement(ecPublicKey).ToByteArrayUnsigned();
            IDigest digestMethod = DigestUtilities.GetDigest(parameters.DigestMethodString);

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

            // wykonanie funkcji KDF
            this.DeriveKey(parameters, sharedSecret, wrappedKeyBytes, digestMethod, wrappedKeyArraySize);

            // odszyfrowanie klucza ktorym nadawca zaszyfrowal dane
            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", wrappedKeyBytes);
            byte[] encryptionKey = EncryptedXml.DecryptKey(Convert.FromBase64String(parameters.EncryptedKeyCipher), keyParameter);

            // uzycie klucza uzyskanego powyzej do odszyfrowania danych (asercji)
            var cipherText = Convert.FromBase64String(parameters.EncryptedDataCipher);

            EncryptionService es = new EncryptionService(256, 128, 96);
            byte[] decryptedDoc = es.DecryptWithKey(cipherText, encryptionKey);
            string decryptedString = Encoding.UTF8.GetString(decryptedDoc);

            using (StringReader sr = new StringReader(decryptedString))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Assertion), Namespaces.ASSERTION);

                return xs.Deserialize(sr) as Assertion;
            }
        }

        private RequiredParameters ExtractRequiredParameters(Saml2SecurityToken token)
        {
            var returnedValue = new RequiredParameters();

            // token
            var encryptedData = token.Response.EncryptedAssertion.FirstOrDefault().EncryptedData;
            var encryptedKey  = token.Response.EncryptedAssertion.FirstOrDefault().EncryptedData.KeyInfo.EncryptedKey;

            returnedValue.KeyEncryptionMethod = encryptedKey.EncryptionMethod.Algorithm;
            returnedValue.PublicKeyBytes = Convert.FromBase64String(encryptedKey.KeyInfo.AgreementMethod.OriginatorKeyInfo.KeyValue.ECKeyValue.PublicKey.Text);
            returnedValue.NamedCurveOid = encryptedKey.KeyInfo.AgreementMethod.OriginatorKeyInfo.KeyValue.ECKeyValue.NamedCurve.URI.Replace("urn:oid:", "");

            var concatKDFParams = encryptedKey.KeyInfo.AgreementMethod.KeyDerivationMethod.ConcatKDFParams;

            returnedValue.AlgorithmID = concatKDFParams.AlgorithmID;
            returnedValue.PartyUInfo = concatKDFParams.PartyUInfo;
            returnedValue.PartyVInfo = concatKDFParams.PartyVInfo;

            returnedValue.DigestMethodString = concatKDFParams.DigestMethod.Algorithm.Substring(concatKDFParams.DigestMethod.Algorithm.IndexOf("#") + 1);

            returnedValue.EncryptedDataCipher = encryptedData.CipherData.CipherValue.Text;
            returnedValue.EncryptedKeyCipher = encryptedKey.CipherData.CipherValue.Text;

            return returnedValue;
        }

        private void DeriveKey(
            RequiredParameters parameters,
            byte[] sharedSecretBytes,
            byte[] wrappedKeyBytes,
            IDigest dm,
            int length)
        {
            ConcatenationKdfGenerator ckdf = new ConcatenationKdfGenerator(dm);

            byte[] algid = Hex.Decode(parameters.AlgorithmID);
            byte[] uinfo = Hex.Decode(parameters.PartyUInfo);
            byte[] vinfo = Hex.Decode(parameters.PartyVInfo);

            ckdf.Init(new KdfParameters(sharedSecretBytes, Concatenate(algid, uinfo, vinfo)));
            ckdf.GenerateBytes(wrappedKeyBytes, 0, length);
        }

        private byte[] Concatenate(params byte[][] arrays)
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

        private static ECDomainParameters GetCurveParameters(string oid)
        {
            X9ECParameters ecP = ECNamedCurveTable.GetByOid(new DerObjectIdentifier(oid));

            if (ecP == null)
                throw new Exception("Unknown curve oid: " + oid);

            return new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H, ecP.GetSeed());
        }
    }


    /// <summary>
    /// Code taken from this Stack question: 
    /// http://codereview.stackexchange.com/questions/14892/review-of-simplified-secure-encryption-of-a-string
    /// https://github.com/lukemerrett/Bouncy-Castle-AES-GCM-Encryption
    /// 
    /// The below code uses AES GCM using a 256bit key.
    /// 
    /// A non secret payload byte[] can be provided as well that won't be encrypted but will be authenticated with GCM.
    /// </summary>
    public class EncryptionService
    {
        #region Constants and Fields

        private const int DEFAULT_KEY_BIT_SIZE = 256;
        private const int DEFAULT_MAC_BIT_SIZE = 128;
        private const int DEFAULT_NONCE_BIT_SIZE = 128;

        private readonly int _keySize;
        private readonly int _macSize;
        private readonly int _nonceSize;

        private readonly SecureRandom _random;

        #endregion

        #region Constructors and Destructors

        public EncryptionService()
            : this(DEFAULT_KEY_BIT_SIZE, DEFAULT_MAC_BIT_SIZE, DEFAULT_NONCE_BIT_SIZE)
        { }

        public EncryptionService(int keyBitSize, int macBitSize, int nonceBitSize)
        {
            _random = new SecureRandom();

            _keySize = keyBitSize;
            _macSize = macBitSize;
            _nonceSize = nonceBitSize;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Simple Decryption & Authentication (AES-GCM) of a UTF8 Message
        /// </summary>
        public string DecryptWithKey(string encryptedMessage, string key, int nonSecretPayloadLength = 0)
        {
            if (string.IsNullOrEmpty(encryptedMessage))
            {
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
            }

            var decodedKey = Convert.FromBase64String(key);
            var cipherText = Convert.FromBase64String(encryptedMessage);
            var plaintext = DecryptWithKey(cipherText, decodedKey, nonSecretPayloadLength);

            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>
        /// Simple Encryption And Authentication (AES-GCM) of a UTF8 string.
        /// </summary>
        public string EncryptWithKey(string messageToEncrypt, string key, byte[] nonSecretPayload = null)
        {
            if (string.IsNullOrEmpty(messageToEncrypt))
            {
                throw new ArgumentException("Secret Message Required!", "messageToEncrypt");
            }

            var decodedKey = Convert.FromBase64String(key);

            var plainText = Encoding.UTF8.GetBytes(messageToEncrypt);
            var cipherText = EncryptWithKey(plainText, decodedKey, nonSecretPayload);
            return Convert.ToBase64String(cipherText);
        }

        /// <summary>
        /// Helper that generates a random new key on each call.
        /// </summary>
        /// <returns>Base 64 encoded string</returns>
        public string NewKey()
        {
            var key = new byte[_keySize / 8];
            _random.NextBytes(key);
            return Convert.ToBase64String(key);
        }

        #endregion

        #region Methods

        public byte[] DecryptWithKey(byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
        {
            //User Error Checks
            CheckKey(key);

            if (encryptedMessage == null || encryptedMessage.Length == 0)
            {
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
            }

            using (var cipherStream = new MemoryStream(encryptedMessage))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                //Grab Payload
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);

                //Grab Nonce
                var nonce = cipherReader.ReadBytes(_nonceSize / 8);

                var cipher = new GcmBlockCipher(new AesEngine());

                //var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), _macSize, nonce, nonSecretPayload);
                cipher.Init(false, parameters);

                //Decrypt Cipher Text
                var cipherText = cipherReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength - nonce.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);

                string decoded = Encoding.UTF8.GetString(plainText);

                cipher.DoFinal(plainText, len);

                return plainText;
            }
        }

        public byte[] EncryptWithKey(byte[] messageToEncrypt, byte[] key, byte[] nonSecretPayload = null)
        {
            //User Error Checks
            CheckKey(key);

            //Non-secret Payload Optional
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            //Using random nonce large enough not to repeat
            var nonce = new byte[_nonceSize / 8];
            _random.NextBytes(nonce, 0, nonce.Length);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), _macSize, nonce, nonSecretPayload);
            cipher.Init(true, parameters);

            //Generate Cipher Text With Auth Tag
            var cipherText = new byte[cipher.GetOutputSize(messageToEncrypt.Length)];
            var len = cipher.ProcessBytes(messageToEncrypt, 0, messageToEncrypt.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            //Assemble Message
            using (var combinedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(combinedStream))
                {
                    //Prepend Authenticated Payload
                    binaryWriter.Write(nonSecretPayload);
                    //Prepend Nonce
                    binaryWriter.Write(nonce);
                    //Write Cipher Text
                    binaryWriter.Write(cipherText);
                }
                return combinedStream.ToArray();
            }
        }

        private void CheckKey(byte[] key)
        {
            if (key == null || key.Length != _keySize / 8)
            {
                throw new ArgumentException(String.Format("Key needs to be {0} bit! actual:{1}", _keySize, key?.Length * 8), "key");
            }
        }

        #endregion
    }
}

