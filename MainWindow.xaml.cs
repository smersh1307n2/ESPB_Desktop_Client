using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using ESPB_Desktop_Client.Properties;

namespace ESPB_Desktop_Client
{
    public partial class MainWindow : Window
    {
        private string _apiKey = "";
        private readonly HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Загружаем настройки из ресурсов приложения
                _apiKey = Settings.Default.ApiKey ?? "";
                Settings.Default.BaseUrl = string.IsNullOrEmpty(Settings.Default.BaseUrl) 
                    ? "http://espb.runasp.net/api/v1" 
                    : Settings.Default.BaseUrl;
                
                if (!string.IsNullOrEmpty(_apiKey))
                    ApiKeyBox.Password = _apiKey;
                
                if (!string.IsNullOrEmpty(Settings.Default.OutputFile))
                    OutputFileTextBox.Text = Settings.Default.OutputFile;
                
                if (!string.IsNullOrEmpty(Settings.Default.BcFile))
                    BcFileTextBox.Text = Settings.Default.BcFile;
                
                if (!string.IsNullOrEmpty(Settings.Default.HintsFile))
                    HintsFileTextBox.Text = Settings.Default.HintsFile;
                
                if (!string.IsNullOrEmpty(Settings.Default.IdfFile))
                    IdfFileTextBox.Text = Settings.Default.IdfFile;
                
                if (!string.IsNullOrEmpty(Settings.Default.CustomFile))
                    CustomFileTextBox.Text = Settings.Default.CustomFile;
                
