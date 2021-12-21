import { build } from "esbuild";
import clear from "esbuild-plugin-clear";
import { exec } from "child_process";
import glob from "glob";

glob.glob("./src/**/*.ts", (err, files) => {
    build({
        entryPoints: files,
        outdir: "./dist",
        minify: true,
        plugins: [
            clear("./build"),
        ]
    })
    .catch(() => {
        process.exit(1);
    });
});

exec("npx tsc", (err, stdout, stderr) => {
    console.log(stdout);
    console.log(stderr);
});