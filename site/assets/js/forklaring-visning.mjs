/**
 * Standard lesevisning av forklaringsmodellen.
 *
 * Tar svaret fra GET /api/saker/{sakId}/forklaring (eller GET /api/vedtak/{id}/forklaring) som
 * JSON og lager en lesbar, strukturert visning. Modulen er ren funksjon over data: den har ingen
 * avhengigheter, ingen DOM og ingen kunnskap om en bestemt sak, så den kjører likt i nettleser og
 * i Node. All tekst som vises kommer fra svaret. Det eneste modulen selv skriver er etiketter for
 * modellens egne enumer og merknader som regnes ut av strukturen (se `lagMerknader`).
 *
 * Tre steg, slik at hvert steg kan brukes og testes for seg:
 *   1. normaliser(json)        -> felles form, uansett hvilket endepunkt svaret kommer fra
 *   2. byggVisning(json, valg) -> visningsmodellen: lesbar struktur uten noe format
 *   3. tilMarkdown / tilTekst / tilHtml / tilHtmlDokument -> utdata
 */

export const VISNINGSVERSJON = 1;

// ---------------------------------------------------------------------------------------------
// Etiketter for modellens egne enumer. Ukjente verdier vises uendret.
// ---------------------------------------------------------------------------------------------

const ETIKETTER = {
  utfall: { Oppfylt: 'Oppfylt', IkkeOppfylt: 'Ikke oppfylt', Uaktuelt: 'Uaktuelt', IkkeVurdert: 'Ikke vurdert', Uavklart: 'Uavklart' },
  vurderingstype: { Deterministisk: 'Deterministisk regel', GenerativKI: 'Generativ KI', Skjonn: 'Skjønn' },
  automatiseringsgrad: { Helautomatisert: 'Helautomatisert', DelvisAutomatisert: 'Delvis automatisert', Manuell: 'Manuell' },
  virkningstype: { Tillatelse: 'Tillatelse', Plikt: 'Plikt', OkonomiskYtelse: 'Økonomisk ytelse', Tilskudd: 'Tilskudd', Gebyr: 'Gebyr' },
  varighet: { Varig: 'Varig', Tidsbegrenset: 'Tidsbegrenset', LopendeInntilVilkarBrister: 'Løpende inntil vilkår brister' },
  sakstatus: { UnderBehandling: 'Under behandling', Avsluttet: 'Avsluttet', Klaget: 'Klaget' },
  faktumtype: { Raatt: 'Rått', Subsumert: 'Subsumert' },
  hendelse: { Soknad: 'søknad', Innrapportering: 'innrapportering', Melding: 'melding', Tilbakekall: 'tilbakekall', Kontroll: 'kontroll', Klage: 'klage', Omgjoring: 'omgjøring' },
};

export function etikett(gruppe, verdi) {
  if (verdi == null || verdi === '') return '';
  return (ETIKETTER[gruppe] && ETIKETTER[gruppe][verdi]) || String(verdi);
}

/** Utfall som er en konklusjon, kontra «ikke avgjort» (regel 3.14). */
const KONKLUSJON = new Set(['Oppfylt', 'IkkeOppfylt']);

// ---------------------------------------------------------------------------------------------
// Små hjelpere
// ---------------------------------------------------------------------------------------------

const liste = (x) => (Array.isArray(x) ? x : []);
const per = (rader, nokkel) => Object.fromEntries(liste(rader).map((r) => [r[nokkel], r]));

/** Velger tekst på ønsket språk, med reserve; returnerer også de øvrige variantene. */
export function velgTekst(varianter, sprak = 'nb') {
  const v = liste(varianter).filter((x) => x && x.verdi);
  if (!v.length) return null;
  const hoved = v.find((x) => x.spraakKode === sprak) || v.find((x) => x.spraakKode === 'nb') || v[0];
  return {
    tekst: hoved.verdi,
    sprak: hoved.spraakKode,
    andre: v.filter((x) => x !== hoved).map((x) => ({ sprak: x.spraakKode, tekst: x.verdi })),
  };
}

const idForkortet = (id) => (id ? String(id).slice(0, 8) : '');

// ---------------------------------------------------------------------------------------------
// Steg 1: normaliser
// ---------------------------------------------------------------------------------------------

/**
 * Godtar svaret slik API-et leverer det, eller pakket som {method, path, response}
 * (slik filene på eksempelsiden er lagret). Returnerer samme form uansett endepunkt.
 */
