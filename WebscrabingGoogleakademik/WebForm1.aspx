<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="yazlabproje1.WebForm1" Async="true" %>
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Akademik </title>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f9f9f9; /* Arka plan rengi */
            background-image: url('https://cdn.pixabay.com/photo/2016/10/17/14/31/background-1747777_1280.jpg'); /* Desenli arka plan */
        }

        header {
            text-align: center;
            padding: 20px 0;
            background-color: #a9a9a9; /* Başlık arka plan rengi */
            color: white;
            text-transform: uppercase;
            font-size: 36px; /* Başlık font boyutu */
            margin-bottom: 20px; /* Başlık alt boşluk */
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); /* Hafif gölge efekti */
        }

        .logo {
            text-align: center;
        }

        .logo img {
            width: 300px; /* Logo genişliği */
            margin-bottom: 20px;
        }

        main {
            text-align: center;
        }

        .arama-cubugu {
            margin-top: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        input[type="text"] {
            padding: 10px;
            width: 300px; /* Arama kutusu genişliği */
            border-radius: 20px; /* Kenar yuvarlatma */
            border: 2px solid #e9967a; /* Kenarlık rengi */
            outline: none; /* Odak çerçevesini kaldır */
        }

        button#btnAra {
            padding: 12px 30px; /* Buton iç boşluk */
            background-color: #4285f4; /* Buton arka plan rengi */
            color: white;
            border: none;
            cursor: pointer;
            transition: background-color 0.3s;
            border-radius: 20px; /* Kenar yuvarlatma */
            outline: none; /* Odak çerçevesini kaldır */
            font-size: 16px; /* Buton metin font boyutu */
        }

        button#btnAra:hover {
            background-color: #357ae8; /* Butonun üzerine gelindiğinde arka plan rengi */
        }

        .makale-listesi {
            margin-top: 40px; /* Makale listesinin üst boşluğunu artır */
            padding: 0; /* Liste içi boşluk */
            text-align: left; /* Metni sola hizala */
        }

        .makale-listesi .makale {
            background-color: #ffffff; /* Makale arka plan rengi */
            border-radius: 10px; /* Kenar yuvarlatma */
            padding: 20px; /* Makale iç boşluk */
            margin-bottom: 20px; /* Makaleler arası boşluk */
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); /* Hafif gölge efekti */
            transition: box-shadow 0.3s; /* Gölge efekti geçişi */
        }

        .makale-listesi .makale:hover {
            box-shadow: 0 6px 8px rgba(0, 0, 0, 0.2); /* Makale üzerine gelindiğinde gölgeyi artır */
        }
      
    .alinti {
        color: blue; /* Alıntı sayısı rengi */
    }

    .yazar {
        color: blue; /* Yazar adı rengi */
    }
    
   .yayinci {
     color: blue; /* Yazar adı rengi */
 }
      .publishedDate {
     color: blue; /* Yazar adı rengi */
 }


    /* Desen için stil */
   html {
    background-color: #f9f9f9; /* Arka plan rengi */
    background-image: url('https://www.transparenttextures.com/patterns/45-degree-fabric-light.png'); /* Desenli arka plan */
    background-size: cover; /* Arka plan resminin ekranı kaplamasını sağlar */
}


    </style>
</head>
<body>
   <header>
    Akademik  <!-- Başlık -->
    
</header>

  
    <main>
     <form id="form1" runat="server">
    <div class="arama-cubugu">
        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
        <asp:Button ID="btnAra" runat="server" Text="Ara" OnClick="btnAra_Click" />
      
        <asp:DropDownList ID="ddlFiltreleme" runat="server">
            <asp:ListItem Text="Yayınlanma Tarihine Göre" Value="tarih"></asp:ListItem>
            <asp:ListItem Text="Alıntılanma Sayısına Göre" Value="alinti"></asp:ListItem>
        </asp:DropDownList>
        <asp:Button ID="btnFiltrele" runat="server" Text="Filtrele" OnClick="btnFiltrele_Click" />
    </div>

    </div>
</form>

        <!-- Makalelerin listeleneceği alan -->
        <ul class="makale-listesi" id="makaleListesi" runat="server">
            <!-- Makale kartları burada dinamik olarak oluşturulacak -->
        </ul>
  
    </main>
</body>
</html>
