import { build } from "esbuild";
import clear from "esbuild-plugin-clear";

build({
    entryPoints: ["./src/client.ts"],
    bundle: true,
    outfile: "./dist/index.js",
    minify: true,
    plugins: [
        clear("./build"),
    ]
})
.catch(() => {
    process.exit(1);
});
