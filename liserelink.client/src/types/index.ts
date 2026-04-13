import type {
  ClothingFamily,
  RequestLineStatus,
  RequestStatus,
  Size,
  UserRole,
  ZoneType,
} from '../constants/enums';

export interface ArticleDto {
  id: string;
  barcode: string;
  family: ClothingFamily;
  name: string;
  colorOrPrint: string;
  availableSizes: Size[];
  price?: number;
  imageUrl?: string;
}

export interface StockDto {
  articleId: string;
  size: Size;
  availableQuantity: number;
  storeId: string;
}

export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  assignedZone?: ZoneType;
}

export interface StoreDto {
  id: string;
  name: string;
  code: string;
}

export interface AlternativeRequestLineDto {
  id: string;
  requestId: string;
  articleId: string;
  articleName: string;
  articleColorOrPrint: string;
  articleBarcode: string;
  requestedSizes: Size[];
  quantity: number;
  status: RequestLineStatus;
  stockOverride: boolean;
}

export interface RequestLineDto {
  id: string;
  requestId: string;
  articleId: string;
  articleName: string;
  articleBarcode: string;
  colorOrPrint: string;
  requestedSizes: Size[];
  quantity: number;
  status: RequestLineStatus;
  alternativeArticleId?: string;
  alternativeColorOrPrint?: string;
  alternativeSizes?: Size[];
  alternativeStockOverride?: boolean;
}

export interface RequestDto {
  id: string;
  sellerId: string;
  sellerFirstName: string;
  sellerLastName: string;
  stockistId?: string;
  stockistFirstName?: string;
  stockistLastName?: string;
  storeId: string;
  zone: ZoneType;
  status: RequestStatus;
  lines: RequestLineDto[];
  alternativeLines: AlternativeRequestLineDto[];
  createdAt: string;
  completedAt?: string;
  cancelledAt?: string;
}

export interface AuthUserDto {
  id: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface AuthResponseDto {
  token: string;
  expiresAt: string;
  user: AuthUserDto;
}

export interface Notification {
  id: string;
  message: string;
  type: 'info' | 'success' | 'warning';
  requestId?: string;
  createdAt: string;
}

export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
