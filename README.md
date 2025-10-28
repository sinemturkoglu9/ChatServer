ChatServer — C# TCP Çoklu İstemci Sohbet Sunucusu ve Konsol İstemcisi

Özet
WinForms tabanlı bir TCP Sohbet Sunucusu ve Konsol tabanlı bir Sohbet İstemcisi. Sunucu aynı anda birden fazla yazıcıyı kabul eder, iletilerin yayınları ve temel komutları desteklenir. Asenkron IO, thread-safe ürünlerinin yönetimi ve dosyaya log ile basit, anlaşılır bir referans projesi.

Özellikler

-Çoklu gösteriler (TcpListener/TcpClient, asenkron/beklemede)
-Komutlar: /nick, /list, /w, /quit
-Bağlanma/ayrılma duyuruları
-UTF-8 Türkçe karakter desteği
-Dosyaya log: logs/server-YYYYMMDD.txt
-İstikrar devam ediyor ve hata yönetimi

Mimari

Sunucu (WinForms):
-TcpListener ile dinle; Her sonuç için ClientInfo tutar.
-Komutlar; yayın ve özel dağıtım dağıtır.
-Ağ işlemleri eşzamansız; UI güncellemeleri Invoke dosyası.
-Loglar kenarı UI'da dosyada tutulur.
İstemci (Konsol):
-Sunucuya örgü; satır tabanlı mesaj gönderilir/alır. İş mantığı sunucudadır.

Kurulum
Gereksinimler: 
-Windows + .NET (VS 2019/2022 ile test)
-Bu depoyu klonlayın veya ZIP indirin.
-Visual Studio ile ChatServer.sln'yi açın. Ek bağımlılık yok.

Çalıştırma
1.ChatServer'ı başlangıç ​​projesini yapın → F5.
2.Port (örn. 5000) girin → Başlat.
3.ChatClient'e sağ tıklayın → Hata Ayıkla → Yeni örneği başlat (iki tanesi için göz önünde bulundurun).
4.Konsollardan mesaj yazıp Enter'a basın.

Komutlar
-/nick YeniAd — Takma reklam değişiklikleri olmalı (benzersiz olmalı).
-/list — Bağlı listeler.
-/w Kullanıcı Mesaj — Özel mesaj (yalnızca gönderen ve alırsınız).
-/quit — Kibarca ayrılır, duyuru yayınlanır.

Örnek Testi
1.İki çıktıyı açın, /nick ile isim verin (Ali, Ayşe).
2.Genel mesaj gönderin → iki ekranda de görünün.
3./w Ayşe Merhaba → yalnızca Ali ve Ayşe görsün.
4./list ile Kadınlardan görün; /quit ile ayrılmayı doğrulayın.

Loglar
-Konum: ChatServer'ın yaptığı yerde logs/server-YYYYMMDD.txt
-İçerik: Başlat/durdur, bağlanma/ayrılma, komut ve mesaj kayıtları.
-Notlar ve Geliştirme
-Takma adlar benzersizdir.
-Standartların geliştirilmesi: odalar/kanallar, /help, kullanıcı sınırı, kick/ban, gelişmiş GUI'ler, Serilog/NLog dosyası arşivleme.
