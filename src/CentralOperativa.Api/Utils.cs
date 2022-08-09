using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using System;

namespace CentralOperativa
{
    public static class Utils
    {
        public static string SqlLike(string value)
        {
            return "%" + value + "%";
        }

        public static string EntornoWsAFIP
        {
            get
            {
                var configuration = HostContext.Resolve<IConfiguration>();
                var entornoWsAFIP = configuration["EntornoWsAFIP"];
                switch (entornoWsAFIP)
                {
                    case "Produccion":
                        return "https://wsaa.afip.gov.ar/ws/services/LoginCms?WSDL";
                    case "Homologacion":
                        return "https://wsaahomo.afip.gov.ar/ws/services/LoginCms?WSDL";
                    default:
                        throw new ConfigurationErrorsException("EntornoWsAFIP appseting is invalid.");
                }
            }
        }

        public static string[] WebSiteLoadCertificates
        {
            get
            {
                var configuration = HostContext.Resolve<IConfiguration>();
                var webSiteLoadCertificates = configuration["WEBSITE_LOAD_CERTIFICATES"];
                return !string.IsNullOrEmpty(webSiteLoadCertificates) ? webSiteLoadCertificates.Split(';') : null;
            }
        }

        public static StoreLocation CertificateStoreLocation
        {
            get
            {
                var configuration = HostContext.Resolve<IConfiguration>();
                var keyStore = configuration["KeyStore"] ?? "LocalMachine";
                switch (keyStore)
                {
                    case "LocalMachine":
                        return StoreLocation.LocalMachine;
                    case "CurrentUser":
                        return StoreLocation.CurrentUser;
                    default:
                        throw new ConfigurationErrorsException("KeyStore appseting is invalid.");
                }
            }
        }

        public static void SetSessionContextValue<T>(this System.Data.IDbConnection db, string name, T value)
        {
            if (db is System.Data.SqlClient.SqlConnection conn)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_set_session_context";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add("@key", System.Data.SqlDbType.NVarChar).Value = name;
                if (value is Int32)
                {
                    cmd.Parameters.Add("@value", System.Data.SqlDbType.Int).Value = value;
                }
                if (value is string)
                {
                    cmd.Parameters.Add("@value", System.Data.SqlDbType.NVarChar).Value = value;
                }
                cmd.ExecuteNonQuery();
            }
        }
    }
}
