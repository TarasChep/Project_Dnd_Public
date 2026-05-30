import { useState, useEffect } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useAuthStore } from "../store/authStore";
import "./Header.css";

export default function Header() {
  const navigate = useNavigate();
  const location = useLocation();
  const logout = useAuthStore((state) => state.logout);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname]);

  useEffect(() => {
    document.body.style.overflow = menuOpen ? "hidden" : "";
    return () => {
      document.body.style.overflow = "";
    };
  }, [menuOpen]);

  const handleLogout = () => {
    setMenuOpen(false);
    logout();
    navigate("/login", { replace: true });
  };

  const isActive = (path: string) => {
    if (path === "/") {
      return location.pathname === "/" || location.pathname.startsWith("/character");
    }
    return location.pathname === path || location.pathname.startsWith(`${path}/`);
  };

  const navLinkClass = (path: string) =>
    `app-header__link${isActive(path) ? " app-header__link--active" : ""}`;

  const closeMenu = () => setMenuOpen(false);

  return (
    <header className="app-header">
      <div className="app-header__shell">
        <button
          type="button"
          className="app-header__brand"
          onClick={() => {
            closeMenu();
            navigate("/");
          }}
        >
          <span className="app-header__logo" aria-hidden>
            🐉
          </span>
          <span className="app-header__title">
            <span className="app-header__title-main">D&D</span>
            <span className="app-header__title-sub">Platform</span>
          </span>
        </button>

        <nav
          id="app-header-nav"
          className={`app-header__nav${menuOpen ? " app-header__nav--open" : ""}`}
          aria-label="Main navigation"
        >
          <div className="app-header__nav-inner">
            <Link to="/" className={navLinkClass("/")} onClick={closeMenu}>
              Characters
            </Link>
            <Link to="/campaigns" className={navLinkClass("/campaigns")} onClick={closeMenu}>
              Campaigns
            </Link>
          </div>
        </nav>

        <div className="app-header__actions">
          <button
            type="button"
            className={`app-header__menu-btn${menuOpen ? " app-header__menu-btn--open" : ""}`}
            onClick={() => setMenuOpen((open) => !open)}
            aria-expanded={menuOpen}
            aria-controls="app-header-nav"
            aria-label={menuOpen ? "Close menu" : "Open menu"}
          >
            <span />
            <span />
            <span />
          </button>
          <button
            type="button"
            className="app-header__logout"
            onClick={handleLogout}
            aria-label="Log out"
          >
            <svg className="app-header__logout-icon" viewBox="0 0 24 24" aria-hidden>
              <path
                fill="currentColor"
                d="M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5-5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z"
              />
            </svg>
            <span>Log out</span>
          </button>
        </div>
      </div>

      {menuOpen && (
        <button
          type="button"
          className="app-header__backdrop"
          aria-label="Close menu"
          onClick={closeMenu}
        />
      )}
    </header>
  );
}
