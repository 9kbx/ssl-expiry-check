using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SSLExpiryCheck
{
    public class SSLChecker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout">timeout seconds</param>
        /// <returns></returns>
        public static async Task<Dictionary<string,DateTime?>> GetExpirationDateAsync(string hostname, CancellationToken cancellationToken = default(CancellationToken), int timeout = 5)
        {
            // ref:https://stackoverflow.com/a/73299738

            var result = new Dictionary<string, DateTime?>();
            
            // Create an HttpClientHandler object and set to use default credentials
            using var handler = new HttpClientHandler();

            // Set custom server validation callback
            handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, x509Chain, SslPolicyErrors) =>
            {
#if DEBUG
                // It is possible inpect the certificate provided by server
                Console.WriteLine($"Requested URI: {requestMessage.RequestUri}");
#endif

                if (certificate is null)
                    return false;

                var host = requestMessage.RequestUri.Host;
                if (!result.ContainsKey(host))
                {
                    result.Add(host, null);
                }

                if (DateTime.TryParse(certificate.GetExpirationDateString(), out var expDate))
                {
                    result[host] = expDate;
                }
#if DEBUG
                Console.WriteLine($"Effective date: {certificate.GetEffectiveDateString()}");
                Console.WriteLine($"Exp date: {certificate.GetExpirationDateString()}");
                Console.WriteLine($"Issuer: {certificate.Issuer}");
                Console.WriteLine($"Subject: {certificate.Subject}");
#endif

                return true;
            };

            // Create an HttpClient object
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(timeout);

            try
            {
                using HttpResponseMessage response = await client.GetAsync($"https://{hostname}/", cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // wait for callback
            //await Task.Delay(10000);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout">timeout seconds</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, int>> GetRemainingDaysAsync(string hostname, CancellationToken cancellationToken = default(CancellationToken), int timeout = 5)
        {
            var expirationDate = await GetExpirationDateAsync(hostname, cancellationToken, timeout);
            if (expirationDate != null && expirationDate.Count > 0)
            {
                var result = new Dictionary<string, int>();
                foreach (var item in expirationDate)
                {
                    result.Add(item.Key, int.MinValue);
                    if (item.Value.HasValue)
                    {
                        var daysRemaining = item.Value.Value.Subtract(DateTime.Now).Days;
                        result[item.Key] = daysRemaining;
#if DEBUG
                        Console.WriteLine($"Days Remaining: {daysRemaining}\t{item.Key}");
#endif
                    }
                }
                return result;
            }
            return null;
        }
    }
}
