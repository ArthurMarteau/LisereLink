import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { UserRole } from '@/constants/enums';
import AuthLayout from '@/layouts/AuthLayout';
import SellerLayout from '@/layouts/SellerLayout';
import StockistLayout from '@/layouts/StockistLayout';
import AdminLayout from '@/layouts/AdminLayout';
import ProtectedRoute from '@/components/ProtectedRoute';
import LoginPage from '@/pages/LoginPage';
import StoreSelectionPage from '@/pages/StoreSelectionPage';
import UnauthorizedPage from '@/pages/UnauthorizedPage';
import SearchPage from '@/pages/seller/SearchPage';
import ScanPage from '@/pages/seller/ScanPage';
import ArticleDetailPage from '@/pages/seller/ArticleDetailPage';
import RequestsPage from '@/pages/seller/RequestsPage';
import CartPage from '@/pages/seller/CartPage';
import SellerHistoryPage from '@/pages/seller/HistoryPage';
import QueuePage from '@/pages/stockist/QueuePage';
import InProgressPage from '@/pages/stockist/InProgressPage';
import StockistHistoryPage from '@/pages/stockist/HistoryPage';
import AdminStockPage from '@/pages/admin/StockPage';
import CataloguePage from '@/pages/admin/CataloguePage';
import AdminRequestsPage from '@/pages/admin/RequestsPage';
import UsersPage from '@/pages/admin/UsersPage';

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />

        {/* Auth */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route path="/store-selection" element={<StoreSelectionPage />} />
          </Route>
        </Route>

        {/* Seller */}
        <Route element={<ProtectedRoute role={UserRole.Seller} />}>
          <Route element={<SellerLayout />}>
            <Route path="/search" element={<SearchPage />} />
            <Route path="/scan" element={<ScanPage />} />
            <Route path="/article/:id" element={<ArticleDetailPage />} />
            <Route path="/requests" element={<RequestsPage />} />
            <Route path="/cart" element={<CartPage />} />
            <Route path="/history" element={<SellerHistoryPage />} />
          </Route>
        </Route>

        {/* Stockist */}
        <Route element={<ProtectedRoute role={UserRole.Stockist} />}>
          <Route element={<StockistLayout />}>
            <Route path="/queue" element={<QueuePage />} />
            <Route path="/in-progress" element={<InProgressPage />} />
            <Route path="/stockist-history" element={<StockistHistoryPage />} />
          </Route>
        </Route>

        {/* Admin */}
        <Route element={<ProtectedRoute role={UserRole.Admin} />}>
          <Route element={<AdminLayout />}>
            <Route path="/admin" element={<Navigate to="/admin/stock" replace />} />
            <Route path="/admin/stock" element={<AdminStockPage />} />
            <Route path="/admin/catalogue" element={<CataloguePage />} />
            <Route path="/admin/requests" element={<AdminRequestsPage />} />
            <Route path="/admin/users" element={<UsersPage />} />
          </Route>
        </Route>

        <Route path="/unauthorized" element={<UnauthorizedPage />} />
      </Routes>
    </BrowserRouter>
  );
}
