using Npgsql;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
namespace CryptoTracking
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private static readonly Dictionary<string, string> CryptoMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "биткоин", "BTC" }, { "биткойн", "BTC" }, { "BTC", "BTC" }, { "биток", "BTC" }, { "битко", "BTC" }, { "эфир", "ETH" }, { "эфириум", "ETH" }, { "ETH", "ETH" }, { "эфирка", "ETH" }, { "эфирик", "ETH" }, { "доге", "DOGE" }, { "догикоин", "DOGE" }, { "DOGE", "DOGE" }, { "дожка", "DOGE" }, { "дог", "DOGE" }, { "лайткоин", "LTC" }, { "LTC", "LTC" }, { "лайт", "LTC" }, { "лайтик", "LTC" }, { "рипл", "XRP" }, { "XRP", "XRP" }, { "риплка", "XRP" }, { "кардано", "ADA" }, { "ADA", "ADA" }, { "ада", "ADA" }, { "полкадо", "DOT" }, { "DOT", "DOT" }, { "дот", "DOT" }, { "бинанс коин", "BNB" }, { "BNB", "BNB" }, { "бинанс", "BNB" }, { "солана", "SOL" }, { "SOL", "SOL" }, { "сол", "SOL" }, { "чейнлинк", "LINK" }, { "LINK", "LINK" }, { "линк", "LINK" }, { "аве", "AAVE" }, { "AAVE", "AAVE" }, { "ааве", "AAVE" }, { "юнисвап", "UNI" }, { "UNI", "UNI" }, { "юни", "UNI" }, { "тезер", "USDT" }, { "USDT", "USDT" }, { "тез", "USDT" }, { "USD Coin", "USDC" }, { "USDC", "USDC" }, { "юсдс", "USDC" }, { "биткоин кэш", "BCH" }, { "BCH", "BCH" }, { "биткоин кеш", "BCH" }, { "монеро", "XMR" }, { "XMR", "XMR" }, { "монерка", "XMR" }, { "тезос", "XTZ" }, { "XTZ", "XTZ" }, { "тезка", "XTZ" }, { "эос", "EOS" }, { "EOS", "EOS" }, { "еос", "EOS" }, { "трон", "TRX" }, { "TRX", "TRX" }, { "троник", "TRX" }, { "нео", "NEO" }, { "NEO", "NEO" }, { "вейв", "WAVES" }, { "WAVES", "WAVES" }, { "вейвс", "WAVES" }, { "стейлар", "XLM" }, { "XLM", "XLM" }, { "стеллар", "XLM" }, { "файлкоин", "FIL" }, { "FIL", "FIL" }, { "файл", "FIL" }, { "аваланч", "AVAX" }, { "AVAX", "AVAX" }, { "авакс", "AVAX" }, { "алгоритм", "ALGO" }, { "ALGO", "ALGO" }, { "алго", "ALGO" }, { "кусама", "KSM" }, { "KSM", "KSM" }, { "кусам", "KSM" }, { "козом", "ATOM" }, { "ATOM", "ATOM" }, { "атом", "ATOM" }, { "космос", "ATOM" }, { "матик", "MATIC" }, { "MATIC", "MATIC" }, { "полигон", "MATIC" }, { "фантом", "FTM" }, { "FTM", "FTM" }, { "фтом", "FTM" }, { "хедер", "HNT" }, { "HNT", "HNT" }, { "хеллум", "HNT" }, { "граф", "GRT" }, { "GRT", "GRT" }, { "грт", "GRT" }, { "ендж", "ENJ" }, { "ENJ", "ENJ" }, { "энж", "ENJ" }, { "санбокс", "SAND" }, { "SAND", "SAND" }, { "санд", "SAND" }, { "децентраленд", "MANA" }, { "MANA", "MANA" }, { "мана", "MANA" }, { "аксі", "AXS" }, { "AXS", "AXS" }, { "акси", "AXS" }, { "криптопанки", "PUNK" }, { "PUNK", "PUNK" }, { "панк", "PUNK" }, { "шиба", "SHIB" }, { "SHIB", "SHIB" }, { "шиба ину", "SHIB" }, { "бат", "BAT" }, { "BAT", "BAT" }, { "бэт", "BAT" }, { "компаунд", "COMP" }, { "COMP", "COMP" }, { "комп", "COMP" }, { "крв", "KAVA" }, { "KAVA", "KAVA" }, { "кава", "KAVA" }, { "зеро", "ZEC" }, { "ZEC", "ZEC" }, { "зек", "ZEC" }, { "даш", "DASH" }, { "DASH", "DASH" }, { "дэш", "DASH" }, { "рэн", "REN" }, { "REN", "REN" }, { "рен", "REN" }, { "окс", "OXT" }, { "OXT", "OXT" }, { "окст", "OXT" }, { "нум", "NU" }, { "NU", "NU" }, { "ну", "NU" }, { "рлб", "RLB" }, { "RLB", "RLB" }, { "рилб", "RLB" }, { "сусі", "SUSHI" }, { "SUSHI", "SUSHI" }, { "суши", "SUSHI" }, { "йфі", "YFI" }, { "YFI", "YFI" }, { "ифи", "YFI" }, { "бал", "BAL" }, { "BAL", "BAL" }, { "бэл", "BAL" }, { "мкр", "MKR" }, { "MKR", "MKR" }, { "мейкер", "MKR" }, { "снх", "SNX" }, { "SNX", "SNX" }, { "синтетикс", "SNX" }, { "уме", "UMA" }, { "UMA", "UMA" }, { "ума", "UMA" } };
        private string GetTicker(string input)
        {
            if (CryptoMap.TryGetValue(input.ToLower(), out string ticker))
            {
                return ticker;
            }
            return "Криптовалюта не найдена";
        }

        private async void GetInfo(object sender, EventArgs e)
        {
            string userInput = InputTextEntry.Text.Trim();
            InputTextEntry.Text = string.Empty;

            if (string.IsNullOrEmpty(userInput))
            {
                await DisplayAlert("Внимание", "Введите название криптовалюты", "OK");
                return;
            }

            string request = GetTicker(userInput);
            await DisplayAlert("Результат", $"Тикер: {request}", "OK");

            if (request == "Криптовалюта не найдена")
            {
                await DisplayAlert("Ошибка", "Криптовалюта не найдена", "OK");
                return;
            }
            var (name, id, symbol, price, volume24h, volumeChange24h, percentChange1h, percentChange24h, percentChange7d, marketCap, marketCapDominance, fullyDilutedMarketCap, lastUpdated) = await BackCrupto.MainInfo(request);

            NameLabel.Text = $"Название: {name}";
            PriceLabel.Text = $"Цена: {price.ToString("F9")} USD";
            PercentChange1hLabel.Text = $"Изменение за 1 час: {percentChange1h.ToString("F5")}%";
            PercentChange24hLabel.Text = $"Изменение за 24 часа: {percentChange24h.ToString("F5")}%";
            Volume24hLabel.Text = $"Объем торгов за 24 часа: {volume24h.ToString("F5")} USD";

            var tracker = new crypto_tracker();
            var quotes = new Quotes();

            try
            {
                await tracker.SaveCryptoDataAsync(id, name, symbol, lastUpdated = DateTime.Now);
            }
            catch (Npgsql.PostgresException ex)
            {
                Console.WriteLine($"Ошибка базы данных: {ex.Message}");
                Console.WriteLine($"Детали ошибки: {ex.Detail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }

            try
            {
                await quotes.SaveCryptoDataAsyncQuotes(id, price, volume24h, volumeChange24h, percentChange1h, percentChange24h, percentChange7d, marketCap, marketCapDominance, fullyDilutedMarketCap);
            }
            catch (Npgsql.PostgresException ex)
            {
                Console.WriteLine($"Ошибка PostgreSQL: {ex.Message}");
                Console.WriteLine($"Код ошибки: {ex.SqlState}");  // Это особенно важно!
                Console.WriteLine($"Таблица: {ex.TableName}");
                Console.WriteLine($"Колонка: {ex.ColumnName}");
                Console.WriteLine($"Ограничение: {ex.ConstraintName}");
                await DisplayAlert("Ошибка базы данных",
                 $"Не удалось сохранить данные:\n\n" +
                 $"Код ошибки: {ex.SqlState}\n" +
                 $"Таблица: {ex.TableName ?? "не указана"}\n" +
                 $"Ограничение: {ex.ConstraintName ?? "не указано"}",
                 "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                await DisplayAlert("Ошибка",
                    $"Произошла непредвиденная ошибка: {ex.Message}",
                    "OK");
            }
        }
    }
}