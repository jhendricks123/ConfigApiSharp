using ConfigApiSharp.ConfigurationApiService;
using ConfigApiSharp.ServerCommandService;
using System;
using System.Net;

namespace ConfigApiSharp
{
    /// <summary>
    /// Utility class to generate WCF proxy clients for interacting with XProtect Management Server.
    /// </summary>
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
            var authType = userType == UserType.BasicUser ? "Basic" : "Negotiate";
            var nc = userType == UserType.CurrentUser
                ? CredentialCache.DefaultNetworkCredentials
                : userType == UserType.Windows
                    ? new NetworkCredential(username, password)
                    : new NetworkCredential($@"[BASIC]\{username}", password);
            var cc = new CredentialCache { { uri, authType, nc } };
            return BuildServerCommandServiceClient(uri, userType, cc);
        }

        public static BuildClientResult<IServerCommandService> BuildServerCommandServiceClient(Uri uri, UserType userType, CredentialCache cc)
        {
            var uriBuilder = new UriBuilder(uri) {Path = "/ManagementServer/ServerCommandService.svc"};
            var channelFactory = WcfChannelBuilder.BuildChannelFactory<IServerCommandService>(uriBuilder.Uri, userType);
            var authType = userType == UserType.BasicUser ? "Basic" : "Negotiate";
            var nc = cc.GetCredential(uri, authType) ?? throw new InvalidOperationException($"No NetworkCredential found matching {uri}");
            if (userType == UserType.BasicUser)
            {
                channelFactory.Credentials.UserName.UserName = nc.UserName;
                channelFactory.Credentials.UserName.Password = nc.Password;
            }
            else
            {
                channelFactory.Credentials.Windows.ClientCredential = nc;
            }

            ServicePointManager.ServerCertificateValidationCallback = WcfSettings.RemoteCertificateValidationCallback;

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
            var authType = userType == UserType.BasicUser ? "Basic" : "Negotiate";
            var nc = userType == UserType.CurrentUser 
                ? CredentialCache.DefaultNetworkCredentials 
                : userType == UserType.Windows
                    ? new NetworkCredential(username, password)
                    : new NetworkCredential($@"[BASIC]\{username}", password);
            var cc = new CredentialCache {{uri, authType, nc}};
            return BuildConfigApiClient(uri, userType, cc);
        }

        public static BuildClientResult<IConfigurationService> BuildConfigApiClient(Uri uri, UserType userType, CredentialCache cc)
        {
            var uriBuilder = new UriBuilder(uri) {Path = "/ManagementServer/ConfigurationApiService.svc"};
            var channelFactory = WcfChannelBuilder.BuildChannelFactory<IConfigurationService>(uriBuilder.Uri, userType);
            var authType = userType == UserType.BasicUser ? "Basic" : "Negotiate";
            var nc = cc.GetCredential(uri, authType) ?? throw new InvalidOperationException($"No NetworkCredential found matching {uri}");
            if (userType == UserType.BasicUser)
            {
                channelFactory.Credentials.UserName.UserName = nc.UserName;
                channelFactory.Credentials.UserName.Password = nc.Password;
            }
            else
            {
                channelFactory.Credentials.Windows.ClientCredential = nc;
            }

            ServicePointManager.ServerCertificateValidationCallback = WcfSettings.RemoteCertificateValidationCallback;

            try
            {
                var client = channelFactory.CreateChannel();
                var result = new BuildClientResult<IConfigurationService>()
                {
                    Name = "ConfigurationService",
                    Client = client,
                    Exception = null
                };
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