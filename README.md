# YouTube-Downloader

Desktopowa aplikacja .NET 8 (WinForms) do przeglądania kanałów YouTube i pobierania audio, z lokalną biblioteką odsłuchów i historią pobrań. Odsłuchy zbiera towarzysząca wtyczka do przeglądarki (Chrome / Edge).

## Funkcje

- **Przeglądanie kanału** — wklejasz URL, `@handle`, `/user/…`, `/c/…` albo ID `UC…`; aplikacja ładuje playlisty i uploady kanału z miniaturami.
- **Pobieranie audio** — zaznaczasz filmy i pobierasz najlepszy dostępny strumień audio (do 5 równolegle), z paskiem postępu i logiem.
- **Filtrowanie i zaznaczanie** — filtr po tytułach, „Select All”, „Show All” (filmy ze wszystkich playlist).
- **Lokalna biblioteka (SQLite)** — lista „Recently listened” i „Download history” osobno dla każdego konta.
- **Wtyczka do przeglądarki** — zapisuje film jako odsłuchany po obejrzeniu ok. 80%. Gdy aplikacja działa, wysyła odsłuchy na żywo (`http://127.0.0.1:17823`); gdy nie, zapisuje `pending-listens.json`, który aplikacja importuje przy starcie.

## Stack

| | |
|---|---|
| Język / platforma | C#, .NET 8, Windows Forms |
| Metadane YouTube | `Google.Apis.YouTube.v3` (YouTube Data API v3) |
| Pobieranie strumieni | `YoutubeExplode` |
| Baza danych | SQLite (`Microsoft.Data.Sqlite`) |
| Testy | MSTest |
| Wtyczka | JavaScript, Manifest V3 |

## Struktura

```
Do pobierania z YT.sln
├── MusicDownloadLib        logika: kanały, playlisty, pobieranie, biblioteka SQLite
├── MusicDownloadUI         aplikacja WinForms
├── Do pobierania z YT      prosta aplikacja konsolowa (lista uploadów i playlist)
├── MusicDownloadLibTests   testy jednostkowe i integracyjne
└── BrowserExtension        wtyczka do Chrome / Edge
```

Szczegóły architektury, diagramy i przepływ danych: [ARCHITECTURE.md](ARCHITECTURE.md).

## Uruchomienie

Wymagania: Windows, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), klucz [YouTube Data API v3](https://console.cloud.google.com/apis/library/youtube.googleapis.com).

1. Podaj klucz API w jeden z dwóch sposobów:
   - zmienna środowiskowa `YOUTUBE_API_KEY`, albo
   - plik `youtube-api-key.txt` w katalogu solution (jest w `.gitignore`).
2. Uruchom aplikację:
   ```
   dotnet run --project MusicDownloadUI
   ```
3. (Opcjonalnie) zainstaluj wtyczkę: `chrome://extensions` lub `edge://extensions` → tryb dewelopera → „Load unpacked” → folder `BrowserExtension`. Szczegóły w [BrowserExtension/INSTALL.txt](BrowserExtension/INSTALL.txt).

Pliki trafiają domyślnie do `Muzyka\YouTube Downloads` (można zmienić przyciskiem **Folder**). Baza biblioteki: `%LocalAppData%\MusicDownload\library.db`.

## Testy

```
dotnet test
```

`ChannelTests` to testy integracyjne — wymagają klucza API i połączenia z internetem. Pozostałe (`PlaylistTests`, `YouTubeUrlTests`, `LibraryStoreTests`, `PlaylistItemExtensionTests`) działają offline.

## Uwagi

Projekt edukacyjny do użytku prywatnego. Pobieraj tylko treści, do których masz prawa, zgodnie z [warunkami korzystania z YouTube](https://www.youtube.com/t/terms).
