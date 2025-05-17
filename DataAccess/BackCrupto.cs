using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Npgsql;

namespace CryptoTracking
{
    public class BackCrupto
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<(
            string Name,
            int Id,
            string Symbol,
            decimal Price,
            decimal Volume24h,
            decimal VolumeChange24h,
            decimal PercentChange1h,
            decimal PercentChange24h,
            decimal PercentChange7d,
            decimal MarketCap,
            decimal MarketCapDominance,
            decimal FullyDilutedMarketCap,
            DateTime LastUpdated
        )> MainInfo(string cryptoName)
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
                            crypto.Id,
                            crypto.Symbol,
                            crypto.Quote.Usd.Price ?? 0,
                            crypto.Quote.Usd.Volume24h ?? 0,
                            crypto.Quote.Usd.VolumeChange24h ?? 0,
                            crypto.Quote.Usd.PercentChange1h ?? 0,
                            crypto.Quote.Usd.PercentChange24h ?? 0,
                            crypto.Quote.Usd.PercentChange7d ?? 0,
                            crypto.Quote.Usd.MarketCap ?? 0,
                            crypto.Quote.Usd.MarketCapDominance ?? 0,
                            crypto.Quote.Usd.FullyDilutedMarketCap ?? 0,
                            crypto.Quote.Usd.LastUpdated != null ? DateTime.Parse(crypto.Quote.Usd.LastUpdated) : DateTime.MinValue
                        );
                    }
                    else
                    {
                        Console.WriteLine("Данные не найдены.");
                        return (null, 0, null, 0, 0, 0, 0, 0, 0, 0, 0, 0, DateTime.MinValue);
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Детали ошибки: {errorResponse}");
                    return (null, 0, null, 0, 0, 0, 0, 0, 0, 0, 0, 0, DateTime.MinValue);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Ошибка при запросе к API: {e.Message}");
                return (null, 0, null, 0, 0, 0, 0, 0, 0, 0, 0, 0, DateTime.MinValue);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Неизвестная ошибка: {e.Message}");
                return (null, 0, null, 0, 0, 0, 0, 0, 0, 0, 0, 0, DateTime.MinValue);
            }
        }

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
            public decimal CirculatingSupply { get; set; }

            [JsonProperty("market_cap_by_total_supply_strict")]
            public decimal MarketCapByTotalSupplyStrict { get; set; }

            [JsonProperty("last_updated")]
            public string LastUpdated { get; set; }

            [JsonProperty("percent_change_1h")]
            public decimal? PercentChange1h { get; set; }

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
            public decimal? Price { get; set; }

            [JsonProperty("volume_24h")]
            public decimal? Volume24h { get; set; }

            [JsonProperty("volume_change_24h")]
            public decimal? VolumeChange24h { get; set; }

            [JsonProperty("percent_change_1h")]
            public decimal? PercentChange1h { get; set; }

            [JsonProperty("percent_change_24h")]
            public decimal? PercentChange24h { get; set; }

            [JsonProperty("percent_change_7d")]
            public decimal? PercentChange7d { get; set; }

            [JsonProperty("market_cap")]
            public decimal? MarketCap { get; set; }

            [JsonProperty("market_cap_dominance")]
            public decimal? MarketCapDominance { get; set; }

            [JsonProperty("fully_diluted_market_cap")]
            public decimal? FullyDilutedMarketCap { get; set; }

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