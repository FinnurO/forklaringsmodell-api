# Forklaringsmodell API

**Dokumentasjonsnettsted:** https://finnuro.github.io/forklaringsmodell-api/ — formål, modell, forretningsregler, et konkret eksempel (Stavangers automatiske piperehabilitering), API-guide og versjoner.

> **Prototype / proof of concept** — ikke en produksjonstjeneste og ikke en offisiell Digdir-tjeneste. Utviklet sammen med Claude Code.

ASP.NET Core Web API som lar en saksbehandlingsløsning fylle ut og lese ut informasjonsmodellen som **forklarer et vedtak** — kombinasjonen av forvaltningsloven § 25 (begrunnelse) og digital-rettsstats lag for automatisert forklaring (Kildelaget, Datalaget, Regellaget).

Modellen dokumenterer et vedtak uavhengig av om vurderingen bak er deterministisk regelanvendelse, en generativ KI-vurdering eller et menneskelig skjønn — og uavhengig av om faktum er strukturert/ustrukturert eller kommer fra en autoritativ kilde.

**Kjerneprinsipp:** `Sak` er en levende saksmappe. `Vedtak` og `Forklaringslogg` er et frosset øyeblikksbilde. Alt som er referert av et frosset `Vedtak` er *append-only* — korrigeringer skjer ved å legge til nye rader, aldri ved å endre eksisterende.

Full spesifikasjon: [`docs/api-spesifikasjon-forklaringsmodell.md`](docs/api-spesifikasjon-forklaringsmodell.md). Versjonshistorikk: [`CHANGELOG.md`](CHANGELOG.md).

## Domenemodell

```mermaid
erDiagram
  SAK ||--o{ FAKTUM : har
  SAK ||--o{ PARTSMEDVIRKNING : har
  SAK ||--o{ VURDERING : inneholder
  SAK ||--o{ VEDTAK : resulterer_i
  FAKTUM }o--|| KILDE : kommer_fra
  KILDE }o--o{ RETTSKILDE : har_hjemmel_for_innhenting
  FAKTUM }o--o{ RETTSKILDE : har_tilleggshjemmel
  VURDERING }o--o{ FAKTUM : bruker
  VURDERING }o--|| REGEL : anvender
  REGEL }o--o{ RETTSKILDE : operasjonaliserer
  VURDERING }o--o{ RETTSKILDE : siterer
  VEDTAK ||--|| FORKLARINGSLOGG : har
  FORKLARINGSLOGG ||--o{ FORKLARINGSLOGG_OPPFORING : bestar_av
  VEDTAK ||--o{ VEDTAKSVIRKNING : medforer
  VEDTAKSVIRKNING }o--o{ VURDERING : fastsettes_av
  VEDTAKSVIRKNING }o--o{ FAKTUM : bygger_pa
  VEDTAKSVIRKNING }o--o| VILKAR : er_instans_av
  VILKAR }o--o{ RETTSKILDE : har_hjemmel
  VILKAR }o--o| REGEL : baserer_pa
  SAK }o--o{ SAK : relaterer_til
  VURDERING }o--o{ VURDERING : bygger_pa
  VURDERING }o--o| VURDERING : er_delvurdering_av
  VURDERING }o--o| VILKAR : gjelder
  VILKAR }o--o| FLERSPRAAKLIG_TEKST : har_forklaringstekst
  VURDERING }o--o| FLERSPRAAKLIG_TEKST : har_forklaringstekst
  VEDTAKSVIRKNING }o--o| FLERSPRAAKLIG_TEKST : har_forklaringstekst
  FLERSPRAAKLIG_TEKST ||--o{ TEKST_VARIANT : bestar_av
```

