using System;
using System.Runtime.Serialization;

namespace SE.AppConfiguration.Core
{
    [Serializable]
    internal class SecureAppConfigException : Exception
    {

        public SecureAppConfigException(string message) : base(message)
        {
        }

        public SecureAppConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}