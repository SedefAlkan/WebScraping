# 📚 Google Akademik Web Scraping ve PDF İndirme Uygulaması

## Genel Bakış

Bu proje, **Google Akademik** üzerinden yapılan aramalar ile belirli makalelerin başlıklarını, yazar bilgilerini, alıntı sayısını ve yayın tarihlerini web kazıma (web scraping) tekniğiyle toplar. Veriler MongoDB veritabanına kaydedilir ve kullanıcı, web arayüzü aracılığıyla bu makalelere ulaşabilir. Ayrıca, makalelerin PDF bağlantıları da kazınarak kullanıcılara sağlanır. Kullanıcılar, makaleleri arayarak filtreleme yapabilir ve ilgili PDF dosyalarını indirebilir.

## Özellikler

- Google Akademik üzerinden anahtar kelimelerle arama yapılır.
- Makale başlıkları, yazarlar, alıntı sayısı ve yayın tarihi gibi veriler kazınır.
- Elde edilen veriler MongoDB veritabanına kaydedilir.
- Kullanıcı arayüzü ile sonuçlar filtrelenebilir (alıntı sayısı, yayın tarihi vb.).
- PDF linkleri veritabanına kaydedilir ve kullanıcılar bu PDF'lere erişebilir.
- Kullanıcılar arama sonuçlarını Excel formatında indirebilir.

## Kullanılan Teknolojiler

- **Programlama Dili:** C#
- **Web Kazıma Kütüphanesi:** HtmlAgilityPack
- **Veritabanı:** MongoDB
- **Veri Kaydetme:** MongoDB.Driver, LINQ
- **Diğer Kütüphaneler:** DinkToPdf (PDF oluşturma), HttpClient (veri çekme), Task (eş zamanlı işlem)
- **Arayüz:** ASP.NET Web Forms

## Gereksinimler

Aşağıdaki bağımlılıkların yüklü olduğundan emin olun:

```bash
pip install HtmlAgilityPack MongoDB.Driver DinkToPdf