export function normaliser(input) {
  const rot = input && input.response && typeof input.response === 'object' ? input.response : input;
  if (!rot || typeof rot !== 'object') throw new Error('Forventet et JSON-objekt fra forklaring-endepunktet.');

  if (rot.sak && rot.oppsummering) {
    return {
      endepunkt: 'sak',
      sak: rot.sak,
      relasjoner: liste(rot.relasjoner),
      faktum: liste(rot.faktum),
      vurderinger: liste(rot.vurderinger),
      vurderingstre: liste(rot.vurderingstre),
      partsmedvirkninger: liste(rot.partsmedvirkninger),
      vedtak: liste(rot.vedtak),
      refererteVurderinger: liste(rot.refererteVurderinger),
      andreFaktum: liste(rot.andreFaktum),
      referansedata: rot.referansedata || {},
    };
  }

  if (rot.vedtak && rot.forklaringslogg) {
    return {
      endepunkt: 'vedtak',
      sak: null,
      relasjoner: [],
      faktum: liste(rot.faktum),
      vurderinger: liste(rot.vurderinger),
      vurderingstre: liste(rot.vurderingstre),
      partsmedvirkninger: liste(rot.partsmedvirkninger),
      vedtak: [{ vedtak: rot.vedtak, forklaringslogg: rot.forklaringslogg, virkninger: liste(rot.virkninger) }],
      refererteVurderinger: liste(rot.refererteVurderinger),
      andreFaktum: liste(rot.andreFaktum),
      referansedata: rot.referansedata || {},
    };
  }

  throw new Error('Kjenner ikke igjen formen. Gi svaret fra GET /api/saker/{sakId}/forklaring eller GET /api/vedtak/{id}/forklaring.');
}

// ---------------------------------------------------------------------------------------------
// Steg 2: visningsmodell
// ---------------------------------------------------------------------------------------------

/**
 * @param {object} input svar fra et forklaring-endepunkt
 * @param {{sprak?: string}} [valg]
 */
