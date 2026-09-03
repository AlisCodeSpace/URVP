// Copies the static export into the backend's wwwroot, which is what makes the deployment
// same-origin: ASP.NET Core then serves the app and the API from one origin, so the session
// cookie stays first-party and no CORS grant is needed in Production.
//
// The destination is wiped first. A stale file left behind from an earlier build would keep
// being served as a public asset, and hashed bundle names mean it would never be overwritten.

import { cp, mkdir, rm, stat } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const source = resolve(here, "..", "out");
const destination = resolve(here, "..", "..", "FEA.URVP.Backend", "wwwroot");

async function main() {
  try {
    const info = await stat(source);
    if (!info.isDirectory()) throw new Error("not a directory");
  } catch {
    console.error(`No export found at ${source}. Run "npm run build" first.`);
    process.exit(1);
  }

  try {
    await stat(join(source, "index.html"));
  } catch {
    console.error(`${source} has no index.html; the export looks incomplete.`);
    process.exit(1);
  }

  await rm(destination, { recursive: true, force: true });
  await mkdir(destination, { recursive: true });
  await cp(source, destination, { recursive: true });

  console.log(`Published export to ${destination}`);
}

await main();
