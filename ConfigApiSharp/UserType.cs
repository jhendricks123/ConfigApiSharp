namespace ConfigApiSharp
{
    /// <summary>
    /// Determines the type of authentication, and the port used for communication with the Management Server. <see cref="UserType.BasicUser"/> always uses HTTPS over TCP port 443. Other types will use HTTPS over 80 or an alternate HTTP port.
    /// </summary>
    public enum UserType
    {
        CurrentUser,
        Windows,
        BasicUser
    }
}