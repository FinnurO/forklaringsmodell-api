# Changelog

Alle vesentlige endringer i dette prosjektet dokumenteres i denne filen.

## [1.8.0] — Standard lesevisning (parser)

### Lagt til

- **`site/assets/js/forklaring-visning.mjs`**: ren ES-modul uten avhengigheter og uten DOM, som kjører likt i nettleser og Node. Den gjør svaret fra `GET /api/saker/{sakId}/forklaring` (og `GET /api/vedtak/{id}/forklaring`) om til en standard, lesbar visning. Tre steg: `normaliser(json)` (samme form uansett endepunkt, godtar også pakket `{ method, path, response }`), `byggVisning(json, { sprak })` (visningsmodell med tittel, sak, tall, vedtak, vurderinger som tre, faktum, partsmedvirkninger, relasjoner, kryss-sak, referanser og merknader) og formater: `tilMarkdown`, `tilTekst`, `tilHtml` (fragment med klasser `fv-*`), `visningCss`, `tilHtmlDokument` og `vis(json, format)`.
- **Lesbarhet fra data, ikke fra håndskrevne setninger**: all tekst kommer fra svaret. Modulen skriver bare etiketter for modellens egne enumer og merknader som regnes ut av strukturen: ingen vedtak (ingenting er frosset), N eskalerte, vurderinger uten konklusjon, skjønn uten hovedhensyn (regel 3.2), sitert kryss-sak (regel 3.11) og manglende oppslag i svaret. HTML-utdata escapes.
- **Flerspråklige tekster**: velger ønsket språk med reserve og viser øvrige varianter (kan slås av).
- **`visning/cli.mjs`**: `node visning/cli.mjs <fil|-> [--url <adresse>] [-f md|txt|html|dokument|json] [--sprak nb|nn] [--en-sprak] [-o fil]`. Leser fra fil, standard inn eller direkte fra et kjørende API.
- **`visning/README.md`**: bruk i nettleser, Node og kommandolinje, visningsmodellens felter, merknadene og hvordan man utvider med nye formater.
- **`.github/workflows/visning-test.yml`**: kjører testene på push og pull request når `visning/**`, `site/assets/js/**` eller `site/eksempel/data/**` endres.

### Endret

- **Eksempeldataene er spilt inn på nytt** (28 innspilte kall). Sak B har nå tre delvurderinger. Tidligere hadde den to, mens rotens beregningsspor sa «ett av tre delvilkår».
- **Eksempelsiden tegnes nå av parseren** i stedet for av håndskrevet rendering.

### Migrasjon

- Ingen endring i API-et og ingen databasemigrasjon. Ingen ny forretningsregel (antallet er fortsatt 18).

### Tester

- `visning/test/visning.test.mjs` og `visning/package.json`: 15 tester med `node:test`, kjørt med `npm test` i `visning/` (eller `node --test test/*.test.mjs`). De går mot de innspilte, ekte svarene i `site/eksempel/data`.

## [1.7.0] — Samlet forklaring av en sak

### Lagt til

- **Nytt endepunkt `GET /api/saker/{sakId}/forklaring`**: hele saken levende i ett svar, også når saken ikke har vedtak (f.eks. en sak rutet til manuell behandling). Svaret inneholder `sak`, `oppsummering` (`harVedtak`, `antallVedtak`, `antallFaktum`, `antallVurderinger`, `antallEskalerteVurderinger`), `relasjoner`, `faktum`, `vurderinger` (flat liste), `vurderingstre` (samme rader nøstet), `partsmedvirkninger`, `vedtak` (liste der hvert element har `vedtak`, `forklaringslogg` og `virkninger`), `refererteVurderinger`, `andreFaktum` og `referansedata`. `erLaast` på hver rad viser hva som er frosset av et vedtak (beregnet fra forklaringsloggene). 404 hvis saken ikke finnes.
- **`vurderingstre`**: vurderingene nøstet via `forelderVurderingId` (regel 3.18). Hver node har alle `VurderingDto`-feltene pluss `delvurderinger`, slik at en visning kan gå rett fra tre til skjerm.
- **`referansedata`**: oppløste `kilder`, `rettskilder`, `regler` og `vilkar` som er referert i svaret, inkludert rettskilder som kilder, regler og vilkår selv peker på, og regelen et vilkår peker på. Klienten slipper egne oppslag.
- **`refererteVurderinger`**: vurderinger i *andre*, frosne saker som vurderingene her bygger på (regel 3.11). Følges ett nivå.
- **`andreFaktum`**: faktum som er referert fra vurderinger eller virkninger, men ikke er oppført i forklaringsloggen (typisk fra en annen sak).

