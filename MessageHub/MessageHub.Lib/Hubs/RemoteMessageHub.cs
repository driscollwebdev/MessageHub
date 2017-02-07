namespace MessageHub.Hubs
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using System.Security.Cryptography;
    using System.IO;
    using System.Runtime.Serialization;
    using Security;

    /// <summary>
    /// An abstract class representing a message hub that is connected to a remote server
    /// </summary>
    public abstract class RemoteMessageHub : IRemoteMessageHub, IDisposable
    {
        /// <summary>
        /// A contained local hub used for much of the basic operation of this hub
        /// </summary>
        private LocalMessageHub _innerHub;

        private Lazy<SecureKeyProvider> _keyProvider = new Lazy<SecureKeyProvider>(() => new SecureKeyProvider());

        private SecureKeyProvider KeyProvider
        {
            get
            {
                return _keyProvider.Value;
            }
        }

        protected bool UseEncryption { get; set; }

        /// <summary>
        /// Gets a value for the Id of this instance
        /// </summary>
        public Guid Id
        {
            get
            {
                return _innerHub.Id;
            }
        }

        public string PublicKey
        {
            get
            {
                return KeyProvider.PublicKey;
            }
        }

        public string RemotePublicKey { get; set; }

        /// <summary>
        /// Broadcasts a message to all receivers. Must be overridden in a derived class.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        /// <returns>A task for continuation</returns>
        public abstract Task Broadcast(Message message);

        public abstract Task Broadcast(SecureMessageContainer secureMessage);

        /// <summary>
        /// Gets or creates the channel indicated by name
        /// </summary>
        /// <param name="name">The name of the channel to retrieve</param>
        /// <returns>An instance of the <see cref="Channel"/> class</returns>
        public Channel Channel(string name)
        {
            return _innerHub.Channel(name);
        }

        /// <summary>
        /// Receive a message from another source.
        /// </summary>
        /// <param name="message">The message to receive</param>
        /// <returns>A task for continuation purposes</returns>
        public virtual Task Receive(Message message)
        {
            return _innerHub.Receive(message);
        }

        public virtual Task Receive(SecureMessageContainer secureMessage)
        {
            Message message = Decrypt(secureMessage);

            return Receive(message);
        }

        /// <summary>
        /// Connects this hub to a remote service.
        /// </summary>
        /// <returns>A task, for async/continuation purposes</returns>
        public virtual Task Connect()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnects this hub from the remote service, performing any necessary cleanup tasks.
        /// </summary>
        public virtual void Disconnect()
        {
        }

        /// <summary>
        /// Configures this hub instance with the configuration provided
        /// </summary>
        /// <param name="config">The configuration object</param>
        /// <returns>The current instance</returns>
        public abstract IRemoteMessageHub WithConfiguration(IHubConfiguration config);

        /// <summary>
        /// Configures this hub instance with the configuration function (lambda) provided
        /// </summary>
        /// <param name="configure">The configuration function</param>
        /// <returns>The current instance</returns>
        public virtual void Configure<THubConfiguration>(Action<THubConfiguration> configure) where THubConfiguration : IHubConfiguration
        {
            THubConfiguration configObj = Activator.CreateInstance<THubConfiguration>();
            configure(configObj);
            WithConfiguration(configObj);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMessageHub"/> class.
        /// </summary>
        /// <param name="inner">The inner hub</param>
        protected RemoteMessageHub(LocalMessageHub inner)
        {
            if (inner == null)
            {
                inner = (LocalMessageHub)LocalMessageHub.Create();
            }

            _innerHub = inner;
            _innerHub.Broadcasting += OnInnerHubBroadcast;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMessageHub"/> class.
        /// </summary>
        protected RemoteMessageHub() : this(null) { }

        private void OnInnerHubBroadcast(object sender, MessageEventArgs msgArgs)
        {
            if (UseEncryption)
            {
                SecureMessageContainer container = Encrypt(msgArgs.Message);
                Broadcast(container);
            }
            else
            {
                Broadcast(msgArgs.Message);
            }
        }

        private string GenerateKey()
        {
            CspParameters csParams = new CspParameters();
            csParams.KeyContainerName = Id.ToString();

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(csParams);

            return rsaProvider.ToXmlString(false);
        }

        protected SecureMessageContainer Encrypt(Message msg)
        {
            SecureMessageContainer container = new SecureMessageContainer();
            RijndaelManaged rmCrypto = new RijndaelManaged();

            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.FromXmlString(RemotePublicKey);

            container.EncryptedKey = rsaProvider.Encrypt(rmCrypto.Key, false);
            container.EncryptedIV = rsaProvider.Encrypt(rmCrypto.IV, false);

            using (MemoryStream enc = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(enc, rmCrypto.CreateEncryptor(), CryptoStreamMode.Write))
            using (MemoryStream ser = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Message));
                serializer.WriteObject(ser, msg);
                ser.Position = 0;

                ser.WriteTo(cs);

                cs.FlushFinalBlock();

                container.EncryptedData = enc.ToArray();
            }

            return container;
        }

        private Message Decrypt(SecureMessageContainer container)
        {
            Message msg = null;

            RijndaelManaged rmCrypto = new RijndaelManaged();

            byte[] key = KeyProvider.Decrypt(container.EncryptedKey);
            byte[] iv = KeyProvider.Decrypt(container.EncryptedIV);

            using (MemoryStream encrypted = new MemoryStream(container.EncryptedData))
            using (CryptoStream cs = new CryptoStream(encrypted, rmCrypto.CreateDecryptor(key, iv), CryptoStreamMode.Read))
            {
                byte[] buffer = new byte[encrypted.Length];

                int bytesRead = cs.Read(buffer, 0, (int)encrypted.Length);

                using (MemoryStream decrypted = new MemoryStream(bytesRead))
                {
                    decrypted.Write(buffer, 0, bytesRead);
                    decrypted.Position = 0;

                    DataContractSerializer serializer = new DataContractSerializer(typeof(Message));
                    msg = (Message)serializer.ReadObject(decrypted);
                }
            }

            return msg;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    KeyProvider.Dispose();
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
