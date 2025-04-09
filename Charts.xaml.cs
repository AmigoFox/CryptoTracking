using Npgsql;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
namespace CryptoTracking;
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
    private void OnCryptoSelected(object sender, SelectionChangedEventArgs e)
    {
        selectedCrypto = e.CurrentSelection.FirstOrDefault() as CryptoInfo;

        if (selectedCrypto != null)
        {
            SelectedCryptoFrame.IsVisible = true;
            SelectedCryptoName.Text = selectedCrypto.Name;
            SelectedCryptoSymbol.Text = selectedCrypto.Symbol;
            LoadCryptoDetails(selectedCrypto.Id);
        }
        Thread.Sleep(2000);
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
                              FROM quotescrypto";

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