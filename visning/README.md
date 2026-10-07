# Lesevisning av forklaringen

En standard, lesbar visning av forklaringsmodellen, drevet av data. Modulen tar svaret fra `GET /api/saker/{sakId}/forklaring` (eller `GET /api/vedtak/{id}/forklaring`) og gjør det om til en strukturert, lesbar visning i Markdown, ren tekst eller HTML.

Lesbarheten kommer fra en generisk parser, ikke fra håndskrevne setninger per sak. Samme kode viser hvilken som helst sak, og all tekst som vises kommer fra svaret.

- Modulen: [`site/assets/js/forklaring-visning.mjs`](../site/assets/js/forklaring-visning.mjs). Ren ES-modul uten avhengigheter og uten DOM, og den kjører likt i nettleser og Node.
- Kommandolinjeverktøy: [`visning/cli.mjs`](cli.mjs).
- Tester: [`visning/test/visning.test.mjs`](test/visning.test.mjs), kjørt mot de innspilte, ekte svarene i [`site/eksempel/data`](../site/eksempel/data).
- Eksempelsiden på nettstedet tegnes av denne modulen.

Ingen endring i API-et og ingen migrasjon: modulen leser bare det API-et allerede leverer.

## De tre stegene

Hvert steg kan brukes og testes for seg.

| Steg | Funksjon | Hva den gjør |
|---|---|---|
| 1 | `normaliser(json)` | Gir samme form uansett endepunkt. Godtar svaret slik API-et leverer det, og også pakket som `{ method, path, response }` (slik filene på eksempelsiden er lagret). Kaster en tydelig feil hvis formen ikke kjennes igjen. |
| 2 | `byggVisning(json, { sprak })` | Bygger visningsmodellen: en lesbar struktur uten noe format. Slår opp kilder, rettskilder, regler og vilkår i `referansedata`, velger språk og regner ut merknader. |
| 3 | `tilMarkdown`, `tilTekst`, `tilHtml`, `tilHtmlDokument` | Formaterer visningsmodellen. Hver er en ren funksjon over visningsmodellen. |

I tillegg finnes `visningCss` (stilark for `tilHtml`) og `vis(json, format, valg)`, som kjører alle tre stegene i ett kall.

Svaret fra vedtak-endepunktet normaliseres til samme form som saksvisningen, med ett vedtak i listen. Det mangler da `sak` og `relasjoner`, og tittelen blir `Vedtak: <utfall>`. Har svaret ikke `vurderingstre`, bygges treet fra den flate listen via `forelderVurderingId`.

## Bruk i nettleser

```js
import { byggVisning, tilHtml, visningCss } from '../assets/js/forklaring-visning.mjs';

// Stilarket legges inn én gang per side.
const stil = document.createElement('style');
stil.textContent = visningCss;
document.head.append(stil);

const svar = await fetch('/api/saker/<sakId>/forklaring').then((r) => r.json());
document.querySelector('#visning').innerHTML = tilHtml(byggVisning(svar));
```

- Stien i `import` er relativ til siden. Moduler lastes over `http(s)`, ikke fra `file://`, så server mappen med en vanlig statisk server.
- `tilHtml` gir et fragment med klasser som starter på `fv-` og er pakket i `<article class="fv">`. All tekst fra svaret escapes, så resultatet kan settes inn med `innerHTML`.
- `visningCss` bruker designsystemets tokens (`--ds-color-*`) når de finnes, og faller ellers tilbake til nøytrale verdier. Samme CSS fungerer derfor både på nettstedet og i en frittstående fil.
- Vil du ha en hel, frittstående HTML-fil (eksport eller utskrift), bruker du `tilHtmlDokument(vm)`. Den har stilarket inline og støtter mørkt fargevalg.

## Bruk i Node

```js
import { byggVisning, tilMarkdown, vis } from './site/assets/js/forklaring-visning.mjs';

const vm = byggVisning(json, { sprak: 'nn' });   // visningsmodellen
console.log(tilMarkdown(vm, { alleSprak: false }));

// Eller i ett kall:
console.log(vis(json, 'txt'));
```

Valg:

| Valg | Gjelder | Standard | Betydning |
|---|---|---|---|
| `sprak` | `byggVisning` (og `vis`) | `'nb'` | Ønsket språk for flerspråklige tekster. Faller tilbake til `nb`, og ellers til første tilgjengelige variant. |
| `alleSprak` | formaterne (og `vis`) | `true` | Viser også de øvrige språkvariantene, merket med språkkode. Sett til `false` for bare ønsket språk. |

## Kommandolinje

Kjør fra rotmappen i repoet. Det kreves Node 20 eller nyere, og ingenting må installeres.

```bash
node visning/cli.mjs <fil|-> [--url <adresse>] [-f md|txt|html|dokument|json] [--sprak nb|nn] [--en-sprak] [-o fil]
```

