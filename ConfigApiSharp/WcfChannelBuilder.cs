using System;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml;

namespace ConfigApiSharp
{
    internal static class WcfChannelBuilder
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
                binding.OpenTimeout = WcfSettings.Timeouts.OpenTimeout;
                binding.CloseTimeout = WcfSettings.Timeouts.CloseTimeout;
                binding.ReceiveTimeout = WcfSettings.Timeouts.ReceiveTimeout;
                binding.SendTimeout = WcfSettings.Timeouts.SendTimeout;
                var security = binding.Security;
                security.Message.ClientCredentialType = MessageCredentialType.Windows;

                binding.ReaderQuotas.MaxStringContentLength = WcfSettings.MaxStringContentLength;
                binding.MaxReceivedMessageSize = WcfSettings.MaxReceivedMessageSize;
                binding.MaxBufferPoolSize = WcfSettings.MaxBufferPoolSize;

                binding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;
                return binding;
            }
            else
            {
                var binding = new BasicHttpBinding
                {
                    ReaderQuotas = { MaxStringContentLength = WcfSettings.MaxStringContentLength },
                    MaxReceivedMessageSize = WcfSettings.MaxReceivedMessageSize,
                    MaxBufferSize = WcfSettings.MaxBufferSize,
                    MaxBufferPoolSize = WcfSettings.MaxBufferPoolSize,
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
                binding.OpenTimeout = WcfSettings.Timeouts.OpenTimeout;
                binding.CloseTimeout = WcfSettings.Timeouts.CloseTimeout;
                binding.ReceiveTimeout = WcfSettings.Timeouts.ReceiveTimeout;
                binding.SendTimeout = WcfSettings.Timeouts.SendTimeout;
                return binding;
            }
        }
    }
}
