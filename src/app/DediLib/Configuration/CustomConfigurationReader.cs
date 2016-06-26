using System;
using System.Configuration;

namespace DediLib.Configuration
{
    public class CustomConfigurationReader
    {
        public string GetAppSetting(string name)
        {
            var machineName = Environment.MachineName;
            var value = ConfigurationManager.AppSettings.Get(name + "-" + machineName);
            if (value != null) return value;
            
            return ConfigurationManager.AppSettings.Get(name);
        }

        public string GetConnectionString(string name)
        {
            var machineName = Environment.MachineName;

            var connectionString = ConfigurationManager.ConnectionStrings[name + "-" + machineName];
            if (connectionString != null) return connectionString.ConnectionString;

            connectionString = ConfigurationManager.ConnectionStrings[name];
            if (connectionString != null) return connectionString.ConnectionString;
            
            throw new InvalidOperationException($"ConnectionString not found '{name}'");
        }
    }
}