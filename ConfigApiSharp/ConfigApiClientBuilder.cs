using ConfigApiSharp.ConfigurationApiService;
using System;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml;

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

    /// <summary>
    /// The response from <see cref="ConfigApiClientBuilder.BuildClient"/> which contains the Configuration API client proxy if successful, or an exception if not successful.
    /// </summary>
    public class BuildClientResult
    {
        public string Name { get; set; }
        public IConfigurationService Client { get; set; }
        public Exception Exception { get; set; }
        public bool Success => Exception == null;
    }

    public static class ConfigApiClientBuilder
    {
        /// <summary>
        /// Builds a WCF channel using the <see cref="IConfigurationService"/> interface defined by the Management Server. The raw ConfigurationService channel is returned in the Client property of the BuildClientResponse object.
        /// </summary>
        /// <param name="host">Hostname or IP address of the Milestone Advanced VMS Management Server</param>
        /// <param name="port">The HTTP port of the Management Server. If UserType.BasicUser is used, the port will always be 443 regardless of what value is set here. This may change in the future if Milestone ever supports a custom HTTPS port.</param>
        /// <param name="userType">UserType.CurrentUser will use the currently logged in Windows user for authentication (CredentialCache.DefaultNetworkCredentials), so you can omit the username and password parameters. UserType.Windows and UserType.BasicUser both require you to supply the username and password.</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>BuildClientResult where Exception is null and Success is true if authentication with the Management Server was successful. If Success is false, check the exception for more information.</returns>
        public static BuildClientResult BuildClient(string host, int port, UserType userType, string username = null, string password = null)
        {
            var uri = userType == UserType.BasicUser 
                ? new Uri($"https://{host}/ManagementServer/ConfigurationApiService.svc") 
                : new Uri($"http://{host}:{port}/ManagementServer/ConfigurationApiService.svc");

            var channelFactory = BuildChannelFactory(uri, userType);

            SetChannelCredentials(channelFactory, userType, username, password);

            ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                var client = channelFactory.CreateChannel();
                var ms = client.GetItem("/");
                var result = new BuildClientResult()
                {
                    Name = ms.DisplayName,
                    Client = client,
                    Exception = null
                };
                return result;
            }
            catch (Exception ex)
            {
                var result = new BuildClientResult()
                {
                    Exception = ex
                };
                return result;
            }
        }

        private static ChannelFactory<IConfigurationService> BuildChannelFactory(Uri uri, UserType userType)
        {
            return new ChannelFactory<IConfigurationService>(
                GetBinding(userType == UserType.BasicUser, true),
                new EndpointAddress(uri, EndpointIdentity.CreateSpnIdentity("host/localhost"))
            );
        }

        private static void SetChannelCredentials(ChannelFactory<IConfigurationService> channel, UserType userType, string username, string password)
        {
            if (channel?.Credentials == null)
                throw new InvalidOperationException("Either channel or channel.Credentials is null.");
            switch (userType)
            {
                case UserType.BasicUser:
                    channel.Credentials.UserName.UserName = "[BASIC]\\" + username;
                    channel.Credentials.UserName.Password = password;
                    break;
                case UserType.CurrentUser:
                    channel.Credentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                    break;
                case UserType.Windows:
                    channel.Credentials.Windows.ClientCredential.UserName = username;
                    channel.Credentials.Windows.ClientCredential.Password = password;
                    break;
                default:
                    break;
            }
        }

        private static System.ServiceModel.Channels.Binding GetBinding(bool isBasic, bool isCorporate)
        {
            if (!isBasic)
            {
                var binding = new WSHttpBinding();
                var security = binding.Security;
                security.Message.ClientCredentialType = MessageCredentialType.Windows;

                binding.ReaderQuotas.MaxStringContentLength = 2147483647;
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;

                binding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;
                return binding;
            }
            else
            {
                var binding = new BasicHttpBinding
                {
                    ReaderQuotas = {MaxStringContentLength = 2147483647},
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    MaxBufferPoolSize = 2147483647,
                    HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                    MessageEncoding = WSMessageEncoding.Text,
                    TextEncoding = Encoding.UTF8,
                    UseDefaultWebProxy = true,
                    AllowCookies = false,
                    Namespace = "VideoOS.ConfigurationAPI"
                };
                if (isCorporate)
                {
                    binding.Security.Mode = BasicHttpSecurityMode.Transport;
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                }
                else
                {
                    binding.Security.Mode = BasicHttpSecurityMode.None;
                }
                return binding;
            }
        }
    }
}
