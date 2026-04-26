window.HydraFX = {
    fadeIn() {
        const root = document.documentElement;
        root.classList.add("fx-fade-in");
    },

    blurIn() {
        const root = document.documentElement;
        root.classList.add("fx-blur-in");
    },

    blurOut() {
        const root = document.documentElement;
        root.classList.remove("fx-blur-in");
    },

    smoothTransition() {
        const root = document.documentElement;
        root.classList.add("fx-transition");
        setTimeout(() => root.classList.remove("fx-transition"), 600);
    }
};
