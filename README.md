# PrintCoverageAnalyzer (.NET / C#)

Kleines CLI-Tool für Windows (und andere Plattformen), das den **bedruckten Flächenanteil** (alles außer Weiß) aus Rasterbildern und PDFs berechnet.

## Features

- Eingaben: `JPG`, `PNG`, `TIFF`, `BMP`, `WEBP`, `PDF`
- Kennzahlen pro Datei/Seite:
  - Bedruckte Fläche in `%`
  - Bedruckte Pixel vs. Gesamtpixel
  - CMYK-Näherung (C/M/Y/K und Durchschnitt als Tinten-Proxy)
- Schwellwerte anpassbar (`--white-threshold`, `--alpha-threshold`)
- JSON-Export (`--json-out`)

## Beispiel

```bash
dotnet run -- poster-a0.pdf --pdf-dpi 300 --white-threshold 245 --json-out report.json
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