export function byggVisning(input, valg = {}) {
  const sprak = valg.sprak || 'nb';
  const d = normaliser(input);
  const rd = d.referansedata;

  const kilder = per(rd.kilder, 'kildeId');
  const rettskilder = per(rd.rettskilder, 'rettskildeId');
  const regler = per(rd.regler, 'regelId');
  const vilkar = per(rd.vilkar, 'vilkarId');
  const faktumPerId = per([...d.faktum, ...d.andreFaktum], 'faktumId');
  const vurderingPerId = per([...d.vurderinger, ...d.refererteVurderinger], 'vurderingId');
  const manglende = [];

  const slaaOpp = (kart, id, hva) => {
    if (id == null) return null;
    const r = kart[id];
    if (!r) manglende.push(`${hva} ${idForkortet(id)}`);
    return r || null;
  };

  const faktumVisning = (f) => {
    const k = slaaOpp(kilder, f.kildeId, 'kilde');
    return {
      id: f.faktumId,
      verdi: f.verdi,
      type: etikett('faktumtype', f.type),
      struktur: f.struktur,
      innhentet: f.innhentetTidspunkt || null,
      kilde: k ? { navn: k.navn, autoritativ: !!k.autoritativ } : null,
      laast: !!f.erLaast,
      annenSak: d.sak ? f.sakId !== d.sak.sakId : false,
      avledetFra: f.avledetFraFaktumId || null,
    };
  };

  const hjemmel = (id) => {
    const r = slaaOpp(rettskilder, id, 'rettskilde');
    return r ? { henvisning: r.henvisning, type: r.type, eli: r.eliReferanse || null } : null;
  };

  const noder = (n) => {
    const v = vilkarId(n.vilkarId);
    const rg = n.regelId ? slaaOpp(regler, n.regelId, 'regel') : null;
    return {
      id: n.vurderingId,
      type: etikett('vurderingstype', n.type),
      utfall: n.utfall,
      utfallTekst: etikett('utfall', n.utfall),
      erKonklusjon: KONKLUSJON.has(n.utfall),
      eskalert: !!n.eskalert,
      laast: !!n.erLaast,
      konfidens: n.konfidens == null ? null : n.konfidens,
      beregningsspor: n.beregningsspor || null,
      hovedhensyn: velgTekst(n.hovedhensyn, sprak),
      forkastedeUtfall: velgTekst(n.forkastedeUtfall, sprak),
      vilkar: v,
      regel: rg ? { teknologi: rg.teknologi, type: etikett('vurderingstype', rg.type), referanse: rg.regeldefinisjonReferanse || null } : null,
      faktum: liste(n.faktumIder).map((id) => slaaOpp(faktumPerId, id, 'faktum')).filter(Boolean).map(faktumVisning),
      hjemler: liste(n.rettskildeIder).map(hjemmel).filter(Boolean),
      siterer: liste(n.refererteVurderingIder).map((id) => {
        const r = slaaOpp(vurderingPerId, id, 'vurdering');
        return r ? { id: r.vurderingId, utfall: etikett('utfall', r.utfall), beregningsspor: r.beregningsspor || null, annenSak: d.sak ? r.sakId !== d.sak.sakId : false } : null;
      }).filter(Boolean),
      barn: liste(n.delvurderinger).map(noder),
    };
  };

  function vilkarId(id) {
    const v = slaaOpp(vilkar, id, 'vilkår');
    return v ? { navn: v.navn, kode: v.kode || null, kodeverk: v.kodeverk || null, tekst: velgTekst(v.standardTekst, sprak) } : null;
  }

  // Treet: bruk det nøstede treet når det finnes, ellers bygg det fra den flate listen.
  const tre = d.vurderingstre.length ? d.vurderingstre : nostFraFlat(d.vurderinger);
  const vurderinger = tre.map(noder);
  const alleNoder = [];
  const gaa = (n) => { alleNoder.push(n); n.barn.forEach(gaa); };
  vurderinger.forEach(gaa);

  const vedtak = d.vedtak.map((v) => {
    const opp = liste(v.forklaringslogg && v.forklaringslogg.oppforinger);
    const tell = (t) => opp.filter((o) => o.type === t).length;
    return {
      id: v.vedtak.vedtakId,
      utfall: v.vedtak.utfall,
      tidspunkt: v.vedtak.tidspunkt,
      automatiseringsgrad: etikett('automatiseringsgrad', v.vedtak.automatiseringsGrad),
      logg: { oppforinger: opp.length, faktum: tell('Faktum'), vurderinger: tell('Vurdering'), partsmedvirkninger: tell('Partsmedvirkning') },
      virkninger: liste(v.virkninger).map((x) => ({
        type: etikett('virkningstype', x.type),
        varighet: etikett('varighet', x.varighet),
        fastsettelsesmate: x.fastsettelsesmate || null,
        beskrivelse: velgTekst(x.beskrivelse, sprak),
        lopendeVilkar: velgTekst(x.lopendeVilkar, sprak),
        gyldigFra: x.gyldigFra || null,
        gyldigTil: x.gyldigTil || null,
        belop: x.belop == null ? null : x.belop,
        vilkar: x.vilkarId ? vilkarId(x.vilkarId) : null,
      })),
    };
  });

  const faktum = d.faktum.map(faktumVisning);
  const antallLaastFaktum = d.faktum.filter((f) => f.erLaast).length;
  const antallLaastVurd = d.vurderinger.filter((v) => v.erLaast).length;

  const tall = {
    vedtak: vedtak.length,
    faktum: d.faktum.length,
    faktumLaast: antallLaastFaktum,
    vurderinger: d.vurderinger.length,
    vurderingerLaast: antallLaastVurd,
    eskalerte: d.vurderinger.filter((v) => v.eskalert).length,
    rotvurderinger: vurderinger.length,
  };

  const visning = {
    versjon: VISNINGSVERSJON,
    endepunkt: d.endepunkt,
    sprak,
    tittel: d.sak ? d.sak.tittel : `Vedtak: ${vedtak[0] ? vedtak[0].utfall : ''}`.trim(),
    sak: d.sak
      ? { id: d.sak.sakId, status: etikett('sakstatus', d.sak.status), hendelse: etikett('hendelse', d.sak.utlosendeHendelse) || null, opprettet: d.sak.opprettet || null, tjenesteReferanse: d.sak.cpsvTjenesteReferanse || null }
      : null,
    tall,
    vedtak,
    vurderinger,
    faktum,
    partsmedvirkninger: d.partsmedvirkninger.map((p) => ({ type: p.type, tidspunkt: p.tidspunkt, innhold: p.innhold })),
    relasjoner: d.relasjoner.map((r) => ({ type: r.type, relatertSakId: r.relatertSakId })),
    kryssSak: {
      vurderinger: d.refererteVurderinger.map((v) => ({ id: v.vurderingId, sakId: v.sakId, utfall: etikett('utfall', v.utfall), beregningsspor: v.beregningsspor || null, laast: !!v.erLaast })),
      faktum: d.andreFaktum.map(faktumVisning),
    },
    referanser: {
      kilder: liste(rd.kilder).map((k) => ({ navn: k.navn, type: k.type, autoritativ: !!k.autoritativ })),
      rettskilder: liste(rd.rettskilder).map((r) => ({ henvisning: r.henvisning, type: r.type, eli: r.eliReferanse || null })),
      regler: liste(rd.regler).map((r) => ({ teknologi: r.teknologi, type: etikett('vurderingstype', r.type), referanse: r.regeldefinisjonReferanse || null })),
      vilkar: liste(rd.vilkar).map((v) => ({ navn: v.navn, kode: v.kode || null, kodeverk: v.kodeverk || null, tekst: velgTekst(v.standardTekst, sprak) })),
    },
    merknader: [],
  };

  visning.merknader = lagMerknader(d, visning, alleNoder, [...new Set(manglende)]);
  return visning;
}

/** Bygger et tre fra en flat liste via forelderVurderingId, for svar uten ferdig nøstet tre. */
function nostFraFlat(flat) {
  const kart = new Map(flat.map((v) => [v.vurderingId, { ...v, delvurderinger: [] }]));
  const røtter = [];
  for (const v of flat) {
    const node = kart.get(v.vurderingId);
    const forelder = v.forelderVurderingId && kart.get(v.forelderVurderingId);
    (forelder ? forelder.delvurderinger : røtter).push(node);
  }
  return røtter;
}

