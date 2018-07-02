using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ConfigApiSharp.ConfigurationApiService;

namespace ConfigApiSharp
{
    internal static class WcfHelpers
    {
        public static ChannelFactory<T> BuildChannelFactory<T>(Uri uri, UserType userType)
        {
            return new ChannelFactory<T>(
                GetBinding(userType == UserType.BasicUser, true),
                new EndpointAddress(uri, EndpointIdentity.CreateSpnIdentity("host/localhost"))
            );
        }

        public static void SetChannelCredentials<T>(ChannelFactory<T> channel, UserType userType, string username, string password)
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

        public static System.ServiceModel.Channels.Binding GetBinding(bool isBasic, bool isCorporate)
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
                    ReaderQuotas = { MaxStringContentLength = 2147483647 },
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    MaxBufferPoolSize = 2147483647,
                    HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                    MessageEncoding = WSMessageEncoding.Text,
                    TextEncoding = Encoding.UTF8,
                    UseDefaultWebProxy = true,
                    AllowCookies = false
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
