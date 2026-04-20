const gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const sourcemaps = require('gulp-sourcemaps');
const browserSync = require('browser-sync').create();
const fs = require('fs');
const path = require('path');

// Configuração de Caminhos
const paths = {
    scss: './assets/sass/main.scss',
    allScss: './assets/sass/**/*.scss',
    js: './assets/js/**/*.js',
    img: './assets/img',
    html: './assets/**/*.html',
    dist: './dist'
};

// 1. Compilar Sass
function compileSass() {
    return gulp.src(paths.scss)
        .pipe(sourcemaps.init())
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(sourcemaps.write('./'))
        .pipe(gulp.dest(`${paths.dist}/css`))
        .pipe(browserSync.stream());
}

// 2. Mover JS
function moveJs() {
    return gulp.src(paths.js)
        .pipe(gulp.dest(`${paths.dist}/js`))
        .pipe(browserSync.stream());
}

// 3. Mover HTML
function moveHtml() {
    return gulp.src(paths.html)
        .pipe(gulp.dest(paths.dist));
}

// 4. Cópia Binária Segura de Imagens
function copyImages(cb) {
    const srcDir = path.resolve(paths.img);
    const destDir = path.resolve(`${paths.dist}/img`);

    if (!fs.existsSync(srcDir)) { cb(); return; }
    if (!fs.existsSync(destDir)) fs.mkdirSync(destDir, { recursive: true });

    const copyRecursive = (src, dest) => {
        const entries = fs.readdirSync(src, { withFileTypes: true });
        for (const entry of entries) {
            const srcPath = path.join(src, entry.name);
            const destPath = path.join(dest, entry.name);
            if (entry.isDirectory()) {
                if (!fs.existsSync(destPath)) fs.mkdirSync(destPath, { recursive: true });
                copyRecursive(srcPath, destPath);
            } else {
                fs.copyFileSync(srcPath, destPath);
            }
        }
    };

    try {
        copyRecursive(srcDir, destDir);
    } catch (err) {
        console.error("Erro nas imagens:", err);
    }
    cb();
}

// 5. Servidor e Watch
function watchFiles() {
    browserSync.init({
        server: { baseDir: paths.dist }
    });

    gulp.watch(paths.allScss, compileSass);
    gulp.watch(paths.js, moveJs);
    gulp.watch('./assets/img/**/*', copyImages);
    gulp.watch(paths.html, moveHtml).on('change', browserSync.reload);
}

// Exportar tarefas para o CLI
exports.compileSass = compileSass;
exports.moveJs = moveJs;
exports.moveHtml = moveHtml;
exports.copyImages = copyImages;

// Tarefa Default (gulp)
exports.default = gulp.series(
    gulp.parallel(compileSass, moveJs, copyImages, moveHtml),
    watchFiles
);
