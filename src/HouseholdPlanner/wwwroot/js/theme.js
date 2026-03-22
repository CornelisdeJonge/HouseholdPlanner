(() => {
    const storageKey = "theme";
    const root = document.documentElement;
    const media = window.matchMedia("(prefers-color-scheme: dark)");

    function getStoredTheme() {
        const value = localStorage.getItem(storageKey);
        return value === "light" || value === "dark" ? value : null;
    }

    function getPreferredTheme() {
        const stored = getStoredTheme();
        if (stored) {
            return stored;
        }

        return media.matches ? "dark" : "light";
    }

    function applyTheme(theme) {
        root.classList.remove("theme-light", "theme-dark");
        root.classList.add(theme === "dark" ? "theme-dark" : "theme-light");
        root.setAttribute("data-theme", theme);
    }

    function updateToggleUi(theme) {
        document.querySelectorAll("[data-theme-toggle]").forEach((button) => {
            const isDark = theme === "dark";
            button.setAttribute("aria-pressed", String(isDark));
            button.setAttribute("aria-label", isDark ? "Switch to light mode" : "Switch to dark mode");

            const label = button.querySelector("[data-theme-label]");
            if (label) {
                label.textContent = isDark ? "Light mode" : "Dark mode";
            }
        });
    }

    function setTheme(theme, persist = true) {
        applyTheme(theme);

        if (persist) {
            localStorage.setItem(storageKey, theme);
        }

        updateToggleUi(theme);
    }

    function initTheme() {
        const theme = getPreferredTheme();
        applyTheme(theme);
        updateToggleUi(theme);
    }

    document.addEventListener("DOMContentLoaded", () => {
        initTheme();

        document.querySelectorAll("[data-theme-toggle]").forEach((button) => {
            button.addEventListener("click", () => {
                const current = root.classList.contains("theme-dark") ? "dark" : "light";
                const next = current === "dark" ? "light" : "dark";
                setTheme(next, true);
            });
        });
    });

    media.addEventListener?.("change", (event) => {
        if (getStoredTheme()) {
            return;
        }

        const theme = event.matches ? "dark" : "light";
        applyTheme(theme);
        updateToggleUi(theme);
    });

    window.appTheme = {
        get: () => root.classList.contains("theme-dark") ? "dark" : "light",
        set: (theme) => {
            if (theme === "light" || theme === "dark") {
                setTheme(theme, true);
            }
        },
        clearPreference: () => {
            localStorage.removeItem(storageKey);
            const theme = getPreferredTheme();
            applyTheme(theme);
            updateToggleUi(theme);
        }
    };
})();