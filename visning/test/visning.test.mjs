import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import { normaliser, byggVisning, tilMarkdown, tilTekst, tilHtml, tilHtmlDokument, vis } from '../../site/assets/js/forklaring-visning.mjs';

// Fixturene er de innspilte svarene fra et ekte API-kall (se site/eksempel/data).
const her = dirname(fileURLToPath(import.meta.url));
const data = join(her, '..', '..', 'site', 'eksempel', 'data');
const indeks = JSON.parse(readFileSync(join(data, 'index.json'), 'utf8'));
const hent = (metode, sti, gruppe) => {
  const e = indeks.find((x) => x.method === metode && x.path === sti && x.group.startsWith(gruppe));
  assert.ok(e, `fant ikke ${metode} ${sti} for ${gruppe}`);
  return JSON.parse(readFileSync(join(data, e.file), 'utf8'));
};
const sakA = () => hent('GET', '/api/saker/{sakId}/forklaring', 'Sak A');
const sakB = () => hent('GET', '/api/saker/{sakId}/forklaring', 'Sak B');
const vedtakA = () => hent('GET', '/api/vedtak/{vedtakId}/forklaring', 'Sak A');
const kopi = (x) => JSON.parse(JSON.stringify(x));

test('sak med vedtak: tall, tre og ingen advarsler', () => {
  const vm = byggVisning(sakA());
  assert.equal(vm.endepunkt, 'sak');
  assert.match(vm.tittel, /Eiganesveien/);
  assert.deepEqual(vm.tall, { vedtak: 1, faktum: 3, faktumLaast: 3, vurderinger: 4, vurderingerLaast: 4, eskalerte: 0, rotvurderinger: 1 });
  assert.equal(vm.vurderinger[0].barn.length, 3);
  assert.equal(vm.vedtak[0].automatiseringsgrad, 'Helautomatisert');
  assert.equal(vm.vedtak[0].virkninger[0].beskrivelse.andre.length, 1, 'nb som hovedspråk og nn som annen variant');
  assert.equal(vm.merknader.filter((m) => m.niva === 'advarsel').length, 0);
  assert.ok(!vm.merknader.some((m) => /ikke noe vedtak/.test(m.tekst)));
});

test('sak uten vedtak: merknadene regnes ut av strukturen', () => {
  const vm = byggVisning(sakB());
  assert.equal(vm.tall.vedtak, 0);
  assert.equal(vm.vedtak.length, 0);
  assert.equal(vm.tall.eskalerte, 2);
  const tekster = vm.merknader.map((m) => m.tekst);
  assert.ok(tekster.some((t) => /ikke noe vedtak/.test(t)));
  assert.ok(tekster.some((t) => /2 vurdering\(er\) er eskalert/.test(t)));
  // «Ikke oppfylt» er en konklusjon (regel 3.14), så bare roten (Uavklart) telles som uten konklusjon.
  assert.ok(tekster.some((t) => /^Vurderinger uten konklusjon: Uavklart \(1\)\.$/.test(t)));
  assert.equal(vm.merknader.filter((m) => m.niva === 'advarsel').length, 0);
});

test('vedtak-forklaringen normaliseres til samme form', () => {
  const d = normaliser(vedtakA());
  assert.equal(d.endepunkt, 'vedtak');
  assert.equal(d.vedtak.length, 1);
  const vm = byggVisning(vedtakA());
  assert.match(vm.tittel, /^Vedtak: /);
  assert.equal(vm.tall.vurderinger, 4);
  assert.equal(vm.vurderinger[0].barn.length, 3);
});

test('pakket svar ({response}) og rått svar gir samme visning', () => {
  const pakket = sakA();
  assert.ok(pakket.response);
  assert.deepEqual(byggVisning(pakket), byggVisning(pakket.response));
});

