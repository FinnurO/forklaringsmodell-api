#!/usr/bin/env node
// Lesevisning av en forklaring fra kommandolinjen.
//
//   node visning/cli.mjs svar.json                       Markdown til skjerm
//   node visning/cli.mjs svar.json -f html -o sak.html   frittstående HTML-fil
//   node visning/cli.mjs --url http://localhost:5013/api/saker/<id>/forklaring
//   curl -s .../forklaring | node visning/cli.mjs -      les fra standard inn
//
// Formater: md (standard), txt, html (fragment), dokument (hel HTML-fil), json (visningsmodellen).

import { readFileSync, writeFileSync } from 'node:fs';
import { vis } from '../site/assets/js/forklaring-visning.mjs';

const hjelp = `Bruk: node visning/cli.mjs <fil|-> [--url <adresse>] [-f md|txt|html|dokument|json] [--sprak nb|nn|…] [--en-sprak] [-o <fil>]`;

function lesArgumenter(argv) {
  const a = { format: 'md', sprak: 'nb', alleSprak: true, fil: null, url: null, ut: null };
  for (let i = 0; i < argv.length; i++) {
    const x = argv[i];
    if (x === '-h' || x === '--help') { a.hjelp = true; }
    else if (x === '-f' || x === '--format') { a.format = argv[++i]; }
    else if (x === '--sprak') { a.sprak = argv[++i]; }
    else if (x === '--en-sprak') { a.alleSprak = false; }
    else if (x === '--url') { a.url = argv[++i]; }
    else if (x === '-o' || x === '--ut') { a.ut = argv[++i]; }
    else if (!a.fil) { a.fil = x; }
    else { throw new Error(`Uventet argument: ${x}`); }
  }
  return a;
}

async function lesInn(a) {
  if (a.url) {
    const svar = await fetch(a.url);
    if (!svar.ok) throw new Error(`${a.url} svarte ${svar.status}`);
    return svar.json();
  }
  if (!a.fil) throw new Error('Gi en fil, «-» for standard inn, eller --url.');
  const tekst = a.fil === '-' ? readFileSync(0, 'utf8') : readFileSync(a.fil, 'utf8');
  return JSON.parse(tekst);
}

try {
  const a = lesArgumenter(process.argv.slice(2));
  if (a.hjelp) { console.log(hjelp); process.exit(0); }
  const json = await lesInn(a);
  const utdata = vis(json, a.format, { sprak: a.sprak, alleSprak: a.alleSprak });
  if (a.ut) { writeFileSync(a.ut, utdata, 'utf8'); console.error(`Skrev ${a.ut}`); }
  else { process.stdout.write(utdata); }
} catch (feil) {
  console.error(`Feil: ${feil.message}`);
  console.error(hjelp);
  process.exit(1);
}
