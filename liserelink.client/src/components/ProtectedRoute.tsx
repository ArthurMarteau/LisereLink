import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore, selectIsAuthenticated, selectHasRole } from '@/stores/authStore'
import type { UserRole } from '@/constants/enums'

interface ProtectedRouteProps {
  role?: UserRole
}

export default function ProtectedRoute({ role }: ProtectedRouteProps) {
  const state = useAuthStore()

  if (!selectIsAuthenticated(state)) {
    return <Navigate to="/login" replace />
  }

  if (role !== undefined && !selectHasRole(role)(state)) {
    return <Navigate to="/unauthorized" replace />
  }

  if (role !== undefined && state.selectedStoreId === null) {
    return <Navigate to="/store-selection" replace />
  }

  return <Outlet />
}