test('all tekst kommer fra svaret: hver beregningsspor og hvert faktum står i Markdown og HTML', () => {
  for (const svar of [sakA(), sakB(), vedtakA()]) {
    const r = svar.response;
    const vm = byggVisning(svar);
    const md = tilMarkdown(vm);
    const html = tilHtml(vm);
    for (const v of r.vurderinger) {
      assert.ok(md.includes(v.beregningsspor), `Markdown mangler: ${v.beregningsspor}`);
      assert.ok(html.includes(v.beregningsspor.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')), `HTML mangler: ${v.beregningsspor}`);
    }
    for (const f of r.faktum) assert.ok(md.includes(f.verdi), `Markdown mangler faktum: ${f.verdi}`);
  }
});

test('Markdown har forventet oppbygning', () => {
  const md = tilMarkdown(byggVisning(sakA()));
  for (const overskrift of ['# ', '## Sammendrag', '## Vedtak', '## Vurderinger', '## Faktum', '## Kilder og hjemler']) {
    assert.ok(md.includes(overskrift), `mangler ${overskrift}`);
  }
  assert.match(md, /Tillatelse til rehabilitering av skorstein \(innvendig fôring\) \[nb\] \/ Løyve til rehabilitering av skorstein \(innvendig fôring\) \[nn\]/);
  assert.ok(!/\n{3,}/.test(md));
});

test('ren tekst har ingen markdown-tegn', () => {
  const t = tilTekst(byggVisning(sakB()));
  assert.ok(!/\*\*|^#{1,3} /m.test(t));
  assert.match(t, /Sammendrag/);
});

test('--en-sprak og språkvalg', () => {
  const nn = byggVisning(sakA(), { sprak: 'nn' });
  assert.match(nn.referanser.vilkar[0].tekst.tekst, /godkjennast/);
  const md = tilMarkdown(byggVisning(sakA()), { alleSprak: false });
  assert.ok(!/\[nb\]/.test(md));
});

test('HTML escapes tekst fra svaret', () => {
  const svar = kopi(sakA());
  svar.response.vurderinger[0].beregningsspor = '<script>alert(1)</script> & "sitat"';
  svar.response.vurderingstre[0].beregningsspor = svar.response.vurderinger[0].beregningsspor;
  svar.response.sak.tittel = '<img src=x onerror=alert(1)>';
  const html = tilHtml(byggVisning(svar));
  assert.ok(!html.includes('<script>'));
  assert.ok(!html.includes('<img'));
  assert.ok(html.includes('&lt;script&gt;'));
});

test('manglende oppslag gir advarsel i stedet for krasj', () => {
  const svar = kopi(sakA());
  svar.response.referansedata.kilder = [];
  svar.response.referansedata.rettskilder = [];
  const vm = byggVisning(svar);
  const advarsel = vm.merknader.find((m) => m.niva === 'advarsel');
  assert.ok(advarsel);
  assert.match(advarsel.tekst, /kilde/);
  assert.match(advarsel.tekst, /rettskilde/);
});

test('uten ferdig nøstet tre bygges treet fra den flate listen', () => {
  const svar = kopi(sakA());
  const med = byggVisning(svar);
  svar.response.vurderingstre = [];
  const uten = byggVisning(svar);
  assert.equal(uten.vurderinger.length, med.vurderinger.length);
  assert.equal(uten.vurderinger[0].barn.length, med.vurderinger[0].barn.length);
});

test('skjønn uten hovedhensyn gir advarsel (regel 3.2)', () => {
  const svar = kopi(sakA());
  svar.response.vurderingstre[0].type = 'Skjonn';
  svar.response.vurderingstre[0].hovedhensyn = [];
  const vm = byggVisning(svar);
  assert.ok(vm.merknader.some((m) => m.niva === 'advarsel' && /hovedhensyn/.test(m.tekst)));
});

test('ukjent form gir tydelig feil', () => {
  assert.throws(() => byggVisning({ noe: 'annet' }), /Kjenner ikke igjen formen/);
  assert.throws(() => byggVisning(null), /Forventet et JSON-objekt/);
});

test('hele dokumentet og format-bryteren', () => {
  const vm = byggVisning(sakA());
  const dok = tilHtmlDokument(vm);
  assert.match(dok, /^<!doctype html>/);
  assert.match(dok, /<style>/);
  assert.ok(vis(sakA(), 'md').startsWith('# '));
  assert.throws(() => vis(sakA(), 'xml'), /Ukjent format/);
  assert.equal(JSON.parse(vis(sakA(), 'json')).versjon, 1);
});

test('kommandolinjen leser en fil og skriver Markdown', () => {
  const e = indeks.find((x) => x.method === 'GET' && x.path === '/api/saker/{sakId}/forklaring' && x.group.startsWith('Sak B'));
  const ut = execFileSync(process.execPath, [join(her, '..', 'cli.mjs'), join(data, e.file)], { encoding: 'utf8' });
  assert.match(ut, /^# Søknad om rehabilitering av skorstein/);
  assert.match(ut, /Saken har ikke noe vedtak/);
});