```bash
# Markdown til skjerm, fra en lagret fil
node visning/cli.mjs site/eksempel/data/17-sak-a-sak-forklaring.json

# Frittstående HTML-fil
node visning/cli.mjs svar.json -f dokument -o sak.html

# Rett fra et kjørende API
node visning/cli.mjs --url http://localhost:5013/api/saker/<sakId>/forklaring

# Pipe fra curl (- betyr standard inn)
curl -s http://localhost:5013/api/saker/<sakId>/forklaring | node visning/cli.mjs -

# Nynorsk som hovedspråk, uten øvrige språkvarianter
node visning/cli.mjs svar.json --sprak nn --en-sprak
```

| Flagg | Betydning |
|---|---|
| `<fil>` eller `-` | JSON-fil, eller `-` for standard inn. |
| `--url <adresse>` | Henter svaret fra adressen med `fetch` i stedet for å lese en fil. |
| `-f`, `--format` | Utformat, se under. Standard er `md`. |
| `--sprak <kode>` | Ønsket språk. Standard er `nb`. |
| `--en-sprak` | Viser bare ønsket språk, ikke de øvrige variantene. |
| `-o`, `--ut <fil>` | Skriver til fil i stedet for skjerm. |
| `-h`, `--help` | Viser bruk. |

Ved feil skriver verktøyet `Feil: …` til standard feil og avslutter med kode 1.

## Formater

| Format | Funksjon | Utdata |
|---|---|---|
| `md` (også `markdown`) | `tilMarkdown` | Markdown med overskrifter, lister og et nøstet tre for vurderingene. |
| `txt` (også `tekst`) | `tilTekst` | Samme innhold som Markdown, uten markeringstegn. |
| `html` | `tilHtml` | HTML-fragment med klassene `fv-*`. Bruk sammen med `visningCss`. |
| `dokument` (også `htmldoc`) | `tilHtmlDokument` | Frittstående HTML-dokument med stilark. |
| `json` (også `visning`) | `JSON.stringify` | Visningsmodellen som JSON. Nyttig for å bygge eget format eller feilsøke. |

Alle formater har de samme seksjonene: sammendrag, merknader, vedtak med virkninger, vurderinger som tre, partsmedvirkning, relaterte saker, faktum, faktum fra andre saker, og kilder og hjemler. Seksjoner uten innhold utelates der det er naturlig.

## Visningsmodellen

`byggVisning` returnerer ett objekt. Dette er feltene:

| Felt | Innhold |
|---|---|
| `versjon` | Versjon av visningsmodellens form (`VISNINGSVERSJON`, nå `1`). |
| `endepunkt` | `'sak'` eller `'vedtak'`, etter hvilket svar som ble gitt inn. |
| `sprak` | Ønsket språk. |
| `tittel` | Sakens tittel, eller `Vedtak: <utfall>` for vedtak-endepunktet. |
| `sak` | `{ id, status, hendelse, opprettet, tjenesteReferanse }`. `null` for vedtak-endepunktet. |
| `tall` | `{ vedtak, faktum, faktumLaast, vurderinger, vurderingerLaast, eskalerte, rotvurderinger }`. |
| `vedtak` | Liste. Hvert vedtak har `id`, `utfall`, `tidspunkt`, `automatiseringsgrad`, `logg` (`{ oppforinger, faktum, vurderinger, partsmedvirkninger }`) og `virkninger`. |
| `vedtak[].virkninger` | `type`, `varighet`, `fastsettelsesmate`, `beskrivelse`, `lopendeVilkar`, `gyldigFra`, `gyldigTil`, `belop`, `vilkar`. |
| `vurderinger` | Rotvurderingene som tre. Se nodefeltene under. |
| `faktum` | Faktum i saken: `id`, `verdi`, `type`, `struktur`, `innhentet`, `kilde` (`{ navn, autoritativ }`), `laast`, `annenSak`, `avledetFra`. |
| `partsmedvirkninger` | `{ type, tidspunkt, innhold }`. |
| `relasjoner` | `{ type, relatertSakId }`. Tom for vedtak-endepunktet. |
| `kryssSak` | `{ vurderinger, faktum }`: vurderinger og faktum fra andre saker som er sitert. |
| `referanser` | `{ kilder, rettskilder, regler, vilkar }`: katalograder som svaret viser til, ferdig oppløst. |
| `merknader` | Liste av `{ niva, tekst }`, der `niva` er `info` eller `advarsel`. Se neste avsnitt. |

Hver node i `vurderinger` har:

| Felt | Innhold |
|---|---|
| `id`, `utfall`, `utfallTekst` | Id, rå enumverdi og lesbar etikett. |
| `type` | Lesbar etikett for vurderingstypen (deterministisk regel, generativ KI eller skjønn). |
| `erKonklusjon` | `true` for `Oppfylt` og `IkkeOppfylt`, ellers `false` (regel 3.14). |
| `eskalert`, `laast`, `konfidens` | Status. `konfidens` er `null` når den ikke er satt. |
| `beregningsspor` | Tekst fra svaret, eller `null`. |
| `hovedhensyn`, `forkastedeUtfall` | Flerspråklig tekst: `{ tekst, sprak, andre }`, eller `null`. |
| `vilkar` | Katalogvilkåret vurderingen gjelder: `{ navn, kode, kodeverk, tekst }`, eller `null`. |
| `regel` | `{ teknologi, type, referanse }`, eller `null`. |
| `faktum` | Faktumene vurderingen bygger på (samme felt som i `faktum`). |
| `hjemler` | `{ henvisning, type, eli }`. |
| `siterer` | Vurderinger i andre saker som denne bygger på: `{ id, utfall, beregningsspor, annenSak }`. |
| `barn` | Delvurderinger, med samme felt. |

