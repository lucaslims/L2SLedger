/**
 * generate-icons.mjs
 * Generates PNG icon files from the master SVG icon for use in PWA manifest and HTML.
 *
 * Usage: node scripts/generate-icons.mjs  (run from inside frontend/)
 * Requires: sharp (devDependency)
 */

import sharp from 'sharp';
import { readFileSync, mkdirSync } from 'fs';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));

const svgPath = resolve(__dirname, '../public/icons/icon.svg');
const outputDir = resolve(__dirname, '../public/icons');

mkdirSync(outputDir, { recursive: true });

const svgBuffer = readFileSync(svgPath);

const sizes = [16, 32, 48, 72, 96, 128, 144, 152, 180, 192, 384, 512];

console.log('Generating PWA icons from', svgPath);

for (const size of sizes) {
  const outPath = resolve(outputDir, `icon-${size}.png`);
  await sharp(svgBuffer)
    .resize(size, size)
    .png()
    .toFile(outPath);
  console.log(`  ✓ icon-${size}.png`);
}

console.log(`\nDone — ${sizes.length} icons generated in ${outputDir}`);
