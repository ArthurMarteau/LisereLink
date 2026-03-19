import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { UserRole } from '@/constants/enums'
import AuthLayout from '@/layouts/AuthLayout'
import ProtectedRoute from '@/components/ProtectedRoute'
import LoginPage from '@/pages/LoginPage'
import StoreSelectionPage from '@/pages/StoreSelectionPage'
import UnauthorizedPage from '@/pages/UnauthorizedPage'

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />

        {/* Auth pages */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />

          {/* Requires authentication only (no role, no store check) */}
          <Route element={<ProtectedRoute />}>
            <Route path="/store-selection" element={<StoreSelectionPage />} />
          </Route>
        </Route>

        {/* Role-protected pages (also require store selection) */}
        <Route element={<ProtectedRoute role={UserRole.Seller} />}>
          <Route path="/search" element={<div>Recherche — TODO</div>} />
        </Route>

        <Route element={<ProtectedRoute role={UserRole.Stockist} />}>
          <Route path="/queue" element={<div>File d&apos;attente — TODO</div>} />
        </Route>

        <Route element={<ProtectedRoute role={UserRole.Admin} />}>
          <Route path="/admin" element={<div>Admin — TODO</div>} />
        </Route>

        <Route path="/unauthorized" element={<UnauthorizedPage />} />
      </Routes>
    </BrowserRouter>
  )
}
