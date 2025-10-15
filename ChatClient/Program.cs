using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string host = "127.0.0.1";
        int port = 5000;
        if (args.Length >= 1) host = args[0];
        if (args.Length >= 2 && int.TryParse(args[1], out int p)) port = p;

        Console.WriteLine($"Bağlanılıyor: {host}:{port} ...");
        TcpClient client = new TcpClient();
        await client.ConnectAsync(host, port);
        Console.WriteLine("Sunucuya bağlanıldı. Mesaj yazıp Enter'a basın. Çıkış: /quit");

        CancellationTokenSource cts = new CancellationTokenSource();

        Task receiveTask = Task.Run(async () =>
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                    if (read == 0) break;
                    string incoming = Encoding.UTF8.GetString(buffer, 0, read);
                    Console.Write(incoming);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Hata - alım]: {ex.Message}");
            }
        });

        try
        {
            NetworkStream stream = client.GetStream();
            while (true)
            {
                string line = Console.ReadLine();
                if (line == null) break;

                byte[] data = Encoding.UTF8.GetBytes(line + "\n");
                await stream.WriteAsync(data, 0, data.Length);

                if (line.Equals("/quit", StringComparison.OrdinalIgnoreCase))
                    break;
            }
        }
        finally
        {
            cts.Cancel();
            Console.WriteLine("Bağlantı kapatılıyor...");
            client.Close();
        }
    }
}
