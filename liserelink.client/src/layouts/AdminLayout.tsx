import { useState } from 'react';
import { Outlet, NavLink, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';

const NAV_ITEMS = [
  { to: '/admin/stock', label: 'Stock' },
  { to: '/admin/catalogue', label: 'Catalogue' },
  { to: '/admin/requests', label: 'Demandes' },
  { to: '/admin/users', label: 'Utilisateurs' },
] as const;

const PAGE_TITLES: Record<string, string> = {
  '/admin/stock': 'Stock',
  '/admin/catalogue': 'Catalogue',
  '/admin/requests': 'Demandes',
  '/admin/users': 'Utilisateurs',
};

export default function AdminLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const selectedStoreName = useAuthStore((s) => s.selectedStoreName);
  const location = useLocation();
  const pageTitle = PAGE_TITLES[location.pathname] ?? 'Admin';

  return (
    <div className="flex min-h-screen bg-[#f9f4ef]">
      {/* Sidebar overlay on tablet/mobile */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black/40 z-30 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={[
          'fixed top-0 left-0 h-full w-[200px] bg-[#121212] flex flex-col z-40',
          'transition-transform duration-200',
          'lg:translate-x-0 lg:static lg:shrink-0',
          sidebarOpen ? 'translate-x-0' : '-translate-x-full',
        ].join(' ')}
      >
        {/* Logo */}
        <div className="h-20 flex items-center px-6 border-b border-white/10">
          <span className="font-['Libre_Baskerville'] text-white text-xl tracking-widest">
            LISERE
          </span>
        </div>

        {/* Nav */}
        <nav className="flex-1 py-4">
          {NAV_ITEMS.map(({ to, label }) => (
            <NavLink
              key={to}
              to={to}
              onClick={() => setSidebarOpen(false)}
              className={({ isActive }) =>
                `block px-6 py-4 font-[Oswald] text-[12px] tracking-[2px] uppercase transition-colors ${
                  isActive
                    ? 'text-white border-l-2 border-white bg-white/5'
                    : 'text-[#969696] hover:text-white'
                }`
              }
            >
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>

      {/* Main area */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Top bar */}
        <header className="h-16 bg-white border-b border-[#e1e1e1] flex items-center justify-between px-6 shrink-0">
          <div className="flex items-center gap-4">
            {/* Hamburger — hidden on lg+ */}
            <button
              type="button"
              onClick={() => setSidebarOpen(true)}
              className="lg:hidden p-2 -ml-2 text-[#121212]"
              aria-label="Ouvrir le menu"
            >
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none" aria-hidden="true">
                <path
                  d="M2 5h16M2 10h16M2 15h16"
                  stroke="currentColor"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                />
              </svg>
            </button>
            <h1 className="font-['Libre_Baskerville'] text-[#121212] text-lg">{pageTitle}</h1>
          </div>

          {/* Store name */}
          {selectedStoreName && (
            <span className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696]">
              {selectedStoreName}
            </span>
          )}
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