| Entitet | Rolle |
|---|---|
| `Sak` | Levende saksmappe. Utløst av en `HendelseType` (søknad, melding, klage …). |
| `Faktum` | Rått eller subsumert data om saken, med kildehenvisning og evt. rettslig tilleggshjemmel. |
| `Kilde` / `Rettskilde` | Hvor et faktum kom fra / hvilken lov, forskrift eller rundskriv som er hjemmel. |
| `Regel` | Operasjonalisert regelversjon (DMN, Python, LLM-prompt …), koblet til én eller flere rettskilder. |
| `Vurdering` | Resultatet av å anvende en `Regel` på fakta — deterministisk, generativ KI eller skjønn. Har alltid et `Utfall` (også når vilkåret ikke faktisk ble vurdert). |
| `Vedtak` / `Forklaringslogg` | Det frosne øyeblikksbildet: hvilke faktum/vurderinger/partsmedvirkninger som forklarer utfallet. |
| `Vedtaksvirkning` | En konkret virkning av vedtaket (tillatelse, plikt, ytelse, gebyr), evt. instans av en katalogført `Vilkar`. |
| `Vilkar` | Gjenbrukbar vilkårskatalog (som `Regel`) — rettslig, intern praksis eller teknisk/datakvalitet-forankret. |
| `SakRelasjon` | Kobler en oppfølgende sak til en relatert sak, uten å modifisere den. |
| `FlerspraakligTekst` / `TekstVariant` | Beholder for forklaringstekster på flere språk (nb, nn, …) — én variant per språk. |

## Forretningsregler (utvalg)

