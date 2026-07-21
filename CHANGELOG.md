# Changelog

Alle vesentlige endringer i dette prosjektet dokumenteres i denne filen.

## [1.1.0] — Rettskildekobling og CPSV-AP/CCCEV-sporbarhet

### Endret domenemodell

- **`Rettskilde` omstrukturert**: `Paragraf` er byttet ut med `Henvisning`, og en ny `Type`-enum (`RettskildeType`: `Lov`, `Forskrift`, `Rundskriv`, `Forarbeider`, `Rettspraksis`, `InternasjonalRett`, `Forvaltningspraksis`) er lagt til. `VersjonDato` er nå valgfri (`DateTimeOffset?`) — kun meningsfull for `Lov`/`Forskrift`.
- **`Regel.RettskildeId` (enkeltverdi) er byttet ut med `Regel.RettskildeIder` (mange-til-mange)**. En regel kan nå hjemles i flere kilder samtidig (typisk lov + forskrift + rundskriv).
- **Ny relasjon `Kilde` → `Rettskilde` (mange-til-mange)**: en kilde skal ha minst én rettskilde som hjemmel for innhenting, før den kan brukes til å registrere faktum (ny forretningsregel 3.8).
- **Ny valgfri relasjon `Faktum` → `Rettskilde` (mange-til-mange)**: brukes kun når én konkret innhenting krever en tilleggshjemmel utover kildens standardhjemmel.
- **Ny valgfri relasjon `Vurdering` → `Rettskilde` (mange-til-mange)**: saksspesifikke kilder ut over regelens generelle hjemmel, f.eks. en konkret dom en saksbehandler siterer i en skjønnsutøvelse.
- **`Sak.TjenesteReferanse`** (valgfri streng): URI til en CPSV-AP `PublicService` i Felles datakatalog (data.norge.no).
- **`Kilde.CpsvReferanse`** (valgfri streng): URI til en CCCEV `Evidence`/`Criterion`.
- **`Regel.CpsvRuleReferanse`** (valgfri streng): URI til en CPSV-AP `Rule`.

### Nye forretningsregler

- **Regel 3.7**: `Rettskilde` kobles aldri direkte til `Sak` — kun via `Regel.RettskildeIder` og/eller `Vurdering.RettskildeIder`.
- **Regel 3.8**: `Kilde` må ha minst én tilknyttet `Rettskilde` før den kan brukes til å registrere `Faktum`; valideres på API-nivå ved `POST /api/kilder`.
- **Regel 3.9**: CPSV-AP/CCCEV-referansene er eksterne, valgfrie URI-er — ikke internt eide entiteter, og valideres ikke mot noen lokal tabell. En `Vurdering`/`Vedtak` er fullt forklart uten dem.

### Endrede API-kontrakter

- `POST /api/rettskilder`: `paragraf` → `henvisning`, nytt obligatorisk `type`-felt, `versjonDato` er nå valgfri.
- `POST /api/regler`: `rettskildeId` (enkeltverdi) → `rettskildeIder` (liste, minst én), nytt valgfritt `cpsvRuleReferanse`-felt.
- `POST /api/kilder`: nytt obligatorisk `rettskildeIder`-felt (minst én), nytt valgfritt `cpsvReferanse`-felt.
- `POST /api/saker/{sakId}/faktum`: nytt valgfritt `rettskildeIder`-felt.
- `POST /api/saker/{sakId}/vurderinger`: nytt valgfritt `rettskildeIder`-felt.
- `POST /api/saker`: nytt valgfritt `tjenesteReferanse`-felt.
- `PUT`/`DELETE` på `kilder` avvises nå også (409/423) dersom kilden er brukt av minst ett `Faktum` — samme append-only-mønster som allerede gjaldt `faktum`/`vurderinger`/`regler`.

### Migrasjon

- Ny EF Core-migrasjon legger til fire koblingstabeller (`RegelRettskilde`, `KildeRettskilde`, `FaktumRettskilde`, `VurderingRettskilde`), endrer `Rettskilder`-tabellen (kolonneomdøping/nullability), og legger til `TjenesteReferanse`/`CpsvReferanse`/`CpsvRuleReferanse`-kolonner.

### Seed-data

- Dagpenger-eksempelet er utvidet med tre `Rettskilde`-rader og referanser fra `Sak`, `Kilde` (A-ordningen), og de deterministiske/skjønnsbaserte `Vurdering`-radene, i tråd med det oppdaterte eksempelet i spesifikasjonens punkt 6.

## [1.0.0] — Første leveranse

- Domenemodell for `Sak`, `Kilde`, `Faktum`, `Rettskilde`, `Regel`, `Vurdering`, `Partsmedvirkning`, `Vedtak`, `Forklaringslogg`.
- Append-only-håndheving på frosne vedtak (regel 3.1), obligatorisk hovedhensyn for skjønn (3.2), konfidensvalidering (3.3), append-only regelspeil (3.4), serverberegnet automatiseringsgrad (3.5), flere vedtak per sak (3.6).
- Alle endepunkter fra spesifikasjonens punkt 5, inkl. hydrert forklaring.
- EF Core-migrasjon + seed-data fra dagpenger-eksempelet.
- Enhetstester for forretningsreglene.
