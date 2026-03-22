(function () {
    const storageKey = "theme";
    const root = document.documentElement;
    const toggle = document.getElementById("themeToggle");

    function applyTheme(theme) {
        const normalized = theme === "dark" ? "dark" : "light";

        root.classList.remove("theme-light", "theme-dark");
        root.classList.add(normalized === "dark" ? "theme-dark" : "theme-light");
        root.style.colorScheme = normalized;

        if (toggle) {
            toggle.setAttribute("aria-pressed", normalized === "dark" ? "true" : "false");
            toggle.setAttribute(
                "title",
                normalized === "dark" ? "Switch to light mode" : "Switch to dark mode"
            );
        }
    }

    const saved = localStorage.getItem(storageKey);
    const systemPrefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
    const initialTheme =
        saved === "dark" || saved === "light"
            ? saved
            : (systemPrefersDark ? "dark" : "light");

    applyTheme(initialTheme);

    window.toggleTheme = function () {
        const isDark = root.classList.contains("theme-dark");
        const nextTheme = isDark ? "light" : "dark";

        applyTheme(nextTheme);
        localStorage.setItem(storageKey, nextTheme);
    };
})();