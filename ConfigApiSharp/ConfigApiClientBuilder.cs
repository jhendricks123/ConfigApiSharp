using ConfigApiSharp.ConfigurationApiService;
using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml;
using ConfigApiSharp.ServerCommandService;

namespace ConfigApiSharp
{
    public static class ClientFactory
    {
        /// <summary>
        /// Builds a WCF channel using the <see cref="IServerCommandService"/> interface defined by the Management Server. The raw ServerCommandService channel is returned in the Client property of the BuildClientResponse object.
        /// </summary>
        /// <param name="host">Hostname or IP address of the Milestone Advanced VMS Management Server</param>
        /// <param name="port">The HTTP port of the Management Server. If UserType.BasicUser is used, the port will always be 443 regardless of what value is set here. This may change in the future if Milestone ever supports a custom HTTPS port.</param>
        /// <param name="userType">UserType.CurrentUser will use the currently logged in Windows user for authentication (CredentialCache.DefaultNetworkCredentials), so you can omit the username and password parameters. UserType.Windows and UserType.BasicUser both require you to supply the username and password.</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>BuildClientResult where Exception is null and Success is true if authentication with the Management Server was successful. If Success is false, check the exception for more information.</returns>
        public static BuildClientResult<IServerCommandService> BuildServerCommandServiceClient(string host, int port, UserType userType, string username = null, string password = null)
        {
            var uri = userType == UserType.BasicUser
                ? new Uri($"https://{host}/ManagementServer/ServerCommandService.svc")
                : new Uri($"http://{host}:{port}/ManagementServer/ServerCommandService.svc");

            var channelFactory = WcfHelpers.BuildChannelFactory<ServerCommandServiceClient>(uri, userType);

            WcfHelpers.SetChannelCredentials(channelFactory, userType, username, password);

            ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                var client = channelFactory.CreateChannel();
                var result = new BuildClientResult<IServerCommandService>()
                {
                    Name = "ServerCommandService",
                    Client = client,
                    Exception = null
                };

                return result;
            }
            catch (Exception ex)
            {
                var result = new BuildClientResult<IServerCommandService>()
                {
                    Exception = ex
                };
                return result;
            }

        }

        /// <summary>
        /// Builds a WCF channel using the <see cref="IConfigurationService"/> interface defined by the Management Server. The raw ConfigurationService channel is returned in the Client property of the BuildClientResponse object.
        /// </summary>
        /// <param name="host">Hostname or IP address of the Milestone Advanced VMS Management Server</param>
        /// <param name="port">The HTTP port of the Management Server. If UserType.BasicUser is used, the port will always be 443 regardless of what value is set here. This may change in the future if Milestone ever supports a custom HTTPS port.</param>
        /// <param name="userType">UserType.CurrentUser will use the currently logged in Windows user for authentication (CredentialCache.DefaultNetworkCredentials), so you can omit the username and password parameters. UserType.Windows and UserType.BasicUser both require you to supply the username and password.</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>BuildClientResult where Exception is null and Success is true if authentication with the Management Server was successful. If Success is false, check the exception for more information.</returns>
        public static BuildClientResult<IConfigurationService> BuildConfigApiClient(string host, int port, UserType userType, string username = null, string password = null)
        {
            var uri = userType == UserType.BasicUser
                ? new Uri($"https://{host}/ManagementServer/ConfigurationApiService.svc")
                : new Uri($"http://{host}:{port}/ManagementServer/ConfigurationApiService.svc");

            var channelFactory = WcfHelpers.BuildChannelFactory<IConfigurationService>(uri, userType);

            WcfHelpers.SetChannelCredentials(channelFactory, userType, username, password);

            ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                var client = channelFactory.CreateChannel();
                var ms = client.GetItem("/");
                var result = new BuildClientResult<IConfigurationService>()
                {
                    Name = ms.DisplayName,
                    Client = client,
                    Exception = null
                };

                var managementServer = client.GetItem("/");
                Console.WriteLine($"Connected to {managementServer.DisplayName}. Properties:");
                managementServer.Properties.ToList().ForEach(p => Console.WriteLine($"\t{p.DisplayName}: {p.Value}"));
                return result;
            }
            catch (Exception ex)
            {
                var result = new BuildClientResult<IConfigurationService>()
                {
                    Exception = ex
                };
                return result;
            }
        }
    }

    public static class ServerCommandServiceClientBuilder
    {
        public static BuildClientResult<IServerCommandService> BuildClient(string host, int port, UserType userType, string username = null, string password = null)
        {
            var uri = userType == UserType.BasicUser
                ? new Uri($"https://{host}/ManagementServer/ServerCommandService.svc")
                : new Uri($"http://{host}:{port}/ManagementServer/ServerCommandService.svc");

            var channelFactory = WcfHelpers.BuildChannelFactory<ServerCommandServiceClient>(uri, userType);

            WcfHelpers.SetChannelCredentials(channelFactory, userType, username, password);

            ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                var client = channelFactory.CreateChannel();
                var result = new BuildClientResult<IServerCommandService>()
                {
                    Name = "ServerCommandService",
                    Client = client,
                    Exception = null
                };

                return result;
            }
            catch (Exception ex)
            {
                var result = new BuildClientResult<IServerCommandService>()
                {
                    Exception = ex
                };
                return result;
            }

        }
    }

    /// <summary>
    /// The response from client builders which contains the WCF client proxy if successful, or an exception if not successful.
    /// </summary>
    public class BuildClientResult<T>
    {
        public string Name { get; set; }
        public T Client { get; set; }
        public Exception Exception { get; set; }
        public bool Success => Exception == null;
    }

    /// <summary>
    /// Determines the type of authentication, and the port used for communication with the Management Server. <see cref="UserType.BasicUser"/> always uses HTTPS over TCP port 443. Other types will use HTTPS over 80 or an alternate HTTP port.
    /// </summary>
    public enum UserType
    {
        CurrentUser,
        Windows,
        BasicUser
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
        public static BuildClientResult<IConfigurationService> BuildClient(string host, int port, UserType userType, string username = null, string password = null)
        {
            var uri = userType == UserType.BasicUser 
                ? new Uri($"https://{host}/ManagementServer/ConfigurationApiService.svc") 
                : new Uri($"http://{host}:{port}/ManagementServer/ConfigurationApiService.svc");

            var channelFactory = WcfHelpers.BuildChannelFactory<IConfigurationService>(uri, userType);

            WcfHelpers.SetChannelCredentials(channelFactory, userType, username, password);

            ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                var client = channelFactory.CreateChannel();
                var ms = client.GetItem("/");
                var result = new BuildClientResult<IConfigurationService>()
                {
                    Name = ms.DisplayName,
                    Client = client,
                    Exception = null
                };

                var managementServer = client.GetItem("/");
                Console.WriteLine($"Connected to {managementServer.DisplayName}. Properties:");
                managementServer.Properties.ToList().ForEach(p => Console.WriteLine($"\t{p.DisplayName}: {p.Value}"));
                return result;
            }
            catch (Exception ex)
            {
                var result = new BuildClientResult<IConfigurationService>()
                {
                    Exception = ex
                };
                return result;
            }
        }
    }
}