/**
 * Merknader regnes ut av strukturen alene, uten kunnskap om saken. De sier noe om hva som
 * er frosset, hva som er eskalert og om svaret henger sammen. Tekstene er faste, men hver
 * merknad kommer kun når dataene tilsier det.
 */
function lagMerknader(d, v, alleNoder, manglende) {
  const m = [];
  const t = v.tall;

  if (d.endepunkt === 'sak') {
    if (t.vedtak === 0) {
      m.push({ niva: 'info', tekst: 'Saken har ikke noe vedtak. Ingenting er frosset, og alle rader kan fortsatt endres.' });
    } else if (t.faktumLaast < t.faktum || t.vurderingerLaast < t.vurderinger) {
      const fa = t.faktum - t.faktumLaast;
      const vu = t.vurderinger - t.vurderingerLaast;
      m.push({ niva: 'info', tekst: `${vu} vurdering(er) og ${fa} faktum er ikke del av noen forklaringslogg, og er derfor ikke frosset.` });
    }
  }

  if (t.eskalerte > 0) {
    m.push({ niva: 'info', tekst: `${t.eskalerte} vurdering(er) er eskalert til manuell behandling.` });
  }

  const ikkeAvgjort = alleNoder.filter((n) => !n.erKonklusjon);
  if (ikkeAvgjort.length) {
    const grupper = {};
    ikkeAvgjort.forEach((n) => { grupper[n.utfallTekst] = (grupper[n.utfallTekst] || 0) + 1; });
    m.push({ niva: 'info', tekst: 'Vurderinger uten konklusjon: ' + Object.entries(grupper).map(([k, n]) => `${k} (${n})`).join(', ') + '.' });
  }

  const skjonnUtenHensyn = alleNoder.filter((n) => n.type === etikett('vurderingstype', 'Skjonn') && !n.hovedhensyn);
  if (skjonnUtenHensyn.length) {
    m.push({ niva: 'advarsel', tekst: `${skjonnUtenHensyn.length} skjønnsvurdering(er) mangler hovedhensyn (regel 3.2).` });
  }

  if (v.kryssSak.vurderinger.length) {
    m.push({ niva: 'info', tekst: `${v.kryssSak.vurderinger.length} vurdering(er) i andre saker er sitert, ett nivå ut (regel 3.11).` });
  }

  if (manglende.length) {
    m.push({ niva: 'advarsel', tekst: 'Svaret mangler oppslag for: ' + manglende.join(', ') + '.' });
  }

  return m;
}

// ---------------------------------------------------------------------------------------------
// Steg 3: formater
// ---------------------------------------------------------------------------------------------

function dato(iso) {
  if (!iso) return '';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return String(iso);
  return d.toLocaleString('nb-NO', { dateStyle: 'medium', timeStyle: 'short', timeZone: 'Europe/Oslo' });
}

const kr = (n) => `${Number(n).toLocaleString('nb-NO')} kr`;

function virkningslinjer(x) {
  const del = [];
  if (x.fastsettelsesmate) del.push(`fastsatt ${x.fastsettelsesmate.toLowerCase()}`);
  if (x.gyldigFra || x.gyldigTil) del.push(`gyldig ${x.gyldigFra ? dato(x.gyldigFra) : '…'} til ${x.gyldigTil ? dato(x.gyldigTil) : 'uten sluttdato'}`);
  if (x.belop != null) del.push(kr(x.belop));
  return del;
}

function tekstMedSprak(t, alleSprak) {
  if (!t) return '';
  if (!alleSprak || !t.andre.length) return t.tekst;
  return [`${t.tekst} [${t.sprak}]`, ...t.andre.map((a) => `${a.tekst} [${a.sprak}]`)].join(' / ');
}

/** Regelen vises der den starter, ikke gjentatt på hver delvurdering som bruker samme regel. */
function sammeRegel(n, forelder) {
  return !!(forelder && forelder.regel && n.regel && forelder.regel.teknologi === n.regel.teknologi && forelder.regel.type === n.regel.type);
}

function statuslinje(n) {
  const d = [n.type];
  if (n.eskalert) d.push('eskalert');
  d.push(n.laast ? 'låst' : 'levende');
  if (n.konfidens != null) d.push(`konfidens ${n.konfidens}`);
  return d.join(' · ');
}

