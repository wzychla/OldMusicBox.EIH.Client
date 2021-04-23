using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.BouncyCastle.Crypto.Xml
{
    /// <summary>
    /// HMac XML signer
    /// </summary>
    internal class HMacSigner : ISigner
    {
        private bool             forSigning;
        private IDigest          digest;

        private HMacKeyParameter hmacKey;

        public HMacSigner( IDigest digest )
        {
            this.digest = digest;
        }

        public string AlgorithmName
        {
            get
            {
                return "HMACwith" + this.digest.AlgorithmName;
            }
        }

        public void Init(bool forSigning, ICipherParameters parameters)
        {
            if ( !(parameters is HMacKeyParameter))
            {
                throw new ArgumentException();
            }

            this.forSigning = forSigning;
            this.hmacKey    = (HMacKeyParameter)parameters;

            this.Reset();
        }

        public void Update(byte input)
        {
            this.digest.Update(input);
        }

        public void BlockUpdate(byte[] input, int inOff, int length)
        {
            this.digest.BlockUpdate(input, inOff, length);
        }

        public byte[] GenerateSignature()
        {
            if (!forSigning)
                throw new InvalidOperationException("HMACSigner not initialised for signature generation.");

            digest.BlockUpdate(this.hmacKey.Key, 0, this.hmacKey.Key.Length);

            byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);

            return hash;
        }

        public bool VerifySignature(byte[] signature)
        {
            if (forSigning)
                throw new InvalidOperationException("HMACSigner not initialised for signature verification.");

            digest.BlockUpdate(this.hmacKey.Key, 0, this.hmacKey.Key.Length);

            byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);

            return ((IStructuralEquatable)signature).Equals( hash, StructuralComparisons.StructuralEqualityComparer );
        }

        public void Reset()
        {
            digest.Reset();
        }

    }
}
