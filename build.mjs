import { build } from "esbuild";
import { exec } from "child_process";
import glob from "glob";

glob.glob("./src/**/*.ts", (err, files) => {
    build({
        entryPoints: files,
        outdir: "./",
        minify: true
    })
    .catch(() => {
        process.exit(1);
    });
});

exec("npx tsc", (err, stdout, stderr) => {
    console.log(stdout);
    console.log(stderr);
    console.log(err || "");

    if (err) {
        process.exit(1);
    }
});