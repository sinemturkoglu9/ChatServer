# ChatServer — C# TCP Çoklu İstemci Sohbet Sunucusu ve Konsol İstemcisi

## Özet
WinForms tabanlı bir TCP Sohbet Sunucusu ve Konsol tabanlı bir Sohbet İstemcisi.  
Sunucu aynı anda birden fazla istemciyi kabul eder, iletileri yayınlar ve temel komutları destekler.  
Asenkron IO, thread-safe istemci yönetimi ve dosyaya log özelliği ile basit bir örnek projedir.

## Özellikler
- Çoklu istemci desteği (TcpListener/TcpClient)
- Komutlar: /nick, /list, /w, /quit
- Bağlanma/ayrılma duyuruları
- UTF-8 Türkçe karakter desteği
- Dosyaya log: `logs/server-YYYYMMDD.txt`
- Temel hata yönetimi

## Mimari
**Sunucu (WinForms):** TcpListener ile istemcileri dinler, her istemci için ClientInfo tutar. Komutları işler, yayın ve özel mesaj gönderir.  
**İstemci (Konsol):** Sunucuya bağlanır, satır tabanlı mesaj gönderir/alır. İş mantığı sunucudadır.

## Kurulum
- Windows + .NET (VS 2019/2022 ile test edilmiştir)  
- Depoyu klonlayın veya ZIP olarak indirin.  
- Visual Studio ile `ChatServer.sln` dosyasını açın.  

## Çalıştırma
1. `ChatServer`’ı başlangıç projesi yapın ve F5 ile çalıştırın.  
2. Port (örn. 5000) girin ve **Başlat**’a tıklayın.  
3. `ChatClient` projesini ayrı çalıştırarak birden fazla istemci açın.  
4. Konsoldan mesaj göndererek test edin.

## Komutlar
- `/nick YeniAd` — Kullanıcı adını değiştirir.  
- `/list` — Bağlı kullanıcıları listeler.  
- `/w Kullanıcı Mesaj` — Özel mesaj gönderir.  
- `/quit` — Oturumu sonlandırır.  

## Örnek Test
1. İki istemci açın, `/nick` ile isim verin.  
2. Genel mesaj gönderin → iki konsolda da görünür.  
3. `/w Ayşe Merhaba` → yalnızca Ali ve Ayşe görür.  
4. `/list` → bağlı kullanıcıları gösterir.  
5. `/quit` → ayrılma duyurusu gönderilir.

## Loglar
- Kayıtlar `logs/server-YYYYMMDD.txt` içinde tutulur.  
- İçerik: bağlantılar, komutlar, mesajlar.  

## Notlar
- Takma adlar benzersizdir.  
- Geliştirilebilir: /help, kanallar, kullanıcı sınırı, GUI geliştirmeleri.


