import { mkdirSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
const directory = fileURLToPath(new URL('../audio/sfx/', import.meta.url));
mkdirSync(directory, { recursive: true });
for (const [kind, duration, frequency] of [['attack', .18, 180], ['breath', .55, 90], ['death', .4, 260], ['turn', .2, 660]]) {
  const rate = 22050, count = Math.floor(rate * duration);
  const data = Buffer.alloc(44 + count * 2);
  data.write('RIFF'); data.writeUInt32LE(data.length - 8, 4); data.write('WAVEfmt ', 8);
  data.writeUInt32LE(16, 16); data.writeUInt16LE(1, 20); data.writeUInt16LE(1, 22);
  data.writeUInt32LE(rate, 24); data.writeUInt32LE(rate * 2, 28);
  data.writeUInt16LE(2, 32); data.writeUInt16LE(16, 34); data.write('data', 36);
  data.writeUInt32LE(count * 2, 40);
  let seed = 7391;
  for (let i = 0; i < count; i++) {
    const t = i / rate, envelope = Math.min(t / .008, 1) * (1 - i / count) ** 2;
    seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0;
    const noise = seed / 2147483648 - 1;
    const phase = 2 * Math.PI * (frequency * t - (kind === 'death' ? 220 * t * t : 0));
    const value = kind === 'breath' ? noise : Math.sin(phase) + (kind === 'attack' ? noise * .3 : 0);
    data.writeInt16LE(Math.round(value * envelope * 9000), 44 + i * 2);
  }
  writeFileSync(`${directory}/${kind}.wav`, data);
}
console.log('4 SFX PCM mono gerados (22.05 kHz), sem dependências externas.');