### Endrede API-kontrakter

- `GET /api/vedtak/{id}/forklaring` berikes med `vurderingstre`, `referansedata`, `refererteVurderinger` og `andreFaktum`. Bakoverkompatibelt: alle eksisterende felt er uendret, kun nye felt er lagt til. Svaret er fortsatt det *frosne* øyeblikksbildet: kun det forklaringsloggen peker på, pluss oppløste referanser.
- De to visningene har ulik rolle: vedtak-forklaringen er det frosne øyeblikksbildet, sak-forklaringen er den levende saksvisningen.

### Avveininger

- Svaret fra `GET /api/saker/{sakId}/forklaring` kan bli stort for store saker.
- Vurderingene finnes både flatt (`vurderinger`) og nøstet (`vurderingstre`). Det er de samme radene: enklere for klienten, men gir dobbel nyttelast.

### Migrasjon

- Ingen databasemigrasjon og ingen brytende endring. Ingen ny forretningsregel (antallet er fortsatt 18).

### Internt

- Entitet-til-DTO-mappingen er samlet i `DtoMapper`, og alle services og begge forklaringsvisningene bruker den. Tidligere lå en egen, manuelt vedlikeholdt projeksjon i `GetForklaringAsync`, og den falt flere ganger ut av takt med DTO-ene når nye felt ble lagt til. Forklaringen settes nå sammen av `ForklaringService` i stedet for `VedtakService`.

### Tester

- Nye tester for begge forklaringsvisningene (vurderingstre, referansedata, kryss-sak-referanser, sak uten vedtak og 404).

## [1.6.0] — Intra-sak vurderingshierarki og vilkårsmerking

### Lagt til

- **`Vurdering.VilkarId`** (ny, valgfri FK til `Vilkar`): ren sporbarhetsmerking av hvilket katalogvilkår en vurdering gjelder — ingen håndhevet validering, samme kategori som `RettskildeIder`.
- **`Vurdering.ForelderVurderingId`** (ny, valgfri selvreferanse, intra-sak): lar en vurdering være en delvurdering av en annen, i samme (fortsatt åpne) sak. Forelderen må tilhøre samme `Sak`, men trenger ikke være frosset — til forskjell fra `RefererteVurderingIder` (regel 3.11), som kun peker til frosne rader i *andre* saker. Modellen håndhever bevisst ingen fast nedbrytingsmal: det er det kallende systemet som avgjør hvordan et vilkår faktisk ble brutt ned, og modellen gjengir det trofast (regel 3.18).
- **`VurderingDto.DelvurderingIder`** (ny, beregnet): lar en klient lese ut hele treet av delvurderinger uten å måtte liste alle vurderinger i saken og filtrere selv.

### Endrede API-kontrakter

- `OpprettVurderingDto` tar nå imot valgfrie `vilkarId` og `forelderVurderingId`.
- `VurderingDto` returnerer nå `vilkarId`, `forelderVurderingId` og `delvurderingIder`.

### Migrasjon

- Ny EF Core-migrasjon (`VurderingHierarkiOgVilkarReferanse`) legger til `VilkarId`/`ForelderVurderingId` på `Vurderinger` — rent additiv, ingen datatap.

### Seed-data

- `vurderingDeterministisk` merkes med `VilkarId` mot `DP_SATS_INNTEKT`-vilkåret, og får en ny delvurdering (`Uaktuelt`, siste 36 måneders inntektsgrunnlag) som demonstrerer treet ende-til-ende.

### Tester

- Nye tester for `VilkarId`-eksistenssjekk, delvurdering-lenking og at `ForelderVurderingId` avvises på tvers av saker.

## [1.5.0] — Flerspråklige forklaringstekster

### Lagt til