/** Markdown. */
export function tilMarkdown(vm, valg = {}) {
  const alle = valg.alleSprak !== false;
  const o = [];
  o.push(`# ${vm.tittel}`, '');
  if (vm.sak) {
    o.push(`*${vm.sak.status}${vm.sak.hendelse ? ` · utløst av ${vm.sak.hendelse}` : ''}${vm.sak.opprettet ? ` · opprettet ${dato(vm.sak.opprettet)}` : ''}*`, '');
  }

  o.push('## Sammendrag', '');
  o.push(`- Vedtak: ${vm.tall.vedtak}`);
  o.push(`- Faktum: ${vm.tall.faktum} (${vm.tall.faktumLaast} låst)`);
  o.push(`- Vurderinger: ${vm.tall.vurderinger} (${vm.tall.vurderingerLaast} låst, ${vm.tall.eskalerte} eskalert)`, '');

  if (vm.merknader.length) {
    o.push('## Merknader', '');
    vm.merknader.forEach((m) => o.push(`- ${m.niva === 'advarsel' ? '**Advarsel:** ' : ''}${m.tekst}`));
    o.push('');
  }

  o.push('## Vedtak', '');
  if (!vm.vedtak.length) {
    o.push('Ingen vedtak.', '');
  }
  vm.vedtak.forEach((v) => {
    o.push(`### ${v.utfall}`, '');
    o.push(`${v.automatiseringsgrad} · ${dato(v.tidspunkt)}`);
    o.push(`Forklaringsloggen frøs ${v.logg.oppforinger} oppføringer: ${v.logg.faktum} faktum, ${v.logg.vurderinger} vurderinger, ${v.logg.partsmedvirkninger} partsmedvirkning.`, '');
    v.virkninger.forEach((x) => {
      const rest = virkningslinjer(x);
      o.push(`- **${x.type}** (${x.varighet}): ${tekstMedSprak(x.beskrivelse, alle)}${rest.length ? ` — ${rest.join(', ')}` : ''}`);
      if (x.vilkar) o.push(`  - Vilkår: ${x.vilkar.navn}${x.vilkar.kode ? ` (${x.vilkar.kode})` : ''}`);
    });
    o.push('');
  });

  o.push('## Vurderinger', '');
  const skriv = (n, dyp, forelder) => {
    const inn = '  '.repeat(dyp);
    o.push(`${inn}- **${n.utfallTekst}** — ${statuslinje(n)}`);
    if (n.beregningsspor) o.push(`${inn}  ${n.beregningsspor}`);
    if (n.hovedhensyn) o.push(`${inn}  Hovedhensyn: ${tekstMedSprak(n.hovedhensyn, alle)}`);
    if (n.forkastedeUtfall) o.push(`${inn}  Forkastede utfall: ${tekstMedSprak(n.forkastedeUtfall, alle)}`);
    if (n.vilkar && dyp === 0) o.push(`${inn}  Vilkår: ${n.vilkar.navn}${n.vilkar.kode ? ` (${n.vilkar.kode})` : ''}`);
    if (n.regel && !sammeRegel(n, forelder)) o.push(`${inn}  Regel: ${n.regel.teknologi} (${n.regel.type})`);
    n.faktum.forEach((f) => o.push(`${inn}  Faktum: ${f.verdi}${f.kilde ? ` (${f.kilde.navn})` : ''}${f.annenSak ? ' [annen sak]' : ''}`));
    n.hjemler.forEach((h) => o.push(`${inn}  Hjemmel: ${h.henvisning}`));
    n.siterer.forEach((s) => o.push(`${inn}  Bygger på: ${s.utfall}${s.beregningsspor ? ` — ${s.beregningsspor}` : ''}${s.annenSak ? ' [annen sak]' : ''}`));
    n.barn.forEach((b) => skriv(b, dyp + 1, n));
  };
  if (!vm.vurderinger.length) o.push('Ingen vurderinger.');
  vm.vurderinger.forEach((n) => skriv(n, 0, null));
  o.push('');

  if (vm.partsmedvirkninger.length) {
    o.push('## Partsmedvirkning', '');
    vm.partsmedvirkninger.forEach((p) => o.push(`- ${p.type}${p.tidspunkt ? ` (${dato(p.tidspunkt)})` : ''}: ${p.innhold}`));
    o.push('');
  }

  if (vm.relasjoner.length) {
    o.push('## Relaterte saker', '');
    vm.relasjoner.forEach((r) => o.push(`- ${r.type}: ${r.relatertSakId}`));
    o.push('');
  }

  o.push('## Faktum', '');
  if (!vm.faktum.length) o.push('Ingen faktum.');
  vm.faktum.forEach((f) => o.push(`- ${f.verdi}${f.kilde ? ` — ${f.kilde.navn}${f.kilde.autoritativ ? ' (autoritativ)' : ''}` : ''}${f.laast ? ' · låst' : ''}`));
  o.push('');

  if (vm.kryssSak.faktum.length) {
    o.push('## Faktum fra andre saker', '');
    vm.kryssSak.faktum.forEach((f) => o.push(`- ${f.verdi}${f.kilde ? ` — ${f.kilde.navn}` : ''}`));
    o.push('');
  }

  const r = vm.referanser;
  o.push('## Kilder og hjemler', '');
  r.kilder.forEach((k) => o.push(`- Kilde: ${k.navn} (${k.autoritativ ? 'autoritativ' : 'ikke autoritativ'})`));
  r.rettskilder.forEach((x) => o.push(`- Rettskilde: ${x.henvisning}`));
  r.regler.forEach((x) => o.push(`- Regel: ${x.teknologi} (${x.type})${x.referanse ? ` — ${x.referanse}` : ''}`));
  r.vilkar.forEach((x) => o.push(`- Vilkår: ${x.navn}${x.kode ? ` (${x.kode})` : ''}${x.tekst ? `: ${tekstMedSprak(x.tekst, alle)}` : ''}`));
  o.push('');

  return o.join('\n').replace(/\n{3,}/g, '\n\n').trimEnd() + '\n';
}

