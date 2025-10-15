using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        // Sunucu alanları
        private TcpListener _listener;
        private CancellationTokenSource _cts;

        // Aktif istemciler (bilgileriyle)
        private readonly ConcurrentDictionary<TcpClient, ClientInfo> _clients =
            new ConcurrentDictionary<TcpClient, ClientInfo>();

        // Varsayılan takma ad üretimi (User1, User2, ...)
        private int _userCounter = 0;

        // Dosya log için
        private readonly object _fileLogLock = new object();
        private string _logFilePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            string line = $"{DateTime.Now:HH:mm:ss} {message}";
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(message)));
                return;
            }
            txtLog.AppendText(line + Environment.NewLine);
            FileAppendLine(line);
        }

        private void FileAppendLine(string line)
        {
            try
            {
                if (string.IsNullOrEmpty(_logFilePath)) return;
                lock (_fileLogLock)
                {
                    System.IO.File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Dosya erişim hatalarını sessizce yoksay
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port;
                if (!int.TryParse(txtPort.Text, out port) || port < 1 || port > 65535)
                {
                    MessageBox.Show("Geçerli bir port numarası girin (1-65535).");
                    return;
                }

                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();

                // Log dosyası hazırlığı
                var logsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                System.IO.Directory.CreateDirectory(logsDir);
                _logFilePath = System.IO.Path.Combine(logsDir, $"server-{DateTime.Now:yyyyMMdd}.txt");
                FileAppendLine($"==== Sunucu başlatıldı: {DateTime.Now:O} Port: {port} ====");

                Log($"Sunucu başlatıldı. Port: {port}");
                btnStart.Enabled = false;
                btnStop.Enabled = true;

                // UI kilitlenmesin: fire-and-forget
                var _ = AcceptLoopAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                Log($"Hata (başlat): {ex.Message}");
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cts != null) _cts.Cancel();
                if (_listener != null) _listener.Stop();

                foreach (var kv in _clients.Keys.ToList())
                {
                    try { kv.Close(); } catch { }
                }
                _clients.Clear();

                FileAppendLine($"==== Sunucu durduruldu: {DateTime.Now:O} ====");
                Log("Sunucu durduruldu.");
            }
            catch (Exception ex)
            {
                Log($"Hata (durdur): {ex.Message}");
            }
            finally
            {
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        // Yeni bağlantıları kabul
        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = null;
                try
                {
                    client = await _listener.AcceptTcpClientAsync();
                    if (token.IsCancellationRequested) break;

                    var ep = client.Client.RemoteEndPoint != null ? client.Client.RemoteEndPoint.ToString() : "client";
                    var info = new ClientInfo
                    {
                        Client = client,
                        Stream = client.GetStream(),
                        EndPoint = ep,
                        Nickname = $"User{Interlocked.Increment(ref _userCounter)}"
                    };

                    _clients[client] = info;

                    Log($"Yeni bağlantı: {info.EndPoint} ({info.Nickname})");

                    // Hoş geldin + komutlar
                    await SafeSendAsync(info,
                        "[Sunucu] Sunucuya bağlandınız. " +
                        $"Takma adınız: {info.Nickname}\n" +
                        "Komutlar: /nick YeniAd, /list, /w Kullanıcı Mesaj, /quit\n");
                    await BroadcastAsync($"[Sunucu] {info.Nickname} bağlandı.\n");

                    // İstemciyi arka planda işle
                    var _ = HandleClientLifecycleAsync(info, token);
                }
                catch (ObjectDisposedException)
                {
                    break; // Listener durdu
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                        Log($"Hata (accept): {ex.Message}");

                    if (client != null)
                        try { client.Close(); } catch { }
                }
            }
        }

        // İstemci yaşam döngüsü: oku → komutları işle → yayınla
        private async Task HandleClientLifecycleAsync(ClientInfo info, CancellationToken token)
        {
            try
            {
                var stream = info.Stream;
                var buffer = new byte[4096];

                while (!token.IsCancellationRequested)
                {
                    int read;
                    try
                    {
                        read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch (OperationCanceledException)
                    {
                        break; // iptal
                    }
                    catch (ObjectDisposedException)
                    {
                        break; // akış kapalı
                    }
                    catch (SocketException)
                    {
                        break; // ağ kapandı
                    }
                    catch (Exception exRead)
                    {
                        Log($"Okuma hatası ({info.Nickname}): {exRead.Message}");
                        break;
                    }

                    if (read == 0) break; // uzak taraf kapattı

                    var msgRaw = Encoding.UTF8.GetString(buffer, 0, read);
                    var msg = msgRaw.Trim();
                    if (string.IsNullOrWhiteSpace(msg))
                        continue;

                    Log($"[{info.Nickname}] {msg}");

                    // /quit
                    if (msg.Equals("/quit", StringComparison.OrdinalIgnoreCase))
                    {
                        await SafeSendAsync(info, "[Sunucu] Görüşmek üzere.\n");
                        break;
                    }

                    // /nick YeniAd
                    if (msg.StartsWith("/nick ", StringComparison.OrdinalIgnoreCase))
                    {
                        var newNick = msg.Substring(6).Trim();
                        if (string.IsNullOrWhiteSpace(newNick))
                        {
                            await SafeSendAsync(info, "[Sunucu] Kullanım: /nick YeniAd\n");
                        }
                        else
                        {
                            bool exists = _clients.Values.Any(c =>
                                !object.ReferenceEquals(c, info) &&
                                c.Nickname.Equals(newNick, StringComparison.OrdinalIgnoreCase));

                            if (exists)
                            {
                                await SafeSendAsync(info, "[Sunucu] Bu takma ad kullanımda. Başka bir ad deneyin.\n");
                            }
                            else
                            {
                                var old = info.Nickname;
                                info.Nickname = newNick;
                                await SafeSendAsync(info, $"[Sunucu] Takma adınız '{info.Nickname}' olarak ayarlandı.\n");
                                await BroadcastAsync($"[Sunucu] {old} adını {info.Nickname} olarak değiştirdi.\n");
                            }
                        }
                        continue;
                    }

                    // /list
                    if (msg.Equals("/list", StringComparison.OrdinalIgnoreCase))
                    {
                        var names = _clients.Values.Select(c => c.Nickname).OrderBy(n => n).ToArray();
                        var line = names.Length > 0 ? string.Join(", ", names) : "(hiç yok)";
                        await SafeSendAsync(info, $"[Sunucu] Bağlı: {line}\n");
                        continue;
                    }

                    // /w HedefKullanıcı mesaj
                    if (msg.StartsWith("/w ", StringComparison.OrdinalIgnoreCase) ||
                        msg.StartsWith("/whisper ", StringComparison.OrdinalIgnoreCase) ||
                        msg.StartsWith("/pm ", StringComparison.OrdinalIgnoreCase))
                    {
                        string payload = msg.Contains(' ') ? msg.Substring(msg.IndexOf(' ') + 1).Trim() : "";
                        int space = payload.IndexOf(' ');
                        if (space <= 0)
                        {
                            await SafeSendAsync(info, "[Sunucu] Kullanım: /w HedefKullanıcı Mesaj\n");
                            continue;
                        }

                        string targetName = payload.Substring(0, space).Trim();
                        string privateMsg = payload.Substring(space + 1).Trim();
                        if (string.IsNullOrWhiteSpace(privateMsg))
                        {
                            await SafeSendAsync(info, "[Sunucu] Kullanım: /w HedefKullanıcı Mesaj\n");
                            continue;
                        }

                        var target = FindByNickname(targetName);
                        if (target == null)
                        {
                            await SafeSendAsync(info, $"[Sunucu] Kullanıcı bulunamadı: {targetName}\n");
                            continue;
                        }

                        await SafeSendAsync(target, $"[Özel] {info.Nickname}: {privateMsg}\n");
                        await SafeSendAsync(info, $"[Özel] ({info.Nickname} → {target.Nickname}): {privateMsg}\n");
                        continue;
                    }

                    // Normal mesaj → herkese
                    await BroadcastAsync($"[{info.Nickname}] {msg}\n");
                }
            }
            catch (Exception ex)
            {
                Log($"Hata (client {info.Nickname}): {ex.Message}");
            }
            finally
            {
                // Ayrılma
                try
                {
                    ClientInfo removed;
                    _clients.TryRemove(info.Client, out removed);
                    try { info.Client.Close(); } catch { }
                }
                catch { }

                await BroadcastAsync($"[Sunucu] {info.Nickname} ayrıldı.\n");
                Log($"Bağlantı kapandı: {info.EndPoint} ({info.Nickname})");
            }
        }

        // Yardımcılar
        private ClientInfo FindByNickname(string nickname)
        {
            return _clients.Values.FirstOrDefault(c =>
                c.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase));
        }

        private async Task SafeSendAsync(ClientInfo info, string text)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(text);
                await info.Stream.WriteAsync(data, 0, data.Length);
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            catch (SocketException) { }
            catch { }
        }

        private async Task BroadcastAsync(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            var tasks = new List<Task>();

            foreach (var kv in _clients.Values.ToList())
            {
                try
                {
                    tasks.Add(kv.Stream.WriteAsync(data, 0, data.Length));
                }
                catch (ObjectDisposedException) { }
                catch (OperationCanceledException) { }
                catch (SocketException) { }
                catch { }
            }

            try { await Task.WhenAll(tasks); } catch { }
        }

        private void txtPort_TextChanged(object sender, EventArgs e) { }
    }

    // İstemci bilgisi
    class ClientInfo
    {
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public string EndPoint { get; set; }
        public string Nickname { get; set; }
    }
}