- **Ny entitet `FlerspraakligTekst` + `TekstVariant`**: en gjenbrukbar beholder for tekst på flere språk (`SpraakKode` er en fri streng, ikke en enum — nye språk er bare nye rader, ikke en skjemaendring). `Vilkar.StandardTekst`, `Vurdering.Hovedhensyn`/`ForkastedeUtfall` og `Vedtaksvirkning.Beskrivelse`/`LopendeVilkar` peker nå på én `FlerspraakligTekst` hver, i stedet for å være plain `string`-felt (regel 3.17). Interne/tekniske felt (`Sak.Tittel`, `Vedtak.Utfall`, `Faktum.Verdi`, `Vurdering.Beregningsspor` m.fl.) er bevisst holdt utenfor — kun det som faktisk inngår i begrunnelsen overfor en part er flerspråkliggjort.
- Enkel HTML/JS-utforskerside (`wwwroot/index.html`), servert sammen med API-et via `UseStaticFiles`. Lister saker, viser vedtak per sak, og henter hydrert forklaring — samt et skjema for å opprette nye saker (nyttig for å lage flere "seeds" manuelt).
- Nytt endepunkt `GET /api/saker/{sakId}/vedtak` (samme mønster som de andre `Sak{Entitet}`-listene) — nødvendig for at siden skal kunne oppdage et vedtaks ID uten at det allerede er kjent.

### Endrede API-kontrakter

- `standardTekst` (vilkår), `hovedhensyn`/`forkastedeUtfall` (vurderinger) og `beskrivelse`/`lopendeVilkar` (virkninger) tar nå imot/returnerer en liste av `{ spraakKode, verdi }` i stedet for en enkelt streng.

### Fikset

- `GET /api/vedtak/{id}/forklaring` manglet flere felt i sin hydrerte `Faktum`/`Vurdering`-projeksjon som var lagt til `FaktumDto`/`VurderingDto` i tidligere versjoner (`RettskildeIder` fra v1.1, `RefererteVurderingIder` fra v1.2, `Utfall` fra v1.4) — feltene ble aldri mappet inn i denne spesifikke responsen, og viste derfor stille default-verdier (f.eks. `Utfall: Oppfylt` for alle vurderinger uansett faktisk verdi). Oppdaget under manuell verifisering av utforskersiden.

### Migrasjon

- Ny EF Core-migrasjon (`FlerspraakligTekst`) oppretter `FlerspraakligeTekster`/`TekstVarianter`, og erstatter de fem gamle `string`-kolonnene med `Guid`/`Guid?`-fremmednøkler.

### Seed-data

- Skjønnsvurderingens `Hovedhensyn` seedes med både `nb`- og `nn`-variant, for å bevise flerspråkligheten ende-til-ende; øvrige forklaringstekster får kun `nb` foreløpig.

### Tester

- Nye validatortester for regel 3.17 (duplikate språkkoder avvises, variant uten verdi avvises, case-insensitiv dedup).

## [1.4.0] — Utfallstyper og vilkårets rettslige/interne/tekniske grunnlag

### Endret domenemodell

- **`Vurdering.Utfall`** (ny, obligatorisk, `UtfallType`: `Oppfylt`, `IkkeOppfylt`, `Uaktuelt`, `IkkeVurdert`, `Uavklart`): en `Vurdering`-rad skal opprettes selv når vilkåret faktisk ikke ble vurdert — fraværet av en rad skal aldri være den eneste dokumentasjonen på det.
- **`Vilkar.Grunnlagstype`** (ny, obligatorisk, `GrunnlagsType`: `Rettslig`, `InternPraksis`, `Datakvalitet`): skiller vilkår forankret i en rettskilde fra vilkår forankret i forvaltningspraksis eller tekniske datakvalitetskontroller.
- **`Vilkar.Kode`/`Vilkar.Kodeverk`** (nye, valgfrie): strukturert kode fra et kildesystems eget kodeverk (f.eks. NAVs `VILKAR_TYPE`), for maskinell matching mot kildesystemet.
- **`Vilkar.CpsvTjenesteReferanse`** (ny, valgfri): IRI til hvilken(e) CPSV-AP-NO-tjeneste(r) vilkåret kan inngå i.
- **`Regel.RegeldefinisjonReferanse`** (ny, valgfri): ekstern URI til selve regelartefaktet (f.eks. DMN-XML i et regelrepo) — en pekepinn, ikke en kopi; regelmotorens eget lagringsansvar dupliseres ikke.

### Nye forretningsregler

- **Regel 3.14**: `Vurdering.Utfall` skiller reell manglende vurdering (`Uaktuelt`, `IkkeVurdert`) fra reelle konklusjoner (`Oppfylt`/`IkkeOppfylt`) og lav-konfidens-resultater (`Uavklart`). Årsaken skal alltid fremgå av `Beregningsspor`.
- **Regel 3.15**: `Vilkar` med `Grunnlagstype == Rettslig` må ha minst én `RettskildeIder`; validert på API-nivå. `InternPraksis`/`Datakvalitet` krever det ikke.
- **Regel 3.16**: `Regel.RegeldefinisjonReferanse` er en ekstern, ikke-validert pekepinn — samme mønster som CPSV-AP-NO/CCCEV-referansene i regel 3.9.

