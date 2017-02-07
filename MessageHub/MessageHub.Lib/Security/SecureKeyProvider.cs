namespace MessageHub.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class SecureKeyProvider : IDisposable
    {
        private string _secureKeyContainerName = Guid.NewGuid().ToString();

        private string _secureKey;

        private object _keyGenLock = new object();

        public string PublicKey
        {
            get
            {
                if (string.IsNullOrEmpty(_secureKey))
                {
                    lock (_keyGenLock)
                    {
                        if (string.IsNullOrEmpty(_secureKey))
                        {
                            _secureKey = GenerateKey();
                        }
                    }
                }

                return _secureKey;
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            CspParameters csParams = new CspParameters();
            csParams.KeyContainerName = _secureKeyContainerName;

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(csParams);

            return rsaProvider.Decrypt(data, false);
        }

        private string GenerateKey()
        {
            CspParameters csParams = new CspParameters();
            csParams.KeyContainerName = _secureKeyContainerName;

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(csParams);

            return rsaProvider.ToXmlString(false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CspParameters csParams = new CspParameters();
                    csParams.KeyContainerName = _secureKeyContainerName;
                    RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(csParams);
                    rsaProvider.PersistKeyInCsp = false;
                    rsaProvider.Clear();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