Alle 18 regler er beskrevet i spesifikasjonen (og forklart på [nettstedet](https://finnuro.github.io/forklaringsmodell-api/regler/)); de viktigste prinsippene:

- **Append-only etter frysing** — ingen `PUT`/`DELETE` på `Vedtak`, `Forklaringslogg` eller `Vedtaksvirkning`. Alt som refereres i en frosset forklaringslogg blir skrivebeskyttet.
- **Skjønn må forklares** — `Hovedhensyn` er obligatorisk når `Vurdering.Type == Skjonn`.
- **`AutomatiseringsGrad` beregnes serverside**, ikke av klienten, fra andelen skjønn/eskalerte vurderinger.
- **Kryss-sak-referanser er alltid skrivebeskyttede** — en vurdering kan lese fra en annen (allerede frosset) sak, men aldri endre den.
- **Referansedata (`Regel`, `Vilkar`) er append-only** når de er tatt i bruk — nye versjoner opprettes som nye rader.
- **Hele beslutningstreet kan gjengis** — en `Vurdering` kan ha delvurderinger i samme sak (`ForelderVurderingId`) og merkes med hvilket katalogvilkår den gjelder (`VilkarId`). Modellen foreskriver ingen fast nedbryting; den gjengir det kallende systemet faktisk gjorde.
- **Forklaringstekster er flerspråklige** (bokmål, nynorsk og flere) via `FlerspraakligTekst`/`TekstVariant`, uten skjemaendring per nytt språk.

## Arkitektur

Lagdelt .NET 10-løsning:

```
src/
  Forklaringsmodell.Domain          Entiteter, enumer, forretningsregler
  Forklaringsmodell.Application     DTO-er, FluentValidation, use-case-services
  Forklaringsmodell.Infrastructure  EF Core, migrasjoner, repository, seed-data
  Forklaringsmodell.Api             Kontrollere, Swagger, Program.cs
tests/
  Forklaringsmodell.Tests           Enhets- og integrasjonstester (xUnit)
```

EF Core mot SQLite lokalt (`Data Source=forklaringsmodell.db`) — PostgreSQL/SQL Server for reell drift.

## Kom i gang

```bash
dotnet build
dotnet test
```

Kjør API-et (migrerer og seeder databasen automatisk i utviklingsmiljø):

```bash
dotnet run --project src/Forklaringsmodell.Api
```

Swagger-UI åpnes på `https://localhost:7151/swagger` (eller `http://localhost:5013/swagger`), og gir en fullstendig, utprøvbar oversikt over alle endepunkter.

Seed-dataen setter opp et komplett dagpenger-eksempel (sak, faktum, vurderinger med alle `Utfall`-typer, vedtak med virkning) pluss en oppfølgende sak som demonstrerer sak-relasjoner og kryss-sak-referanser — klar for manuell utforskning rett etter oppstart.

## API-endepunkter

| Metode | Sti | Beskrivelse |
|---|---|---|
| GET/POST | `/api/saker` | List / opprett sak |
| GET/PUT | `/api/saker/{id}` | Les / oppdater sak |
| GET/POST | `/api/saker/{sakId}/relasjoner` | List / opprett saksrelasjon |
| GET/POST | `/api/saker/{sakId}/faktum` | List / registrer faktum |
| POST | `/api/faktum/{id}/transformer` | Opprett subsumert faktum avledet fra et rått faktum |
| GET | `/api/faktum/{id}` | Les ett faktum |
| GET/POST | `/api/kilder` | Kilde (referansedata) |
| GET/POST | `/api/rettskilder` | Rettskilde (referansedata) |
| GET/POST | `/api/regler` | Regel (referansedata) |
| GET/POST | `/api/vilkar` | Vilkår (referansedata) |
| GET/POST | `/api/saker/{sakId}/vurderinger` | List / registrer vurdering |
| GET | `/api/vurderinger/{id}` | Les én vurdering |
| GET/POST | `/api/saker/{sakId}/partsmedvirkning` | List / registrer partsmedvirkning |
| POST | `/api/saker/{sakId}/vedtak` | Opprett vedtak (fryser forklaringslogg + virkninger) |
| GET | `/api/vedtak/{id}` | Les vedtaket |
| GET | `/api/vedtak/{id}/forklaring` | Hydrert forklaring: frosset øyeblikksbilde med vurderingstre, oppløst referansedata og kryss-sak-referanser |
| GET | `/api/saker/{sakId}/forklaring` | Hele saken levende i ett svar (også uten vedtak): faktum, vurderinger flatt og som tre, partsmedvirkning, vedtak med virkninger, referansedata |
| GET | `/api/vedtak/{id}/virkninger` | List vedtaksvirkninger |

Forklaringen kan leses på to måter: `GET /api/vedtak/{id}/forklaring` gir det frosne øyeblikksbildet bak ett vedtak, mens `GET /api/saker/{sakId}/forklaring` gir den levende saken samlet, med `erLaast` på hver rad som viser hva et vedtak har frosset.

Se spesifikasjonen for fullstendige request/response-skjemaer og valideringsregler.

## Nettsted (GitHub Pages)

Dokumentasjonsnettstedet ligger i [`site/`](site/) som ren statisk HTML/CSS/JS — ingen bygg-steg, ingen avhengigheter, og ingen kjøretidsavhengighet til eksterne CDN-er (designsystemet.no sitt CSS og fontene Inter og Source Serif 4 er selv-hostet i `site/assets/`, se [`site/THIRD_PARTY_NOTICES.md`](site/THIRD_PARTY_NOTICES.md)). Stilen følger [tjenestedesign-no](https://github.com/FinnurO/tjenestedesign-no).

```
site/
  index.html          Forside: formål og hovedprinsipper
  modell/             Entiteter, relasjoner, beslutningstreet, flerspråklige tekster
  regler/             De 18 forretningsreglene
  eksempel/           Stavangers automatiske piperehabilitering, fylt ut i modellen
  eksempel/data/      De 26 faktiske API-kallene (forespørsel og svar) som JSON-filer
  api/                Kom i gang, endepunkter, typisk flyt
  versjoner/          Versjonshistorikk
  assets/             CSS (designsystemet + site.css), fonter, site.js (tilbakemeldingsknapp)
```

Kjør lokalt — server mappen med hva som helst statisk:

```bash
python -m http.server 8000 --directory site
```

**Deploy:** `.github/workflows/pages-deploy.yml` kopierer `site/` til `gh-pages`-branchen ved hvert push til `master` som endrer `site/`. `.github/workflows/pages-pr-preview.yml` publiserer en forhåndsvisning per åpne PR som endrer `site/`, på `https://finnuro.github.io/forklaringsmodell-api/pr-preview/pr-<nummer>/`. Under *Settings → Pages* står kilden på `gh-pages` / rot.

**Tilbakemelding:** knappen «Gi tilbakemelding» nederst til høyre på alle sider åpner et forhåndsutfylt GitHub-issue (merket `tilbakemelding`) med side, avsnitt og eventuell merket tekst.
