# PrintCoverageAnalyzer (.NET / C#)

Windows-Tool zur Berechnung der **bedruckten Fläche** (alles außer Weiß) inkl. **Preisermittlung nach Papiersorte**.

## Neu: Papiersorten mit Preis + Auswahl vor Berechnung

Die Anwendung hat jetzt eine Windows-Oberfläche:

- Auswahl der Papiersorte über ein Dropdown-Menü **vor** der Berechnung
- Button **„+ Neue Papiersorte“** zum Anlegen weiterer Sorten per Klick
- Preis pro m² wird pro Papiersorte gespeichert
- Kalkulation: `Preis = Fläche (m²) * Papierpreis/m² * bedruckter Anteil`

Gespeicherte Papiersorten liegen pro Benutzer in:

- `%AppData%\PrintCoverageAnalyzer\paper-types.json`

## EXE bauen (für alle PCs im Geschäft)

Voraussetzung: .NET 8 SDK auf einem Build-PC.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\publish-windows.ps1 -Runtime win-x64 -Version 1.1.0
```

Ergebnis:

- `dist/package/PrintCoverageAnalyzer-win-x64-v1.1.0.zip`

Enthalten:

- `PrintCoverageAnalyzer.exe`
- `Run-Analyse.bat`
- `Install-PrintCoverageAnalyzer.ps1`
- `README.md`

## Verteilung auf mehrere PCs

### Variante A (einfach, ohne Admin)

- ZIP entpacken, z. B. nach `C:\Tools\PrintCoverageAnalyzer`
- `PrintCoverageAnalyzer.exe` starten

### Variante B (mit Admin)

```powershell
powershell -ExecutionPolicy Bypass -File .\Install-PrintCoverageAnalyzer.ps1
```

Dann wird installiert nach:

- `C:\Program Files\PrintCoverageAnalyzer`

und eine Desktop-Verknüpfung angelegt.

## Nutzung in 4 Schritten

1. Papiersorte im Menü auswählen
2. Optional neue Papiersorte per Klick hinzufügen
3. Fläche (m²), Weiß-Schwelle und PDF-DPI setzen
4. Datei(en) auswählen und auf **„Fläche berechnen & Preis kalkulieren“** klicken

## Unterstützte Dateiformate

- `JPG`, `PNG`, `TIFF`, `BMP`, `WEBP`, `PDF`

## Genauigkeit

- PDF wird gerastert; höhere DPI = genauer, aber langsamer.
- Weißer Bereich wird über Schwellwert erkannt (nicht nur 255/255/255).
