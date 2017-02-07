namespace MessageHub
{
    /// <summary>
    /// An enumeration of available serialization types
    /// </summary>
    public enum SerializationType
    {
        /// <summary>
        /// Default serialization method, typically binary
        /// </summary>
        Default,
        /// <summary>
        /// JSON serialization
        /// </summary>
        Json,
        /// <summary>
        /// XML serialization
        /// </summary>
        Xml
    }
}
