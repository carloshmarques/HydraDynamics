'use strict';

/* ============================================================
   GLASS-SITE — Gulpfile (CJS Edition)
============================================================ */

/* ------------------------------
   Dependências principais
------------------------------ */
const gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const sourcemaps = require('gulp-sourcemaps');
const browserSync = require('browser-sync').create();
const fs = require('fs');
const path = require('path');

/* ------------------------------
   Clean (compatível com Node 22)
------------------------------ */
const del = require('delete');

/* ============================================================
   Ambiente (dev por padrão)
============================================================ */
const env = process.env.NODE_ENV || 'development';
const isDev = env === 'development';

console.log(`GLASS-SITE está a correr em modo: ${env}`);

/* ============================================================
   Pastas de saída por ambiente
============================================================ */
const outputDir =
  env === 'production' ? './dist/' :
  env === 'staging'    ? './staging/' :
  env === 'deployment' ? './build/' :
                         './dev/';

/* ============================================================
   Caminhos de entrada
============================================================ */
const paths = {
    scss: './assets/sass/main.scss',
    allScss: './assets/sass/**/*.scss',
    js: './assets/js/**/*.js',
    img: './assets/img',
    html: './assets/**/*.html'
};

/* ============================================================
   Clean — apaga a pasta do ambiente atual
============================================================ */
function clean(cb) {
    del([outputDir + '**'], cb);
}

/* ============================================================
   Compilar Sass
============================================================ */
function compileSass() {
    return gulp.src(paths.scss)
        .pipe(sourcemaps.init())
        .pipe(sass({ outputStyle: isDev ? 'expanded' : 'compressed' })
            .on('error', sass.logError))
        .pipe(sourcemaps.write('./'))
        .pipe(gulp.dest(outputDir + 'css'))
        .pipe(browserSync.stream());
}

/* ============================================================
   Mover JS
============================================================ */
function moveJs() {
    return gulp.src(paths.js)
        .pipe(gulp.dest(outputDir + 'js'))
        .pipe(browserSync.stream());
}

/* ============================================================
   Mover HTML
============================================================ */
function moveHtml() {
    return gulp.src(paths.html)
        .pipe(gulp.dest(outputDir));
}

/* ============================================================
   Imagens — cópia binária segura (sem corrupção)
============================================================ */
function images(cb) {
    const srcDir = 'assets/img';
    const destDir = outputDir + 'img';

    fs.mkdirSync(destDir, { recursive: true });

    function copyRecursive(src, dest) {
        fs.readdirSync(src, { withFileTypes: true }).forEach(entry => {
            const srcPath = path.join(src, entry.name);
            const destPath = path.join(dest, entry.name);

            if (entry.isDirectory()) {
                fs.mkdirSync(destPath, { recursive: true });
                copyRecursive(srcPath, destPath);
            } else {
                fs.copyFileSync(srcPath, destPath);
            }
        });
    }

    copyRecursive(srcDir, destDir);
    cb();
}

/* ============================================================
   Servidor + Watch
============================================================ */
function watchFiles() {
    browserSync.init({
        server: { baseDir: outputDir }
    });

    gulp.watch(paths.allScss, compileSass);
    gulp.watch(paths.js, moveJs);
    gulp.watch('./assets/img/**/*', images);   // <-- CORRIGIDO
    gulp.watch(paths.html, moveHtml).on('change', browserSync.reload);
}

/* ============================================================
   Tarefas públicas
============================================================ */
exports.clean = clean;
exports.sass = compileSass;
exports.js = moveJs;
exports.html = moveHtml;
exports.images = images;   // <-- CORRIGIDO

/* ============================================================
   Default (modo desenvolvimento)
============================================================ */
exports.default = gulp.series(
    clean,
    gulp.parallel(compileSass, moveJs, moveHtml, images),  // <-- CORRIGIDO
    watchFiles
);

/* ============================================================
   Build (produção, staging, deployment)
============================================================ */
exports.build = gulp.series(
    clean,
    gulp.parallel(compileSass, moveJs, moveHtml, images)   // <-- CORRIGIDO
);
