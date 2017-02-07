namespace MessageHub.Interfaces
{

    /// <summary>
    /// A marker interface for hub configurations used in ConfigureWith methods.
    /// </summary>
    public interface IHubConfiguration
    {
        bool UseEncryption { get; set; }
    }
}