Flerspråklige tekster er objekter med `tekst` (valgt variant), `sprak` (språkkoden til den valgte varianten) og `andre` (øvrige varianter som `{ sprak, tekst }`).

I Markdown og HTML vises regelen der den starter, ikke gjentatt på hver delvurdering som bruker samme regel. Vilkåret vises på rotvurderingen.

## Automatiske merknader

Merknader regnes ut av strukturen alene, uten kunnskap om saken. Hver merknad kommer bare når dataene tilsier det.

| Merknad | Nivå | Kommer når |
|---|---|---|
| Saken har ikke noe vedtak: ingenting er frosset, og alle rader kan fortsatt endres. | info | Saksvisningen har ingen vedtak. |
| N vurdering(er) og M faktum er ikke del av noen forklaringslogg, og er derfor ikke frosset. | info | Saksvisningen har vedtak, men ikke alle vurderinger og faktum er låst. |
| N vurdering(er) er eskalert til manuell behandling. | info | Minst én vurdering er eskalert. |
| Vurderinger uten konklusjon: Uavklart (1), … | info | En vurdering i treet har et annet utfall enn `Oppfylt` eller `IkkeOppfylt`. Grupperes per utfall. |
| N skjønnsvurdering(er) mangler hovedhensyn (regel 3.2). | advarsel | En vurdering av typen skjønn har ikke hovedhensyn. |
| N vurdering(er) i andre saker er sitert, ett nivå ut (regel 3.11). | info | Svaret har `refererteVurderinger`. |
| Svaret mangler oppslag for: … | advarsel | En id i svaret finnes ikke i `referansedata`, faktumlisten eller vurderingslisten. Visningen krasjer ikke, men sier fra. |

De to første gjelder bare saksvisningen. Vedtak-endepunktet er per definisjon frosset.

## Designprinsipper

- **Ingen saksspesifikk tekst.** Modulen kjenner ingen bestemt sak, regel eller kommune. Den samme koden viser alle saker.
- **All tekst kommer fra svaret.** Modulen skriver selv bare etiketter for modellens egne enumer (for eksempel `IkkeOppfylt` som «Ikke oppfylt», og ukjente verdier vises uendret) og merknadene over.
- **HTML escapes.** All tekst fra svaret escapes i `tilHtml` og `tilHtmlDokument`.
- **Ren funksjon.** Samme inn gir samme ut. Ingen DOM, ingen nettverk i modulen, ingen skjult tilstand og ingen avhengigheter. Nettverkskallet for `--url` ligger i `cli.mjs`.
- **Tre steg som kan brukes hver for seg.** Normalisering, visningsmodell og format er adskilt, så et nytt format ikke trenger å vite noe om API-svaret.
- **Mangler gir merknad, ikke krasj.** Et oppslag som ikke finnes i svaret gir en advarsel i visningen.

## Kjøre testene

Fra `visning/`:

```bash
cd visning
npm test
# eller direkte:
node --test test/*.test.mjs
```

Det er 15 tester med `node:test`. De leser de innspilte svarene fra `site/eksempel/data` (via `index.json`) og dekker blant annet sak med og uten vedtak, vedtak-endepunktet, pakket mot rått svar, merknadene, språkvalg, escaping av HTML, manglende oppslag, treet bygget fra flat liste, format-bryteren og kommandolinjen. Ingenting må installeres.

CI kjører de samme testene i [`.github/workflows/visning-test.yml`](../.github/workflows/visning-test.yml) når `visning/**`, `site/assets/js/**` eller `site/eksempel/data/**` endres.

## Utvide

Et nytt format er en ny funksjon over visningsmodellen, ikke en endring i parseren:

1. Skriv `tilNoeNytt(vm, valg = {})` i `forklaring-visning.mjs`. Den leser bare feltene i visningsmodellen (se tabellene over) og returnerer en streng.
2. Eksporter funksjonen, og legg et `case` for den i `vis`-funksjonen så den kan velges med et formatnavn.
3. Legg formatnavnet til i hjelpeteksten i `visning/cli.mjs`. Selve flagget `-f` sender navnet videre til `vis`.
4. Legg til en test i `visning/test/visning.test.mjs`.

Trenger formatet noe som visningsmodellen ikke har, utvider du `byggVisning` og tester det der. Hold da prinsippene over: ingen saksspesifikk tekst, og ingen tekst som ikke kommer fra svaret eller regnes ut av strukturen.
