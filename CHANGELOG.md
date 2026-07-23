# Changelog

Alle vesentlige endringer i dette prosjektet dokumenteres i denne filen.

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
