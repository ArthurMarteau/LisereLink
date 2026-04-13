import { useState, useEffect } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { ShoppingBag, LogOut } from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';
import { useCartStore } from '@/stores/useCartStore';
import { startConnection, stopConnection } from '@/services/signalRClient';
import SignalRBadge from '@/components/ui/SignalRBadge';
import ZoneChip from '@/components/ui/ZoneChip';
import ZoneSelectorModal from '@/pages/ZoneSelectorModal';

const NAV_ITEMS = [
  { to: '/search', label: 'Recherche' },
  { to: '/requests', label: 'Demandes' },
  { to: '/history', label: 'Historique' },
] as const;

export default function SellerLayout() {
  const navigate = useNavigate();
  const selectedStoreName = useAuthStore((s) => s.selectedStoreName);
  const selectedZone = useAuthStore((s) => s.selectedZone);
  const cartCount = useCartStore((s) => s.lines.length);
  const [isModalOpen, setIsModalOpen] = useState(selectedZone === null);

  useEffect(() => {
    void startConnection();
    return () => {
      void stopConnection();
    };
  }, []);

  return (
    <div className="flex flex-col min-h-screen bg-[#f9f4ef]">
      {/* Header */}
      <header className="bg-[#121212] h-20 flex items-center justify-between px-5 shrink-0">
        <span className="font-['Libre_Baskerville'] text-white text-base truncate mr-4">
          {selectedStoreName}
        </span>
        <div className="flex items-center gap-3 shrink-0">
          <ZoneChip onClick={() => setIsModalOpen(true)} />
          <SignalRBadge />
          <button
            type="button"
            onClick={() => navigate('/cart')}
            className="relative min-h-[44px] min-w-[44px] flex items-center justify-center"
            aria-label="Panier"
          >
            <ShoppingBag size={18} className="text-white" />
            {cartCount > 0 && (
              <span className="absolute top-1 right-1 bg-[#e51940] text-white font-[Oswald] text-[10px] w-4 h-4 rounded-full flex items-center justify-center">
                {cartCount}
              </span>
            )}
          </button>
          <button
            type="button"
            onClick={() => { useAuthStore.getState().logout(); navigate('/login'); }}
            className="min-h-[44px] min-w-[44px] flex items-center justify-center"
            aria-label="Déconnexion"
          >
            <LogOut size={18} className="text-white" />
          </button>
        </div>
      </header>

      {/* Page content — padded above bottom nav */}
      <main className="flex-1 overflow-y-auto pb-16">
        <Outlet />
      </main>

      {/* Bottom navigation */}
      <nav className="fixed bottom-0 inset-x-0 h-16 bg-white border-t border-[#e1e1e1] flex z-40">
        {NAV_ITEMS.map(({ to, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              `flex-1 flex flex-col items-center justify-center font-[Oswald] text-[11px] tracking-[1.5px] uppercase relative min-h-[44px] ${
                isActive ? 'text-[#121212]' : 'text-[#969696]'
              }`
            }
          >
            {({ isActive }) => (
              <>
                {isActive && <span className="absolute top-0 inset-x-4 h-0.5 bg-[#121212]" />}
                {label}
              </>
            )}
          </NavLink>
        ))}
      </nav>

      {isModalOpen && (
        <ZoneSelectorModal
          onClose={() => setIsModalOpen(false)}
          initialZone={selectedZone}
        />
      )}
    </div>
  );
}
