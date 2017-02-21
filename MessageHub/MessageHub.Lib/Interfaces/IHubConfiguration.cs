namespace MessageHub.Interfaces
{

    /// <summary>
    /// A marker interface for hub configurations used in ConfigureWith methods.
    /// </summary>
    public interface IHubConfiguration
    {
        /// <summary>
        /// Gets or sets a value to determine whether encryption is to be used
        /// </summary>
        bool UseEncryption { get; set; }

        /// <summary>
        /// Gets or sets a value to determine the default serialization type
        /// </summary>
        SerializationType DefaultSerializationType { get; set; }
    }
}
