using Npgsql;
using System;
using System.Collections.Generic;

namespace CryptoTracking
{
    public partial class MainPage : ContentPage
    {
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
            await Task.Delay(2000);
            // Получаем кортеж из MainInfo
            var (name, price, volume24h, percentChange1h, percentChange24h, percentChange7d, lastUpdated) = await BackCrupto.MainInfo(request);

            // Обновляем интерфейс
            if (name != null)
            {
                Name.Text = name ?? "Не указано";
                Price.Text = (price.HasValue ? price.Value.ToString("F3") : "Нет данных") + " USD";
                PercentChange1h.Text = (percentChange1h.HasValue ? percentChange1h.Value.ToString("F3") : "Нет данных") + "%";
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось получить данные о криптовалюте", "OK");
                Name.Text = "Ошибка";
                Price.Text = "Ошибка";
                PercentChange1h.Text = "Ошибка";
            }

            // Данные доступны для построения таблиц
            // Например, можно сохранить их в список или использовать для отображения в таблице
            var dataForTable = new[]
            {
                new { Label = "Название", Value = name ?? "Нет данных" },
                new { Label = "Цена", Value = (price.HasValue ? price.Value.ToString("F3") + " USD" : "Нет данных") },
                new { Label = "Изменение 1ч", Value = (percentChange1h.HasValue ? percentChange1h.Value.ToString("F3") + "%" : "Нет данных") }
                // Добавьте другие поля по необходимости
            };
            // Здесь можно передать dataForTable в метод построения таблицы
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
        }
    }
}