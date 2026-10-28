# HotConfig

[![Build & Test](https://github.com/emreakdemir/configuration-library/actions/workflows/build-tests.yml/badge.svg)](https://github.com/emreakdemir/configuration-library/actions/workflows/build-tests.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/HotConfig.svg)](https://www.nuget.org/packages/HotConfig)

.NET servisleri için Redis'te duran, ortak ve yeniden başlatmadan güncellenen yapılandırma.

HotConfig bir `Microsoft.Extensions.Configuration` sağlayıcısıdır. Her servis kendi kayıtlarını Redis'ten okur ve değişiklikleri belirli aralıklarla kontrol eder. Bir değer değişince `IOptionsMonitor<T>` yeni değeri bir yenileme aralığı içinde alır; servisi yeniden başlatmak gerekmez. Küçük bir yönetim API'si ve tek sayfalık bir yönetim ekranı kayıtları MySQL'de tutar ve her değişikliği Redis'e yazar.

[English](README.md)

## Kurulum

```bash
dotnet add package HotConfig
```

Paket .NET 8 ve .NET 10'u hedefler.

## Hızlı başlangıç

1. Sağlayıcıyı ekleyin ve bir bölümü bağlayın:

```csharp
using HotConfig;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddHotConfig(o =>
{
    o.ApplicationName = "orders";
    o.Redis = "localhost:6379";
    o.RefreshInterval = TimeSpan.FromSeconds(10);
});

builder.Services.Configure<FeatureOptions>(builder.Configuration.GetSection("Features"));

var app = builder.Build();
app.MapGet("/features", (IOptionsMonitor<FeatureOptions> features) => features.CurrentValue);
app.Run();

public class FeatureOptions
{
    public string Banner { get; set; } = "";
    public int MaxItems { get; set; }
}
```

2. `orders` uygulaması için `Features:Banner` ve `Features:MaxItems` kayıtlarını ekleyin. Yönetim API'sini ya da yönetim ekranını kullanın.

3. Bir değeri değiştirin. `/features` sonraki yenilemeden sonra yeni değeri döner.

Tam örnek [samples/HotConfig.Sample](samples/HotConfig.Sample) klasöründe.

## Seçenekler

| Seçenek | Zorunlu | Varsayılan | Anlamı |
|---|---|---|---|
| `ApplicationName` | evet | | Kayıtları okunacak uygulama. Redis hash anahtarı da budur. |
| `Redis` | evet | | StackExchange.Redis bağlantı dizesi. |
| `RefreshInterval` | hayır | 30 saniye | Sağlayıcının Redis'i ne sıklıkla yeniden okuduğu. |
| `MySql` | hayır | | Eski kullanım. Verilirse sağlayıcı her okumadan önce uygulamanın etkin kayıtlarını MySQL'den Redis'e kopyalar. |

## Yenileme davranışı

- İlk yükleme uygulamanın bütün etkin kayıtlarını okur. Redis okunamazsa ilk yükleme hata fırlatır.
- Sonra bir zamanlayıcı her `RefreshInterval` sonunda Redis'i yeniden okur. Veri değiştiyse sağlayıcı yeniden yükleme sinyali verir. `IOptionsMonitor<T>` ve `IConfiguration` yeni değerleri döner.
- Bir yenileme başarısız olursa sağlayıcı son geçerli veriyi korur ve sonraki aralıkta yeniden dener.
- Etkin olmayan kayıtlar görünmez. Etkinliği kaldırılan ya da silinen kayıt sonraki yenilemede kaybolur.
- Kayıt adı bölüm oluşturmak için `:` içerebilir, örneğin `Features:Banner`. Anahtarlar büyük-küçük harf ayırt etmez.

## Yönetim API'si

Yönetim API'si `HotConfig.Api` projesindedir ve tek bir container olarak çalışır.

| Yöntem | Yol | İşlem |
|---|---|---|
| `GET` | `/api/configurations` | Kayıtları listeler. Filtreler: `applicationName`, `name` (içerir), `isActive`. |
| `GET` | `/api/configurations/{id}` | Tek kaydı getirir. |
| `POST` | `/api/configurations` | Kayıt ekler. |
| `PUT` | `/api/configurations/{id}` | Kaydı günceller. |
| `DELETE` | `/api/configurations/{id}` | Kaydı siler. |
| `POST` | `/api/configurations/{id}/toggle` | Kaydı etkin ya da etkin değil yapar. |
| `POST` | `/api/applications/{applicationName}/sync` | Uygulamanın etkin kayıtlarını MySQL'den Redis'e yeniden yazar. |
| `GET` | `/health` | Sağlık kontrolü. API anahtarı istemez. |

- Her `/api` isteği `X-Api-Key` başlığını ister. API anahtar olmadan açılmaz.
- `Type` şunlardan biridir: `String`, `Int32`, `Int64`, `Boolean`, `Double`, `Decimal`, `DateTime`. API `string`, `int`, `long`, `bool`, `double` ve `decimal` yazımlarını da kabul eder. Değer bu türe çevrilebilmelidir.
- Her yazma önce MySQL'e, sonra Redis'e gider. Redis yazması başarısız olursa API 500 döner. Redis'i onarmak için `sync` çağırın.
- OpenAPI, Development ortamında `/openapi/v1.json` adresindedir.

Çalıştırma:

```bash
mysql -u root -p hotconfig < db/schema.sql
docker build -t hotconfig-api .
docker run -p 8080:8080 \
  -e HotConfig__ApiKey=change-me \
  -e ConnectionStrings__MySql="Server=host.docker.internal;Database=hotconfig;User ID=hotconfig;Password=..." \
  -e ConnectionStrings__Redis=host.docker.internal:6379 \
  hotconfig-api
```

## Yönetim ekranı

`http://localhost:8080/` adresini açın ve API anahtarını girin. Kayıtları listeleyebilir, filtreleyebilir, ekleyebilir, düzenleyebilir, silebilir ve etkinleştirebilirsiniz. Bir uygulamayı Redis'le eşitleyebilirsiniz. Ekran derleme adımı olmayan tek bir HTML dosyasıdır.

## Diğer seçeneklerle karşılaştırma

| | HotConfig | Azure App Configuration | Consul KV |
|---|---|---|---|
| Maliyet | Ücretsiz, MIT | Sınırlı ücretsiz katman, sonra depo ve istek başına ücretli | Ücretsiz (açık kaynak) |
| Çalıştığı yer | Herhangi bir Redis + MySQL | Yalnız Azure | Bir Consul kümesi |
| .NET bağlantısı | `IConfiguration` sağlayıcısı | `IConfiguration` sağlayıcısı | Topluluk paketleri |
| Değişikliğin ulaşması | Aralıklı okuma | Aralıklı okuma ya da bildirim (Event Grid) | Bekleyen sorgular |
| Yönetim ekranı | Tek sayfa, pakette | Azure portalı | Consul arayüzü |

HotConfig, zaten Redis çalıştıran ve bulut servisi ya da yeni bir küme kurmadan ortak, canlı güncellenen ayar isteyen küçük ekipler içindir.

## Eski API

Önceki sürümlerdeki `ConfigurationReader` hâlâ çalışır ama kullanımdan kalkmıştır:

```csharp
var reader = new ConfigurationReader("orders", connectionStrings, refreshInterval: 10_000);
var rate = reader.GetValue<decimal>("Rate");
```

Kaydın `Type` değeri `T` ile eşleşmezse varsayılan değeri döner. Yeni kodda `AddHotConfig` kullanın.

## Derleme ve test

```bash
dotnet build HotConfig.sln -c Release
dotnet test HotConfig.sln -c Release
```

Testler Redis ve MySQL'i Testcontainers ile başlatır; Docker çalışıyor olmalıdır.

## Lisans

[MIT](LICENSE)