### Migrasjon

- Ny EF Core-migrasjon legger til `Utfall`-kolonnen på `Vurderinger`, `RegeldefinisjonReferanse` på `Regler`, og `Kode`/`Kodeverk`/`Grunnlagstype`/`CpsvTjenesteReferanse` på `Vilkar`.

### Seed-data

- Alle tre dagpenger-vurderingene får satt `Utfall` i tråd med det oppdaterte eksempelet i punkt 6 (`Oppfylt`/`Uavklart`/`Oppfylt`).
- Dagpengesats-vilkåret får `Grunnlagstype: Rettslig` (har allerede en rettskilde) og `Kode`/`Kodeverk`.
- To nye, ureferert `Vurdering`-rader demonstrerer `Uaktuelt` og `IkkeVurdert` (regel 3.14), og en ny `Vilkar` med `Grunnlagstype: Datakvalitet` og uten `RettskildeIder` demonstrerer unntaket i regel 3.15.

## [1.3.0] — Vilkårskatalog og avledede virkninger

### Endret domenemodell

- **Ny entitet `Vilkar`**: en gjenbrukbar referansetabell (som `Regel`/`Rettskilde`/`Kilde`) for vilkårsdefinisjoner som går igjen på tvers av mange vedtak — fra statiske standardvilkår til parametriserte eller skjønnsbaserte vilkårstyper. Koblet til `Rettskilde` (mange-til-mange) og valgfritt til `Regel`.
- **`Vedtaksvirkning.VilkarId`** (ny, valgfri): kobler en virkning til en katalogført `Vilkar`, uten at senere endringer i katalogoppføringen påvirker allerede opprettede virkninger (append-only-prinsippet gjelder også her).
- **`Vedtaksvirkning.Fastsettelsesmate`** (ny, `FastsettelsesmateType`: `Statisk`, `Parametrisert`, `Skjonnsbasert`, `Avledet`): hvordan virkningens innhold ble fastsatt.
- **`Vedtaksvirkning.AvledetFraVirkningId`** (ny, valgfri selvreferanse): lar en virkning være avledet av en annen — kan peke på tvers av `Vedtak` og `Sak` (f.eks. en åpningstid låst til en skjenkebevillings skjenketid).
- **`VirkningType.Gebyr`** (ny enum-verdi): for virkninger der beløpet går *fra* mottaker, i motsetning til `OkonomiskYtelse`/`Tilskudd`.

### Nye forretningsregler

- **Regel 3.12**: `Vilkar` er referansedata — en `Vilkar`-rad referert av minst én `Vedtaksvirkning` skal ikke overskrives; endringer opprettes som en ny `Vilkar`-rad (samme append-only-mønster som `Regel`, regel 3.4).
- **Regel 3.13**: `Vedtaksvirkning.AvledetFraVirkningId` skal kun peke til en virkning som allerede er del av et frosset vedtak — samme skrivebeskyttede kryss-referanse-prinsipp som regel 3.11, nå på virkningsnivå. Siden `Vedtaksvirkning` kun kan opprettes atomisk med sitt `Vedtak` (ingen frittstående opprettelse), er "allerede eksisterer i databasen" tilstrekkelig for å bekrefte at referansen er frosset.

### Endrede API-kontrakter

- Nytt endepunkt: `GET/POST /api/vilkar`.
- `POST /api/saker/{sakId}/vedtak`: hver virkning i `virkninger`-listen tar nå imot valgfrie `vilkarId`, `fastsettelsesmate` og `avledetFraVirkningId`-felt.

### Migrasjon

- Ny EF Core-migrasjon legger til `Vilkar`-tabellen + koblingstabell mot `Rettskilde`, samt `VilkarId`/`Fastsettelsesmate`/`AvledetFraVirkningId`-kolonner på `Vedtaksvirkninger`.

### Seed-data

- Dagpenger-vedtakets virkning kobles til en ny `Vilkar`-katalogoppføring med `Fastsettelsesmate: Parametrisert`.
- Et nytt, andre vedtak demonstrerer `AvledetFraVirkningId` på tvers av vedtak.

## [1.2.0] — Vedtaksvirkninger, saksrelasjoner og kryss-sak-referanser

### Endret domenemodell

