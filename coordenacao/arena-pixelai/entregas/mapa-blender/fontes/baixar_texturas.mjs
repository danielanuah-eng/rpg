import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const destination = path.join(root, 'fontes', 'pixellab-originais');
await fs.mkdir(destination, { recursive: true });
const base = 'https://backblaze.pixellab.ai/file/pixellab-tiles/79dfcb12-a74e-4f4e-87d1-a0307df21f74/969ac24a-abe9-4723-8531-baabde2c70e3/';
for (let start = 0; start < 16; start += 4) {
  await Promise.all(Array.from({ length: 4 }, async (_, j) => {
    const index = start + j;
    const response = await fetch(`${base}tile_${index}.png`, { signal: AbortSignal.timeout(60000) });
    if (!response.ok) throw new Error(`tile_${index}: ${response.status}`);
    const buffer = Buffer.from(await response.arrayBuffer());
    if (buffer.subarray(1, 4).toString() !== 'PNG') throw new Error(`tile_${index}: resposta não PNG`);
    await fs.writeFile(path.join(destination, `tile_${index}.png`), buffer);
    console.log(`TEXTURA_OK ${index} ${buffer.length}`);
  }));
}
await fs.writeFile(path.join(destination, 'origem.json'), JSON.stringify({
  provider: 'PixelLab', tool: 'create_tiles_pro', section: 'Map',
  assetId: '969ac24a-abe9-4723-8531-baabde2c70e3', seed: 10102026,
  date: '2026-10-10', requested: 8, returned: 16,
  baseUrl: base, map: 'fontes/texturas-selecao.json',
  prompt: 'Eight independent square seamless pixel art material textures for a low-poly 3D woodland HD-2D tactical arena, all perfectly FLAT top-down orthographic surfaces filling the entire square, no side faces, no baked shadows, no objects or cast shadows. Restrained earthy palette, crisp pixels, selective outlines only within textures. 1). moss grass A, muted sage and olive green short dense grass. 2). moss grass B, almost same value and hue, slightly warmer, subtle clover flecks. 3). ancient central zone mosaic, grey mossy cobblestones and a delicate circular pale turquoise woodland rune, no text. 4). side earth cross section, dark umber soil with embedded slate and tiny roots. 5). oak bark, vertical rugged russet dark-brown bark. 6). dense oak foliage, green clustered leaves, deep jade shadows and muted spring green. 7). old warm wooden planks, honey brown with dark seams and knots. 8). grey slate rock, desaturated blue-grey stone fissures and sparse olive moss.'
}, null, 2));