                Log("Settings loaded.");
            }
            catch (Exception ex) 
            { 
                Log($"Error loading settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Сохраняем настройки в ресурсы приложения
                Settings.Default.ApiKey = ApiKeyBox.Password;
                Settings.Default.OutputFile = OutputFileTextBox.Text;
                Settings.Default.BcFile = BcFileTextBox.Text;
                Settings.Default.HintsFile = HintsFileTextBox.Text;
                Settings.Default.IdfFile = IdfFileTextBox.Text;
                Settings.Default.CustomFile = CustomFileTextBox.Text;
                Settings.Default.BaseUrl = "http://espb.runasp.net/api/v1";
                
                Settings.Default.Save();
                Log("Settings saved.");
            }
            catch (Exception ex)
            {
                Log($"Error saving settings: {ex.Message}");
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.Clear();
                Log("Log cleared.");
            });
        }

        private void BrowseBc_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "BC Files (*.bc)|*.bc|All Files (*.*)|*.*",
                Title = "Select BC File"
            };
            
            // Если уже выбран файл, используем его папку
            if (!string.IsNullOrEmpty(BcFileTextBox.Text) && File.Exists(BcFileTextBox.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(BcFileTextBox.Text);
            }
            
            if (dialog.ShowDialog() == true)
            {
                BcFileTextBox.Text = dialog.FileName;
                // Автоматически сохраняем
                SaveSettings_Click(sender, e);
            }
        }

        private void BrowseHints_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Hints Files (*.hints)|*.hints|All Files (*.*)|*.*",
                Title = "Select Hints File"
            };
            
            // Если уже выбран файл, используем его папку
            if (!string.IsNullOrEmpty(HintsFileTextBox.Text) && File.Exists(HintsFileTextBox.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(HintsFileTextBox.Text);
            }
            
            if (dialog.ShowDialog() == true)
            {
                HintsFileTextBox.Text = dialog.FileName;
                // Автоматически сохраняем
                SaveSettings_Click(sender, e);
            }
        }

        private void BrowseIdf_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Symbol Files (*.sym)|*.sym|All Files (*.*)|*.*",
                Title = "Select IDF Symbol File"
            };
            
            // Если уже выбран файл, используем его папку
            if (!string.IsNullOrEmpty(IdfFileTextBox.Text) && File.Exists(IdfFileTextBox.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(IdfFileTextBox.Text);
            }
            
            if (dialog.ShowDialog() == true)
            {
                IdfFileTextBox.Text = dialog.FileName;
                // Автоматически сохраняем
                SaveSettings_Click(sender, e);
            }
        }

        private void BrowseCustom_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Symbol Files (*.sym)|*.sym|All Files (*.*)|*.*",
                Title = "Select Custom Symbol File"
            };
            
            // Если уже выбран файл, используем его папку
            if (!string.IsNullOrEmpty(CustomFileTextBox.Text) && File.Exists(CustomFileTextBox.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(CustomFileTextBox.Text);
            }
            
            if (dialog.ShowDialog() == true)
            {
                CustomFileTextBox.Text = dialog.FileName;
                // Автоматически сохраняем
                SaveSettings_Click(sender, e);
            }
        }

        private void BrowseOutputFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "ESPB Files (*.espb)|*.espb|All Files (*.*)|*.*",
                Title = "Select Output File",
                DefaultExt = ".espb"
            };
            
            // Если уже указан файл, используем его папку и имя
            if (!string.IsNullOrEmpty(OutputFileTextBox.Text) && File.Exists(OutputFileTextBox.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(OutputFileTextBox.Text);
                dialog.FileName = Path.GetFileName(OutputFileTextBox.Text);
            }
            else if (!string.IsNullOrEmpty(BcFileTextBox.Text) && File.Exists(BcFileTextBox.Text))
            {
                // Используем папку BC файла
                dialog.InitialDirectory = Path.GetDirectoryName(BcFileTextBox.Text);
                dialog.FileName = Path.GetFileNameWithoutExtension(BcFileTextBox.Text) + ".espb";
            }
            
            if (dialog.ShowDialog() == true)
            {
                OutputFileTextBox.Text = dialog.FileName;
                // Автоматически сохраняем
                SaveSettings_Click(sender, e);
            }
        }

        private async void GetLimits_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = ApiKeyBox.Password;
            }
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                Log("Please enter API key first.");
                return;
            }

            try
            {
                Log("Getting limits...");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{Settings.Default.BaseUrl}/limits");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    var remaining = result.GetProperty("remainingRequests").GetInt32();
                    var total = result.GetProperty("totalRequests").GetInt32();
                    var type = result.GetProperty("subscriptionType").GetString();

                    RemainingText.Text = remaining.ToString();
                    TotalText.Text = total.ToString();
                    Log($"Limits: {remaining}/{total} ({type})");
                }
                else
                {
                    Log($"Error: {response.StatusCode} - {content}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private async void Translate_Click(object sender, RoutedEventArgs e)
        {
            // Получаем API ключ из PasswordBox
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = ApiKeyBox.Password;
            }
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                Log("Please enter API key first.");
                return;
            }

            var bcFile = BcFileTextBox.Text;
            if (string.IsNullOrEmpty(bcFile) || !File.Exists(bcFile))
            {
                Log("Please select a BC file.");
                return;
            }

            try
            {
                TranslateBtn.IsEnabled = false;
                StatusText.Text = "Translating...";
                StatusText.Foreground = Brushes.Orange;
                Log("Starting translation...");

                // Читаем файлы и кодируем в base64
                var bcContent = await File.ReadAllBytesAsync(bcFile);
                var bcBase64 = Convert.ToBase64String(bcContent);

                string? hintsBase64 = null;
                if (!string.IsNullOrEmpty(HintsFileTextBox.Text) && File.Exists(HintsFileTextBox.Text))
                {
                    var hintsContent = await File.ReadAllBytesAsync(HintsFileTextBox.Text);
                    hintsBase64 = Convert.ToBase64String(hintsContent);
                }

                string? idfBase64 = null;
                if (!string.IsNullOrEmpty(IdfFileTextBox.Text) && File.Exists(IdfFileTextBox.Text))
                {
                    var idfContent = await File.ReadAllBytesAsync(IdfFileTextBox.Text);
                    idfBase64 = Convert.ToBase64String(idfContent);
                }

                string? customBase64 = null;
                if (!string.IsNullOrEmpty(CustomFileTextBox.Text) && File.Exists(CustomFileTextBox.Text))
                {
                    var customContent = await File.ReadAllBytesAsync(CustomFileTextBox.Text);
                    customBase64 = Convert.ToBase64String(customContent);
                }

                // Создаем запрос
                var requestObj = new
                {
                    fileName = Path.GetFileName(bcFile),
                    bcFileContent = bcBase64,
                    hintsFileContent = hintsBase64,
                    idxFileContent = idfBase64,
                    customFileContent = customBase64,
                    options = new
                    {
                        enableRegisterPatching = true,
                        enablePeephole = true,
                        enableLlvmOptimizations = true,
                        enableCallResultOptimizations = true,
                        enableAllocaOpt = true,
                        enableCompaction = true
                    }
                };

                var json = JsonSerializer.Serialize(requestObj);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{Settings.Default.BaseUrl}/translate")
                {
                    Content = httpContent
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                Log("Sending request to server...");
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<JsonElement>(content);
                var status = result.GetProperty("status").GetString();

                if (status == "success")
                {
                    var log = result.TryGetProperty("translationLog", out var logProp) ? logProp.GetString() : "";
                    var espbBase64 = result.GetProperty("espbFileContent").GetString();
                    var serverOutputName = result.GetProperty("outputFileName").GetString();
                    var remaining = result.GetProperty("remainingRequests").GetInt32();
                    var total = result.GetProperty("totalRequests").GetInt32();

                    Log($"Translation successful! Log:\n{log}");

                    // Сохраняем результат
                    if (!string.IsNullOrEmpty(espbBase64))
                    {
                        var espbBytes = Convert.FromBase64String(espbBase64);
                        
                        // Определяем путь для сохранения
                        string savePath;
                        
                        if (!string.IsNullOrEmpty(Settings.Default.OutputFile))
                        {
                            // Используем указанный пользователем файл
                            savePath = Settings.Default.OutputFile;
                        }
                        else if (!string.IsNullOrEmpty(OutputFileTextBox.Text))
                        {
                            // Используем файл из текстового поля
                            savePath = OutputFileTextBox.Text;
                        }
                        else
                        {
                            // Используем папку BC файла
                            string folder = Path.GetDirectoryName(bcFile) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            savePath = Path.Combine(folder, serverOutputName ?? "output.espb");
                        }
                        
                        // Перезаписываем файл
                        await File.WriteAllBytesAsync(savePath, espbBytes);
                        Log($"File saved: {savePath}");
                        
                        OutputInfoTextBox.Text = $"Saved: {savePath}";
                    }

                    RemainingText.Text = remaining.ToString();
                    TotalText.Text = total.ToString();
                    StatusText.Text = "Success";
                    StatusText.Foreground = Brushes.Green;
                }
                else
                {
                    var errorMsg = result.TryGetProperty("errorMessage", out var err) ? err.GetString() : "Unknown error";
                    Log($"Translation failed: {errorMsg}");
                    StatusText.Text = "Error";
                    StatusText.Foreground = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                StatusText.Text = "Error";
                StatusText.Foreground = Brushes.Red;
            }
            finally
            {
                TranslateBtn.IsEnabled = true;
            }
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }
    }
}