/** Ren tekst: Markdown uten markeringstegn. */
export function tilTekst(vm, valg = {}) {
  return tilMarkdown(vm, valg)
    .replace(/^#{1,3} /gm, '')
    .replace(/\*\*(.+?)\*\*/g, '$1')
    .replace(/^\*(.+)\*$/gm, '$1');
}

const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');

const PILL = { Oppfylt: 'ok', IkkeOppfylt: 'bad' };

/** HTML-fragment med klassene `fv-*`. Bruk `visningCss` for utseende. */
export function tilHtml(vm, valg = {}) {
  const alle = valg.alleSprak !== false;
  const tekst = (t) => esc(tekstMedSprak(t, alle));
  const pille = (txt, k = '') => `<span class="fv-pille${k ? ` fv-${k}` : ''}">${esc(txt)}</span>`;
  const h = [];

  h.push('<article class="fv">');
  h.push(`<header class="fv-topp"><h2>${esc(vm.tittel)}</h2>`);
  if (vm.sak) {
    h.push(`<p class="fv-under">${esc(vm.sak.status)}${vm.sak.hendelse ? ` · utløst av ${esc(vm.sak.hendelse)}` : ''}${vm.sak.opprettet ? ` · opprettet ${esc(dato(vm.sak.opprettet))}` : ''}</p>`);
  }
  h.push('</header>');

  h.push('<div class="fv-tall">');
  [[vm.tall.vedtak, 'vedtak'], [vm.tall.faktum, `faktum, ${vm.tall.faktumLaast} låst`], [vm.tall.vurderinger, `vurderinger, ${vm.tall.vurderingerLaast} låst`], [vm.tall.eskalerte, 'eskalerte']]
    .forEach(([n, l]) => h.push(`<div class="fv-tall-rute"><strong>${n}</strong><span>${esc(l)}</span></div>`));
  h.push('</div>');

  if (vm.merknader.length) {
    h.push('<ul class="fv-merknader">');
    vm.merknader.forEach((m) => h.push(`<li class="fv-merknad${m.niva === 'advarsel' ? ' fv-advarsel' : ''}">${m.niva === 'advarsel' ? '<b>Advarsel:</b> ' : ''}${esc(m.tekst)}</li>`));
    h.push('</ul>');
  }

  h.push('<section class="fv-seksjon"><h3>Vedtak</h3>');
  if (!vm.vedtak.length) h.push('<p class="fv-tom">Ingen vedtak.</p>');
  vm.vedtak.forEach((v) => {
    h.push(`<div class="fv-kort fv-rot"><div class="fv-hode">${pille(v.automatiseringsgrad, 'ok')}${pille(dato(v.tidspunkt))}</div><p class="fv-hovedtekst"><strong>${esc(v.utfall)}</strong></p>`);
    h.push(`<p class="fv-liten">Forklaringsloggen frøs ${v.logg.oppforinger} oppføringer: ${v.logg.faktum} faktum, ${v.logg.vurderinger} vurderinger, ${v.logg.partsmedvirkninger} partsmedvirkning.</p>`);
    v.virkninger.forEach((x) => {
      const rest = virkningslinjer(x);
      h.push(`<p class="fv-virkning"><b>${esc(x.type)}</b> (${esc(x.varighet)}): ${tekst(x.beskrivelse)}${rest.length ? ` <span class="fv-liten">${esc(rest.join(', '))}</span>` : ''}</p>`);
    });
    h.push('</div>');
  });
  h.push('</section>');

  h.push('<section class="fv-seksjon"><h3>Vurderinger</h3>');
  const node = (n, rot, forelder) => {
    const refs = [];
    if (n.vilkar && rot) refs.push(`<div><b>vilkår</b>${esc(n.vilkar.navn)}${n.vilkar.kode ? ` <code>${esc(n.vilkar.kode)}</code>` : ''}</div>`);
    if (n.regel && !sammeRegel(n, forelder)) refs.push(`<div><b>regel</b>${esc(n.regel.teknologi)} (${esc(n.regel.type)})</div>`);
    n.faktum.forEach((f) => refs.push(`<div><b>faktum</b>${esc(f.verdi)}${f.kilde ? ` <em>(${esc(f.kilde.navn)})</em>` : ''}${f.annenSak ? ' <em>[annen sak]</em>' : ''}</div>`));
    n.hjemler.forEach((x) => refs.push(`<div><b>hjemmel</b>${esc(x.henvisning)}</div>`));
    n.siterer.forEach((s) => refs.push(`<div><b>bygger på</b>${esc(s.utfall)}${s.beregningsspor ? ` — ${esc(s.beregningsspor)}` : ''}${s.annenSak ? ' <em>[annen sak]</em>' : ''}</div>`));
    let x = `<div class="fv-kort${rot ? ' fv-rot' : ''}"><div class="fv-hode">${pille(n.utfallTekst, PILL[n.utfall] || 'varsel')}${pille(n.type)}${n.eskalert ? pille('eskalert', 'varsel') : ''}${pille(n.laast ? 'låst' : 'levende', n.laast ? 'info' : '')}${n.konfidens != null ? pille(`konfidens ${n.konfidens}`) : ''}</div>`;
    if (n.beregningsspor) x += `<p class="fv-hovedtekst">${esc(n.beregningsspor)}</p>`;
    if (n.hovedhensyn) x += `<p class="fv-hovedtekst"><b>Hovedhensyn:</b> ${tekst(n.hovedhensyn)}</p>`;
    if (n.forkastedeUtfall) x += `<p class="fv-hovedtekst"><b>Forkastede utfall:</b> ${tekst(n.forkastedeUtfall)}</p>`;
    if (refs.length) x += `<div class="fv-refs">${refs.join('')}</div>`;
    x += '</div>';
    if (n.barn.length) x += `<div class="fv-barn">${n.barn.map((b) => node(b, false, n)).join('')}</div>`;
    return x;
  };
  if (!vm.vurderinger.length) h.push('<p class="fv-tom">Ingen vurderinger.</p>');
  h.push(`<div class="fv-tre">${vm.vurderinger.map((n) => node(n, true, null)).join('')}</div></section>`);

  if (vm.partsmedvirkninger.length) {
    h.push('<section class="fv-seksjon"><h3>Partsmedvirkning</h3><ul>');
    vm.partsmedvirkninger.forEach((p) => h.push(`<li>${esc(p.type)}${p.tidspunkt ? ` (${esc(dato(p.tidspunkt))})` : ''}: ${esc(p.innhold)}</li>`));
    h.push('</ul></section>');
  }

  if (vm.kryssSak.faktum.length) {
    h.push('<section class="fv-seksjon"><h3>Faktum fra andre saker</h3><ul>');
    vm.kryssSak.faktum.forEach((f) => h.push(`<li>${esc(f.verdi)}${f.kilde ? ` <em>(${esc(f.kilde.navn)})</em>` : ''}</li>`));
    h.push('</ul></section>');
  }

  const r = vm.referanser;
  h.push('<section class="fv-seksjon"><h3>Kilder og hjemler</h3><div class="fv-kolonner">');
  const kol = (tittel, rader) => (rader.length ? `<div><h4>${tittel} (${rader.length})</h4><ul>${rader.map((x) => `<li>${x}</li>`).join('')}</ul></div>` : '');
  h.push(kol('Kilder', r.kilder.map((k) => `${esc(k.navn)} <em>(${k.autoritativ ? 'autoritativ' : 'ikke autoritativ'})</em>`)));
  h.push(kol('Rettskilder', r.rettskilder.map((x) => esc(x.henvisning))));
  h.push(kol('Regler', r.regler.map((x) => `${esc(x.teknologi)} <em>(${esc(x.type)})</em>`)));
  h.push(kol('Vilkår', r.vilkar.map((x) => `${esc(x.navn)}${x.kode ? ` <code>${esc(x.kode)}</code>` : ''}${x.tekst ? `<br><span class="fv-liten">${tekst(x.tekst)}</span>` : ''}`)));
  h.push('</div></section>');

  h.push('</article>');
  return h.join('\n');
}

/**
 * Stilark for `tilHtml`. Bruker designsystemet sine tokens når de finnes og faller ellers
 * tilbake til nøytrale verdier, så samme CSS fungerer på nettstedet og i en frittstående fil.
 */
export const visningCss = `
.fv{--fv-tekst:var(--ds-color-text-default,#1b1d1f);--fv-dempet:var(--ds-color-text-subtle,#5a6168);--fv-linje:var(--ds-color-border-subtle,#d3d8dc);--fv-flate:var(--ds-color-background-default,#fff);--fv-tonet:var(--ds-color-background-tinted,#f3f5f6);--fv-aksent:var(--ds-color-accent-text-default,#0b5aa6);--fv-aksentflate:var(--ds-color-accent-surface-tinted,#e3eefa);--fv-ok:var(--ds-color-success-text-default,#14683a);--fv-okflate:var(--ds-color-success-surface-tinted,#e1f3e8);--fv-feil:var(--ds-color-danger-text-default,#a1281c);--fv-feilflate:var(--ds-color-danger-surface-tinted,#fbe5e2);--fv-varsel:var(--ds-color-warning-text-default,#7a5200);--fv-varselflate:var(--ds-color-warning-surface-tinted,#fcefd2);color:var(--fv-tekst);font-size:1rem;line-height:1.5;max-width:60rem}
.fv *{box-sizing:border-box}
.fv h2{margin:0 0 .25rem;font-size:1.5rem}.fv h3{margin:1.75rem 0 .6rem;font-size:1.15rem}.fv h4{margin:.5rem 0 .25rem;font-size:.9rem}
.fv-under,.fv-liten,.fv-tom{color:var(--fv-dempet);font-size:.875rem;margin:.25rem 0}
.fv-tall{display:grid;grid-template-columns:repeat(auto-fit,minmax(9rem,1fr));gap:.75rem;margin:1rem 0}
.fv-tall-rute{border:1px solid var(--fv-linje);border-radius:.6rem;padding:.6rem .8rem;background:var(--fv-flate)}
.fv-tall-rute strong{display:block;font-size:1.5rem;line-height:1.1;color:var(--fv-aksent)}.fv-tall-rute span{font-size:.85rem;color:var(--fv-dempet)}
.fv-merknader{list-style:none;margin:0 0 .5rem;padding:0;display:flex;flex-direction:column;gap:.4rem}
.fv-merknad{border-left:4px solid var(--fv-aksent);background:var(--fv-aksentflate);border-radius:.4rem;padding:.5rem .8rem;font-size:.9rem}
.fv-advarsel{border-left-color:var(--fv-varsel);background:var(--fv-varselflate)}
.fv-kort{border:1px solid var(--fv-linje);border-radius:.6rem;padding:.75rem 1rem;margin:0 0 .6rem;background:var(--fv-flate);min-width:0}
.fv-rot{border-color:var(--fv-aksent);background:var(--fv-aksentflate)}
.fv-hode{display:flex;flex-wrap:wrap;gap:.4rem;align-items:center;margin-bottom:.4rem}
.fv-hovedtekst{margin:.2rem 0}.fv-virkning{margin:.3rem 0}
.fv-refs{margin-top:.5rem;display:flex;flex-direction:column;gap:.2rem;font-size:.85rem;color:var(--fv-dempet)}
.fv-refs b{font-size:.72rem;text-transform:uppercase;letter-spacing:.04em;color:var(--fv-aksent);margin-right:.5em}
.fv-barn{margin-left:1rem;padding-left:1rem;border-left:2px solid var(--fv-linje)}
.fv-pille{display:inline-block;font-size:.75rem;font-weight:600;line-height:1;padding:.3em .6em;border-radius:999px;border:1px solid var(--fv-linje);background:var(--fv-tonet)}
.fv-ok{background:var(--fv-okflate);color:var(--fv-ok)}.fv-bad{background:var(--fv-feilflate);color:var(--fv-feil)}.fv-varsel{background:var(--fv-varselflate);color:var(--fv-varsel)}.fv-info{background:var(--fv-aksentflate);color:var(--fv-aksent)}
.fv-kolonner{display:grid;grid-template-columns:repeat(auto-fit,minmax(15rem,1fr));gap:1rem}
.fv ul{padding-left:1.1rem;margin:.25rem 0}.fv li{margin:.15rem 0}.fv code{font-size:.85em}
@media (max-width:560px){.fv-barn{margin-left:.4rem;padding-left:.6rem}}
`;

/** Frittstående HTML-dokument, for eksport eller utskrift. */
export function tilHtmlDokument(vm, valg = {}) {
  return `<!doctype html>
<html lang="${esc(vm.sprak)}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>${esc(vm.tittel)}</title><style>body{margin:0;padding:1.5rem 1rem;font-family:system-ui,sans-serif;background:#fff;color:#1b1d1f}@media (prefers-color-scheme:dark){body{background:#101214;color:#e8eaec}.fv{--fv-tekst:#e8eaec;--fv-dempet:#a3abb2;--fv-linje:#3a4047;--fv-flate:#16191c;--fv-tonet:#20252a;--fv-aksent:#7db4ee;--fv-aksentflate:#1b2a3b;--fv-ok:#7fd69f;--fv-okflate:#173325;--fv-feil:#f09a90;--fv-feilflate:#3a1d1a;--fv-varsel:#e9c06a;--fv-varselflate:#33290f}}${visningCss}</style></head>
<body>${tilHtml(vm, valg)}</body></html>
`;
}

/** Bekvemmelighet: JSON inn, ønsket format ut. */
export function vis(input, format = 'md', valg = {}) {
  const vm = byggVisning(input, valg);
  switch (format) {
    case 'md': case 'markdown': return tilMarkdown(vm, valg);
    case 'txt': case 'tekst': return tilTekst(vm, valg);
    case 'html': return tilHtml(vm, valg);
    case 'dokument': case 'htmldoc': return tilHtmlDokument(vm, valg);
    case 'json': case 'visning': return JSON.stringify(vm, null, 2) + '\n';
    default: throw new Error(`Ukjent format «${format}». Bruk md, txt, html, dokument eller json.`);
  }
}
