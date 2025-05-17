using Npgsql;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
namespace CryptoTracking;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class Charts : ContentPage
{
    private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=80085;Database=crypto_tracker";
    private CryptoInfo selectedCrypto;
    public ObservableCollection<CryptoInfo> CryptoList { get; set; } = new ObservableCollection<CryptoInfo>();

    public Charts()
    {
        InitializeComponent();
        LoadCryptocurrencies();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCryptocurrencies();
    }

    private async void LoadCryptocurrencies()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            CryptoList.Clear();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string sql = "SELECT DISTINCT id, name, symbol, last_updated FROM cryptocurrencies;";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var cryptoInfo = new CryptoInfo
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Symbol = reader.GetString(2),
                            LastUpdated = reader.GetDateTime(3).ToString("g")
                        };

                        if (!CryptoList.Any(c => c.Id == cryptoInfo.Id))
                        {
                            CryptoList.Add(cryptoInfo);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить криптовалюты: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async Task<(decimal price, decimal volume24h, decimal volumeChange24h,
    decimal percentChange1h, decimal percentChange24h, decimal percentChange7d,
    decimal marketCap, decimal marketCapDominance, decimal fullyDilutedMarketCap)>
    GetCryptoDetailsFromDb(int cryptoId)
    {
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT price, volume_24h, volume_change_24h, 
                              percent_change_1h, percent_change_24h, percent_change_7d, 
                              market_cap, market_cap_dominance, fully_diluted_market_cap 
                              FROM quotescrypto WHERE id = @id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", cryptoId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return (
                                reader.GetDecimal(0),
                                reader.GetDecimal(1),
                                reader.GetDecimal(2),
                                reader.GetDecimal(3),
                                reader.GetDecimal(4),
                                reader.GetDecimal(5),
                                reader.GetDecimal(6),
                                reader.GetDecimal(7),
                                reader.GetDecimal(8)
                            );
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить детали: {ex.Message}", "OK");
        }
        return (0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
    private async Task LoadCryptoDetails(int cryptoId)
    {
        try
        {
            var (price, volume24h, volumeChange24h, percentChange1h,
                percentChange24h, percentChange7d, marketCap,
                marketCapDominance, fullyDilutedMarketCap) = await GetCryptoDetailsFromDb(cryptoId);

            var cryptoInfo = CryptoList.FirstOrDefault(c => c.Id == cryptoId);
            if (cryptoInfo != null)
            {
                cryptoInfo.Price = price;
                cryptoInfo.Volume24h = volume24h;
                cryptoInfo.VolumeChange24h = volumeChange24h;
                cryptoInfo.PercentChange1h = percentChange1h;
                cryptoInfo.PercentChange24h = percentChange24h;
                cryptoInfo.PercentChange7d = percentChange7d;
                cryptoInfo.MarketCap = marketCap;
                cryptoInfo.MarketCapDominance = marketCapDominance;
                cryptoInfo.FullyDilutedMarketCap = fullyDilutedMarketCap;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить детали: {ex.Message}", "OK");
        }
    }
    private async Task<List<(DateTime time, decimal price)>> GetPriceHistory(int cryptoId)
    {
        var history = new List<(DateTime, decimal)>();
        var seenTimestamps = new HashSet<DateTime>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        const string sql = @"
        SELECT time_stamp, price
        FROM quotescrypto
        WHERE id = @id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", cryptoId);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var time = reader.GetDateTime(0);
            var price = reader.GetDecimal(1);
            int minutes = time.Minute >= 30 ? 30 : 0;
            var roundedTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, minutes, 0);
            if (!seenTimestamps.Contains(roundedTime))
            {
                history.Add((roundedTime, price));
                seenTimestamps.Add(roundedTime);
            }

        }
        return history;
    }





    private async void OnCryptoSelected(object sender, SelectionChangedEventArgs e)
    {
        selectedCrypto = e.CurrentSelection.FirstOrDefault() as CryptoInfo;
        if (selectedCrypto == null) return;

        await LoadCryptoDetails(selectedCrypto.Id);
        SelectedCryptoFrame.IsVisible = true;
        SelectedCryptoName.Text = selectedCrypto.Name;
        SelectedCryptoSymbol.Text = selectedCrypto.Symbol;

        try
        {
            var PriceHistory = await GetPriceHistory(selectedCrypto.Id);
            var labels = PriceHistory.Select(h => h.time.ToString("yyyy-MM-dd HH:mm")).ToArray();
            var price = PriceHistory.Select(h => decimal.ToDouble(h.price)).ToArray();

            // 1) Строим конфиг как строку, чтобы сохранить JS-функцию formatter
            string chartConfigPrice = $@"{{
              type: 'line',
              data: {{
                labels: [{string.Join(",", labels.Select(l => $"'{l}'"))}],
                datasets: [{{
                  label: 'Цена USD',
                  data: [{string.Join(",", price)}],
                  borderColor: '#00FF5E',
                  fill: false
                }}]
              }},
              options: {{
                responsive: true,
                plugins: {{
                  legend: {{ display: false }},
                  datalabels: {{
                    display: true,
                    anchor: 'end',
                    align: 'top',
                    formatter: function(value) {{ return (value.toFixed(6)); }},
                    font: {{ weight: 'bold', size: 12 }},
                    color: '#000',
                    padding: 4
                  }}
                }},
                scales: {{
                  x: {{ display: true }},
                  y: {{ display: true }}
                }}
              }}
            }}";


            var payload = new
            {
                chart = chartConfigPrice,
                width = 1000,
                height = 800,
                devicePixelRatio = 1.0,
                format = "png"
            };

            string json = JsonSerializer.Serialize(payload);
            using var http = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.PostAsync("https://quickchart.io/chart/create", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка", $"HTTP {response.StatusCode}\n{body}", "OK");
                return;
            }

            var respObj = JsonSerializer.Deserialize<QuickChartResponse>(body);
            if (string.IsNullOrEmpty(respObj?.Url))
            {
                await DisplayAlert("Ошибка", $"Пустой URL в ответе:\n{body}", "OK");
                return;
            }

            var imageBytes = await http.GetByteArrayAsync(respObj.Url);
            ChartImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            ChartImage.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    public class QuickChartResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
public partial class CryptoInfo : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _symbol;

    [ObservableProperty]
    private string _lastUpdated;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private decimal _volume24h;

    [ObservableProperty]
    private decimal _volumeChange24h;

    [ObservableProperty]
    private decimal _percentChange1h;

    [ObservableProperty]
    private decimal _percentChange24h;

    [ObservableProperty]
    private decimal _percentChange7d;

    [ObservableProperty]
    private decimal _marketCap;

    [ObservableProperty]
    private decimal _marketCapDominance;

    [ObservableProperty]
    private decimal _fullyDilutedMarketCap;
}