- **Ny entitet `Vedtaksvirkning`**: et `Vedtak` kan nå medføre flere, uavhengig tidsbegrensede virkninger (`VirkningType`: `Tillatelse`, `Plikt`, `OkonomiskYtelse`, `Tilskudd`), hver med egen `VarighetsType` (`Tidsbegrenset`, `Varig`, `LopendeInntilVilkarBrister`), gyldighetsperiode, beløp og sporbar kobling til hvilke `Vurdering`/`Faktum`-rader som fastsatte den.
- **Ny entitet `SakRelasjon`**: kobler en ny/oppfølgende `Sak` til en relatert `Sak` (`SakRelasjonType`: `Tilbakekall`, `Revurdering`, `OppfolgingAvMelding`, `Klage`, `Kontroll`, `Annet`), uten å modifisere den opprinnelige saken. Modellen modellerer bevisst **ikke** saksflyt/tilstandsoverganger — en ny hendelse gir alltid en ny `Sak`.
- **`Sak.UtlosendeHendelse`** (ny, obligatorisk): `HendelseType` (`Soknad`, `Innrapportering`, `Melding`, `Tilbakekall`, `Kontroll`, `Klage`, `Omgjoring`) som merker hvilken CPSV-AP-hendelse som utløste saken.
- **`Vurdering.RefererteVurderingIder`** (ny, valgfri, mange-til-mange selvreferanse): lar en vurdering eksplisitt bygge på en (allerede frosset) vurdering fra en *annen* sak, uten å gjøre den om.
- **`Vurdering.FaktumIder` kan nå peke til `Faktum` i en annen `Sak`** — tidligere implisitt begrenset til samme sak.
- **Feltomdøping for CPSV-AP-NO/CCCEV-samsvar**: `Sak.TjenesteReferanse` → `Sak.CpsvTjenesteReferanse`, `Kilde.CpsvReferanse` → `Kilde.CccevReferanse`, `Regel.CpsvRuleReferanse` → `Regel.CpsvRegelReferanse`.

### Nye forretningsregler

- **Regel 3.10**: `Vedtaksvirkning` er append-only på samme måte som `Forklaringslogg` (ingen `PUT`/`DELETE` etter opprettelse). `GyldigTil` skal være `null` når `Varighet == Varig`. `RapporteringsFrekvens` skal kun være satt når `Type == Plikt`.
- **Regel 3.11**: Kryss-sak-referanser (`Vurdering.RefererteVurderingIder`, og `Vurdering.FaktumIder`/`RettskildeIder` som peker til en annen sak) skal kun peke til rader som allerede er del av en frosset `Forklaringslogg` i den relaterte saken — en vurdering kan lese fra en annen sak, men skal aldri kunne endre den. Validert på API-nivå (409/423 ved brudd, samme mønster som append-only for øvrig).

### Endrede API-kontrakter

- `POST /api/saker`: nytt obligatorisk `utlosendeHendelse`-felt; `tjenesteReferanse` → `cpsvTjenesteReferanse`.
- `POST /api/kilder`: `cpsvReferanse` → `cccevReferanse`.
- `POST /api/regler`: `cpsvRuleReferanse` → `cpsvRegelReferanse`.
- `POST /api/saker/{sakId}/vurderinger`: nytt valgfritt `refererteVurderingIder`-felt.
- `POST /api/saker/{sakId}/vedtak`: nytt valgfritt `virkninger`-felt (liste av virkningsobjekter), opprettet i samme transaksjon som vedtaket.
- Nye endepunkter: `GET/POST /api/saker/{sakId}/relasjoner`, `GET /api/vedtak/{id}/virkninger`.
- `GET /api/vedtak/{id}/forklaring` inkluderer nå virkninger i den hydrerte responsen.

### Migrasjon

- Ny EF Core-migrasjon legger til `SakRelasjoner`, `Vedtaksvirkninger` + to koblingstabeller (`VedtaksvirkningVurdering`, `VedtaksvirkningFaktum`), en selvrefererende koblingstabell for `Vurdering.RefererteVurderingIder`, samt de omdøpte/nye kolonnene på `Sak`/`Kilde`/`Regel`.

### Seed-data

- Dagpenger-vedtaket får en `OkonomiskYtelse`-virkning (fra spesifikasjonens `POST .../vedtak`-eksempel i punkt 5).
- Ny, andre `Sak` (`utlosendeHendelse: Melding`) demonstrerer `SakRelasjon` (`OppfolgingAvMelding`) og `Vurdering.RefererteVurderingIder` mot den opprinnelige skjønnsvurderingen, i tråd med meldingseksempelet i spesifikasjonens punkt 6.

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
