using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Npgsql;

namespace CryptoTracking
{
    public class BackCrupto
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<(string Name, double? Price, double? Volume24h, double? PercentChange1h, double? PercentChange24h, double? PercentChange7d, DateTime? LastUpdated)> MainInfo(string cryptoName)
        {
            string API_KEY = "aa6c25cd-275a-4ec7-89b7-26c00e85be39";
            string apiUrl = $"https://pro-api.coinmarketcap.com/v2/cryptocurrency/quotes/latest?symbol={cryptoName}";

            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", API_KEY);
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    CryptoInfo cryptoData = JsonConvert.DeserializeObject<CryptoInfo>(jsonResponse);

                    if (cryptoData.Data != null && cryptoData.Data.Count > 0)
                    {
                        var cryptoKey = cryptoData.Data.Keys.First();
                        var crypto = cryptoData.Data[cryptoKey].First();

                        return (
                            crypto.Name,
                            crypto.Quote.Usd.Price.HasValue ? Math.Round(crypto.Quote.Usd.Price.Value, 3) : (double?)null,
                            crypto.Quote.Usd.Volume24h.HasValue ? Math.Round(crypto.Quote.Usd.Volume24h.Value, 3) : (double?)null,
                            crypto.Quote.Usd.PercentChange1h.HasValue ? Math.Round(crypto.Quote.Usd.PercentChange1h.Value, 3) : (double?)null,
                            crypto.Quote.Usd.PercentChange24h.HasValue ? Math.Round(crypto.Quote.Usd.PercentChange24h.Value, 3) : (double?)null,
                            crypto.Quote.Usd.PercentChange7d.HasValue ? Math.Round(crypto.Quote.Usd.PercentChange7d.Value, 3) : (double?)null,
                            crypto.Quote.Usd.LastUpdated != null ? DateTime.Parse(crypto.Quote.Usd.LastUpdated) : (DateTime?)null
                        );
                    }
                    else
                    {
                        Console.WriteLine("Данные не найдены.");
                        return (null, null, null, null, null, null, null);
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Детали ошибки: {errorResponse}");
                    return (null, null, null, null, null, null, null);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Ошибка при запросе к API: {e.Message}");
                return (null, null, null, null, null, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Неизвестная ошибка: {e.Message}");
                return (null, null, null, null, null, null, null);
            }
        }

        // Классы для десериализации
        public class CryptoInfo
        {
            [JsonProperty("data")]
            public Dictionary<string, List<Information>> Data { get; set; }
        }

        public class Information
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("symbol")]
            public string Symbol { get; set; }

            [JsonProperty("circulating_supply")]
            public double CirculatingSupply { get; set; }

            [JsonProperty("market_cap_by_total_supply_strict")]
            public double MarketCapByTotalSupplyStrict { get; set; }

            [JsonProperty("last_updated")]
            public string LastUpdated { get; set; }

            [JsonProperty("percent_change_1h")]
            public double? PercentChange1h { get; set; }

            [JsonProperty("quote")]
            public QuoteData Quote { get; set; }
        }

        public class QuoteData
        {
            [JsonProperty("USD")]
            public UsdData Usd { get; set; }
        }

        public class UsdData
        {
            [JsonProperty("price")]
            public double? Price { get; set; }

            [JsonProperty("volume_24h")]
            public double? Volume24h { get; set; }

            [JsonProperty("volume_change_24h")]
            public double? VolumeChange24h { get; set; }

            [JsonProperty("percent_change_1h")]
            public double? PercentChange1h { get; set; }

            [JsonProperty("percent_change_24h")]
            public double? PercentChange24h { get; set; }

            [JsonProperty("percent_change_7d")]
            public double? PercentChange7d { get; set; }

            [JsonProperty("market_cap")]
            public double? MarketCap { get; set; }

            [JsonProperty("market_cap_dominance")]
            public double? MarketCapDominance { get; set; }

            [JsonProperty("fully_diluted_market_cap")]
            public double? FullyDilutedMarketCap { get; set; }

            [JsonProperty("last_updated")]
            public string LastUpdated { get; set; }
        }
    }

    public class DatabaseManager
    {
        private static string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=80085;";

        public static string CreateDatabase()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("CREATE DATABASE CryptoDB;", conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                        return "База данных создана!";
                    }
                    catch (Exception ex)
                    {
                        return $"Ошибка: {ex.Message}";
                    }
                }
            }
        }
    }
}