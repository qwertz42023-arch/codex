# PrintCoverageAnalyzer (.NET / C#)

Kleines Tool für Windows, das den **bedruckten Flächenanteil** (alles außer Weiß) aus Rasterbildern und PDFs berechnet.

## Features

- Eingaben: `JPG`, `PNG`, `TIFF`, `BMP`, `WEBP`, `PDF`
- Kennzahlen pro Datei/Seite:
  - Bedruckte Fläche in `%`
  - Bedruckte Pixel vs. Gesamtpixel
  - CMYK-Näherung (C/M/Y/K und Durchschnitt als Tinten-Proxy)
- Schwellwerte anpassbar (`--white-threshold`, `--alpha-threshold`)
- JSON-Export (`--json-out`)

## EXE bauen (für alle PCs im Geschäft)

### 1) Einmal auf einem Build-PC ausführen

Voraussetzung: .NET 8 SDK installiert.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\publish-windows.ps1 -Runtime win-x64 -Version 1.0.0
```

Danach liegt eine fertige ZIP-Datei hier:

- `dist/package/PrintCoverageAnalyzer-win-x64-v1.0.0.zip`

Diese enthält:

- `PrintCoverageAnalyzer.exe` (self-contained, kein .NET Runtime-Install nötig)
- `Run-Analyse.bat` (einfacher Starter)
- `Install-PrintCoverageAnalyzer.ps1` (optional für lokale Installation + Desktop-Shortcut)

### 2) Einfachste Verteilung auf mehrere PCs

**Variante A (empfohlen, ohne Admin-Rechte):**

- ZIP auf jedem PC entpacken, z. B. nach `C:\Tools\PrintCoverageAnalyzer`
- `Run-Analyse.bat` auf den Desktop kopieren
- Dateien auf die BAT ziehen (Drag & Drop)

**Variante B (mit Admin-Rechten):**

- ZIP entpacken
- PowerShell als Administrator starten
- aus dem entpackten Ordner ausführen:

```powershell
powershell -ExecutionPolicy Bypass -File .\Install-PrintCoverageAnalyzer.ps1
```

Ergebnis:

- Installation nach `C:\Program Files\PrintCoverageAnalyzer`
- Desktop-Verknüpfung für alle Benutzer wird angelegt

## Nutzung

```bat
Run-Analyse.bat poster-a0.pdf --pdf-dpi 300 --white-threshold 245 --json-out report.json
```

Beispielausgabe:

```text
[poster-a0.pdf#page-1] 9933x14043
  Bedruckt: 38.42% (53,590,000/139,490,000 Pixel)
  CMYK-Schätzung: C=11.22% M=9.80% Y=10.11% K=7.33% | Ø=9.62%
```

## Preislogik (optional)

Eine mögliche Preisformel:

```text
Preis = Grundpreis + (Vollflächenpreis - Grundpreis) * (Bedruckt% / 100)
```

So kann ein Motiv mit viel Weißanteil automatisch günstiger werden.

## Hinweise zur Genauigkeit

- PDF wird gerastert; DPI erhöht die Genauigkeit (und Rechenzeit).
- "Weiß" wird über Schwellwert bestimmt, nicht nur `255/255/255`.
- Für produktive Kalkulation empfiehlt sich Kalibrierung mit Testjobs aus eurem RIP.
