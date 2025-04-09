using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoTracking
{
    internal class Quotes
    {
        string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=80085;Database=crypto_tracker";

        public async Task SaveCryptoDataAsyncQuotes(
            int id,
            decimal price,
            decimal volume_24h,
            decimal volume_change_24h,
            decimal percent_change_1h,
            decimal percent_change_24h,
            decimal percent_change_7d,
            decimal market_cap,
            decimal market_cap_dominance,
            decimal fully_diluted_market_cap)
        {
            string query = @"
        INSERT INTO quotescrypto (
            id, 
            price, 
            volume_24h, 
            volume_change_24h, 
            percent_change_1h, 
            percent_change_24h, 
            percent_change_7d, 
            market_cap, 
            market_cap_dominance, 
            fully_diluted_market_cap
        )
        VALUES (
            @id, 
            @price, 
            @volume_24h, 
            @volume_change_24h, 
            @percent_change_1h, 
            @percent_change_24h, 
            @percent_change_7d, 
            @market_cap, 
            @market_cap_dominance, 
            @fully_diluted_market_cap
        );";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.Parameters.AddWithValue("price", price);
                        command.Parameters.AddWithValue("volume_24h", volume_24h);
                        command.Parameters.AddWithValue("volume_change_24h", volume_change_24h);
                        command.Parameters.AddWithValue("percent_change_1h", percent_change_1h);
                        command.Parameters.AddWithValue("percent_change_24h", percent_change_24h);
                        command.Parameters.AddWithValue("percent_change_7d", percent_change_7d);
                        command.Parameters.AddWithValue("market_cap", market_cap);
                        command.Parameters.AddWithValue("market_cap_dominance", market_cap_dominance);
                        command.Parameters.AddWithValue("fully_diluted_market_cap", fully_diluted_market_cap);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                Console.WriteLine($"Ошибка PostgreSQL: {ex.Message}");
                Console.WriteLine($"Код ошибки: {ex.SqlState}");
                Console.WriteLine($"Таблица: {ex.TableName}");
                Console.WriteLine($"Колонка: {ex.ColumnName}");
                Console.WriteLine($"Ограничение: {ex.ConstraintName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
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