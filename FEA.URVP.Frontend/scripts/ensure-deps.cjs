try {
  require.resolve("next/package.json");
} catch {
  require("child_process").execSync("npm ci --include=dev", { stdio: "inherit" });
}
