using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using DinkToPdf;
using MongoDB.Bson;
using MongoDB.Driver;

namespace yazlabproje1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private int maxPdfCount = 10;
        private List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)> yeniMakaleler = new List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)>();

        protected void Page_Load(object sender, EventArgs e)
        {
            
            yeniMakaleleriDoldur();
        }
        private async void yeniMakaleleriDoldur()
        {
            
            makaleListesi.InnerHtml = "";
            var aramaMetni = TextBox1.Text;
            var makaleler = await GoogleAkademikAraAsync(aramaMetni);

            yeniMakaleler = KaydetVeritabaniya(makaleler);
            var pdfLinks = GetPdfLinksFromDatabase(yeniMakaleler);

            var pdfContents = await Task.WhenAll(pdfLinks.Select(pdfLink => FetchPdfContentAsync(pdfLink.Link, pdfLink.Title)));

            foreach (var pdfLink in yeniMakaleler.Take(maxPdfCount))
            {
                var makaleHtml = $"<li class='makale'><a href='{pdfLink.Link}' target='_blank'>{pdfLink.Title}</a></li>";
                makaleHtml += $"<div class='ozet'>{pdfLink.ozet}</div>";
                makaleHtml += $"<div class='AlintiSayisi'>Alıntı Sayısı: <span class='alinti'>{pdfLink.AlintiSayisi}</span> | Yayıncı: <span class='yayinci'>{pdfLink.yayinci}</span> | Yazar: <span class='yazar'>{pdfLink.Author}</span> | Yayınlanma Tarihi: <span class='publishedDate'>{pdfLink.publishedDate}</span></div>"; 
                makaleHtml += "<hr>";
                makaleListesi.InnerHtml += makaleHtml;
            }
        }
        protected async void btnAra_Click(object sender, EventArgs e)
        {
           

            makaleListesi.InnerHtml = "";
            var aramaMetni = TextBox1.Text;
            var makaleler = await GoogleAkademikAraAsync(aramaMetni);

            var yeniMakaleler = KaydetVeritabaniya(makaleler);
            var pdfLinks = GetPdfLinksFromDatabase(yeniMakaleler);

            var pdfContents = await Task.WhenAll(pdfLinks.Select(pdfLink => FetchPdfContentAsync(pdfLink.Link, pdfLink.Title)));

            foreach (var pdfLink in yeniMakaleler.Take(maxPdfCount))
            {
                var makaleHtml = $"<li class='makale'><a href='{pdfLink.Link}' target='_blank'>{pdfLink.Title}</a></li>";
                makaleHtml += $"<div class='ozet'>{pdfLink.ozet}</div>"; 
                
                makaleHtml += $"<div class='AlintiSayisi'>Alıntı Sayısı: <span class='alinti'>{pdfLink.AlintiSayisi}</span> | Yayıncı: <span class='yayinci'>{pdfLink.yayinci}</span> | Yazar: <span class='yazar'>{pdfLink.Author}</span> | Yayınlanma Tarihi: <span class='publishedDate'>{pdfLink.publishedDate}</span></div>"; 
                makaleHtml += "<hr>";
                makaleListesi.InnerHtml += makaleHtml;
            }



            await InsertPdfLinksToDatabase(yeniMakaleler, aramaMetni);

         
        }

        



        private async Task InsertPdfLinksToDatabase(List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)> pdfLinks, string aramaMetni)
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("yazlab2");
            var collection = database.GetCollection<BsonDocument>("yazlabweb2");

            var filterBuilder = Builders<BsonDocument>.Filter;
            var existingLinksFilter = filterBuilder.In("Link", pdfLinks.Select(link => link.Link));
            var existingLinks = await collection.Find(existingLinksFilter).ToListAsync();

            var newPdfLinks = pdfLinks
                .Where(link => !existingLinks.Any(existingLink => existingLink["Link"].AsString == link.Link))
                .Take(maxPdfCount)
                .ToList();

            foreach (var pdfLink in newPdfLinks)
            {
               
                var authorParts = pdfLink.Author.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);

                var document = new BsonDocument
        {
            { "Makale ismi", pdfLink.Title },
            { "Makale linki", pdfLink.Link },
            { "Yazar", pdfLink.Author },
            {"Yayın turu", "PDF"  },
            {"Ozet",pdfLink.ozet },
            {"Aranan Kelime", aramaMetni },
             {"Alıntılanma Sayısı",pdfLink.AlintiSayisi},
             {"Yayıncı",pdfLink.yayinci },
              {"YayınID",pdfLink.yayinID },
             {"yayınlanma tarihi",pdfLink.publishedDate }
        };

                try
                {
                    await collection.InsertOneAsync(document);
                    Console.WriteLine($"Veritabanına eklendi: {pdfLink.Title} - {pdfLink.Link} - {pdfLink.Author} ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }
        }

        private List<(string Title, string Link)> GetPdfLinksFromDatabase(List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)> yeniMakaleler)
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("yazlab");
            var collection = database.GetCollection<BsonDocument>("yazlabweb");

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Regex("Link", new BsonRegularExpression(@".pdf$")),
                filterBuilder.In("Title", yeniMakaleler.Select(m => m.Title))
            );

            var pdfLinks = collection.Find(filter).ToList();
            var linksList = pdfLinks.Select(doc => (doc["Title"].AsString, doc["Link"].AsString)).ToList();

            return linksList;
        }

        private List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)> KaydetVeritabaniya(List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)> makaleler)
        {
            var yeniMakaleler = new List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)>();

            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("yazlab");
            var collection = database.GetCollection<BsonDocument>("yazlabweb");

            foreach (var makale in makaleler)
            {
                var document = new BsonDocument
                {
                    { "Makale ismi", makale.Title },
                    { "Link", makale.Link },
                    { "Yazar", makale.Author }
                };

                try
                {
                    collection.InsertOne(document);
                    Console.WriteLine($"Veritabanına eklendi: {makale.Title} - {makale.Link} - {makale.Author}");
                    yeniMakaleler.Add(makale);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }

            return yeniMakaleler;
        }

        private async Task<List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci,string yayinID, string publishedDate)>> GoogleAkademikAraAsync(string aramaKelimesi, int sayfaSayisi = 3)
        {
            var makaleler = new List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)>();
            var tasks = new List<Task<List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)>>>();

            for (int sayfa = 0; sayfa < sayfaSayisi; sayfa++)
            {
                tasks.Add(FetchPageContentAsync(aramaKelimesi, sayfa));
            }

            await Task.WhenAll(tasks);

            foreach (var result in tasks.SelectMany(taskResult => taskResult.Result))
            {
                makaleler.Add(result);
            }

            return makaleler;
        }
        private string DecodeHtmlCharacters(string input)
        {
            return WebUtility.HtmlDecode(input);
        }

        private async Task<List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci,string yayinID,string publishedDate)>> FetchPageContentAsync(string aramaKelimesi, int sayfa)
        {
            var googleAkademikUrl = $"https://scholar.google.com/scholar?q={WebUtility.UrlEncode(aramaKelimesi)}&start={sayfa * 10 + 1}";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var htmlContent = await httpClient.GetStringAsync(googleAkademikUrl);
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlContent);

                    var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='gs_ri']");

                    if (nodes != null)
                    {
                        var pdfLinks = nodes.Select(node =>
                        {
                            var titleNode = node.SelectSingleNode(".//h3[@class='gs_rt']/a");
                            var title = titleNode != null ? DecodeHtmlCharacters(titleNode.InnerText.Trim()) : "";

                            var link = titleNode?.GetAttributeValue("href", "") ?? "";

                            var authorNode = node.SelectSingleNode(".//div[@class='gs_a']");
                            var author = authorNode != null ? DecodeHtmlCharacters(authorNode.InnerText.Trim()) : "";

                            // Yazar adını '-' karakterini gördüğün yere kadar al
                            var endIndex = author.IndexOf('-');
                            if (endIndex != -1)
                            {
                                author = author.Substring(0, endIndex).Trim();
                            }

                            var ozetNode = node.SelectSingleNode(".//div[@class='gs_rs']");
                            var ozet = ozetNode != null ? DecodeHtmlCharacters(ozetNode.InnerText.Trim()) : "";

                          
                            var alintiNode = node.SelectSingleNode(".//a[contains(@href,'cites')]");
                            var alintiSayisi = 0;
                            if (alintiNode != null)
                            {
                                var alintiText = alintiNode.InnerText.Trim();
                                var alintiIndex = alintiText.LastIndexOf(":");
                                if (alintiIndex != -1)
                                {
                                    var alintiPart = alintiText.Substring(alintiIndex + 1).Trim();
                                    int.TryParse(alintiPart, out alintiSayisi);
                                }
                            }

                           
                            var yayinciNode = node.SelectSingleNode(".//div[@class='gs_a']");
                            var yayinci = yayinciNode != null ? GetPublisher(yayinciNode.InnerText.Trim()) : "";

                            var idNode = node.SelectSingleNode(".//a[@id]");
                            var yayinID = idNode != null ? idNode.GetAttributeValue("id", "") : "";
                           
                            var dateNode = node.SelectSingleNode(".//div[@class='gs_a']");
                            var publishedDate = "";
                            if (dateNode != null)
                            {
                                var dateText = DecodeHtmlCharacters(dateNode.InnerText.Trim());
                                var dateParts = dateText.Split('-');
                                if (dateParts.Length > 1)
                                {
                                    var fullDate = dateParts[1].Trim(); // Önce tüm tarih bilgisini alıyoruz

                                    // Virgülden sonrasını almak için
                                    var commaIndex = fullDate.LastIndexOf(",");
                                    if (commaIndex != -1)
                                    {
                                        publishedDate = fullDate.Substring(commaIndex + 1).Trim();
                                    }
                                }
                            }
                        

                            return (title, link, author, ozet, alintiSayisi, yayinci, yayinID,publishedDate);


                           
                        })
                        .Where(link => link.Item2.EndsWith(".pdf"))
                        .ToList();

                        pdfLinks.ForEach(pdfLink =>
                        {
                            var fileName = pdfLink.Item1.Replace(" ", "_");
                            var filePath = Path.Combine(Server.MapPath("~/pdfler"), $"{fileName}.pdf");
                            pdfLink.Item2 = $"DownloadPdf.ashx?title={fileName}&link={pdfLink.Item2}&filePath={filePath}";
                        });

                        return pdfLinks;
                    }

                    return new List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)>();
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Hata: {ex.Message}");
                return new List<(string Title, string Link, string Author, string ozet, int AlintiSayisi, string yayinci, string yayinID, string publishedDate)>();
            }
        }

        private string GetPublisher(string authorText)
        {
            var startIndex = authorText.LastIndexOf('-');
            if (startIndex != -1)
            {
                return authorText.Substring(startIndex + 1).Trim();
            }
            return "";
        }


        private async Task<string> FetchPdfContentAsync(string pdfUrl, string pdfTitle)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var pdfContent = await httpClient.GetByteArrayAsync(pdfUrl);
                    var fileName = $"{pdfTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    var directoryPath = Path.Combine(Server.MapPath("~/pdfler"));


                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var filePath = Path.Combine(directoryPath, fileName);

                    // PDF içeriğini dosyaya yaz
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await fileStream.WriteAsync(pdfContent, 0, pdfContent.Length);
                    }

                    return filePath;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Hata: {ex.Message}");
                return string.Empty;
            }
        }

        protected void btnFiltrele_Click(object sender, EventArgs e)
        {
            string filtreSecimi = ddlFiltreleme.SelectedValue;

            if (filtreSecimi == "tarih")
            {
                FiltreleYayinlanmaTarihineGore();
            }
            else if (filtreSecimi == "alinti")
            {
                FiltreleAlintiSayisinaGore();
            }
        }

        private void FiltreleYayinlanmaTarihineGore()
        {
            
            var siraliMakaleler = yeniMakaleler.OrderBy(makale => makale.publishedDate).ToList();

            // Yeniden oluşturulacak makale listesi
            var filtrelenmisMakaleListesi = new List<string>();

            
            foreach (var makale in siraliMakaleler.Take(maxPdfCount))
            {
                var makaleHtml = $"<li class='makale'><a href='{makale.Link}' target='_blank'>{makale.Title}</a></li>";
                makaleHtml += $"<div class='ozet'>{makale.ozet}</div>";
                makaleHtml += $"<div class='AlintiSayisi'>Alıntı Sayısı: <span class='alinti'>{makale.AlintiSayisi}</span> | Yayıncı: <span class='yayinci'>{makale.yayinci}</span> | Yazar: <span class='yazar'>{makale.Author}</span> | Yayınlanma Tarihi: <span class='publishedDate'>{makale.publishedDate}</span></div>"; // Alıntı sayısı ve yazarı ekleyin
                makaleHtml += "<hr>";
                filtrelenmisMakaleListesi.Add(makaleHtml);
            }

            // Yeniden oluşturulan makale listesini ekrana yaz
            makaleListesi.InnerHtml = string.Join("", filtrelenmisMakaleListesi);
        }

        private void FiltreleAlintiSayisinaGore()
        {
           
            var siraliMakaleler = yeniMakaleler.OrderByDescending(makale => makale.AlintiSayisi).ToList();

           
            var filtrelenmisMakaleListesi = new List<string>();

            foreach (var makale in siraliMakaleler.Take(maxPdfCount))
            {
                var makaleHtml = $"<li class='makale'><a href='{makale.Link}' target='_blank'>{makale.Title}</a></li>";
                makaleHtml += $"<div class='ozet'>{makale.ozet}</div>";
                makaleHtml += $"<div class='AlintiSayisi'>Alıntı Sayısı: <span class='alinti'>{makale.AlintiSayisi}</span> | Yayıncı: <span class='yayinci'>{makale.yayinci}</span> | Yazar: <span class='yazar'>{makale.Author}</span> | Yayınlanma Tarihi: <span class='publishedDate'>{makale.publishedDate}</span></div>"; // Alıntı sayısı ve yazarı ekleyin
                makaleHtml += "<hr>";
                filtrelenmisMakaleListesi.Add(makaleHtml);
            }

            // Yeniden oluşturulan makale listesini ekrana yaz
            makaleListesi.InnerHtml = string.Join("", filtrelenmisMakaleListesi);
        }


    }
}