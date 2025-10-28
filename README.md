ChatServer — C# TCP Çoklu İstemci Sohbet Sunucusu ve Konsol İstemcisi

Özet
WinForms tabanlı bir TCP Sohbet Sunucusu ve Konsol tabanlı bir Sohbet İstemcisi. Sunucu aynı anda birden fazla yazıcıyı kabul eder, iletilerin yayınları ve temel komutları desteklenir. Asenkron IO, thread-safe ürünlerinin yönetimi ve dosyaya log ile basit, anlaşılır bir referans projesi.

Özellikler

1.Çoklu gösteriler (TcpListener/TcpClient, asenkron/beklemede)
2.Komutlar: /nick, /list, /w, /quit
3.Bağlanma/ayrılma duyuruları
4.UTF-8 Türkçe karakter desteği
5.Dosyaya log: logs/server-YYYYMMDD.txt
6.İstikrar devam ediyor ve hata yönetimi

Mimari

Sunucu (WinForms):
1.TcpListener ile dinle; Her sonuç için ClientInfo tutar.
2.Komutlar; yayın ve özel dağıtım dağıtır.
3.Ağ işlemleri eşzamansız; UI güncellemeleri Invoke dosyası.
4.Loglar kenarı UI'da dosyada tutulur.
İstemci (Konsol):
1.Sunucuya örgü; satır tabanlı mesaj gönderilir/alır. İş mantığı sunucudadır.

Kurulum
Gereksinimler: 
1.Windows + .NET (VS 2019/2022 ile test)
2.Bu depoyu klonlayın veya ZIP indirin.
3.Visual Studio ile ChatServer.sln'yi açın. Ek bağımlılık yok.

Çalıştırma
1.ChatServer'ı başlangıç ​​projesini yapın → F5.
2.Port (örn. 5000) girin → Başlat.
3.ChatClient'e sağ tıklayın → Hata Ayıkla → Yeni örneği başlat (iki tanesi için göz önünde bulundurun).
4.Konsollardan mesaj yazıp Enter'a basın.

Komutlar
1./nick YeniAd — Takma reklam değişiklikleri olmalı (benzersiz olmalı).
2./list — Bağlı listeler.
3./w Kullanıcı Mesaj — Özel mesaj (yalnızca gönderen ve alırsınız).
4./quit — Kibarca ayrılır, duyuru yayınlanır.

Örnek Testi
1.İki çıktıyı açın, /nick ile isim verin (Ali, Ayşe).
2.Genel mesaj gönderin → iki ekranda de görünün.
3./w Ayşe Merhaba → yalnızca Ali ve Ayşe görsün.
4./list ile Kadınlardan görün; /quit ile ayrılmayı doğrulayın.

Loglar
1.Konum: ChatServer'ın yaptığı yerde logs/server-YYYYMMDD.txt
2.İçerik: Başlat/durdur, bağlanma/ayrılma, komut ve mesaj kayıtları.
3.Notlar ve Geliştirme
4.Takma adlar benzersizdir.
5.Standartların geliştirilmesi: odalar/kanallar, /help, kullanıcı sınırı, kick/ban, gelişmiş GUI'ler, Serilog/NLog dosyası arşivleme.
