using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoTracking
{
    internal class crypto_tracker
    {
        string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=80085;Database=crypto_tracker";
        public async Task SaveCryptoDataAsync(int id, string name, string symbol, DateTime lastUpdated)
        {
            
            string query = @"
        INSERT INTO cryptocurrencies (id, name, symbol, last_updated)
        VALUES (@id, @name, @symbol, @lastUpdated)";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("name", name);
                    command.Parameters.AddWithValue("symbol", symbol);
                    command.Parameters.AddWithValue("lastUpdated", lastUpdated);

                    await command.ExecuteNonQueryAsync();
                }
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
            public decimal PercentChange1h { get; set; }

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
            public decimal Price { get; set; }

            [JsonProperty("volume_24h")]
            public decimal Volume24h { get; set; }

            [JsonProperty("volume_change_24h")]
            public decimal VolumeChange24h { get; set; }

            [JsonProperty("percent_change_1h")]
            public decimal PercentChange1h { get; set; }

            [JsonProperty("percent_change_24h")]
            public decimal PercentChange24h { get; set; }

            [JsonProperty("percent_change_7d")]
            public decimal PercentChange7d { get; set; }

            [JsonProperty("market_cap")]
            public decimal MarketCap { get; set; }

            [JsonProperty("market_cap_dominance")]
            public decimal MarketCapDominance { get; set; }

            [JsonProperty("fully_diluted_market_cap")]
            public decimal FullyDilutedMarketCap { get; set; }

            [JsonProperty("last_updated")]
            public string LastUpdated { get; set; }
        }
    }
}