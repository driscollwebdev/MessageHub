namespace MessageHub.Security
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// A class that provides a public key, key management, and decryption services.
    /// </summary>
    public sealed class SecureKeyProvider : IDisposable
    {
        /// <summary>
        /// The container name to use for this object's lifetime
        /// </summary>
        private string _secureKeyContainerName = Guid.NewGuid().ToString();

        /// <summary>
        /// The asymmetric key pair
        /// </summary>
        private string _secureKey;

        /// <summary>
        /// An object used to control access to the key
        /// </summary>
        private readonly object _keyGenLock = new object();

        /// <summary>
        /// Gets a value (as an XML string) for the public key in use by this instance
        /// </summary>
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

        /// <summary>
        /// Decrypts the encrypted data using the instance's private key
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <returns>A buffer containing decrypted data</returns>
        public byte[] Decrypt(byte[] data)
        {
            CspParameters csParams = new CspParameters();
            csParams.KeyContainerName = _secureKeyContainerName;

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(csParams);

            return rsaProvider.Decrypt(data, false);
        }

        /// <summary>
        /// Generates a full asymmetic key and stores it in a key container.
        /// </summary>
        /// <returns>An XML string containing the public key</returns>
        private string GenerateKey()
        {
            CspParameters csParams = new CspParameters();
            csParams.KeyContainerName = _secureKeyContainerName;

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(csParams);

            return rsaProvider.ToXmlString(false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes resources associated with the current instance
        /// </summary>
        /// <param name="disposing">A boolean that indicates whether the current instance is being disposed</param>
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
