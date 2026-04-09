import {
  ClothingFamily,
  RequestLineStatus,
  RequestStatus,
  Size,
  ZoneType,
} from '../constants/enums';

export function formatDate(date: string): string {
  return new Intl.DateTimeFormat('fr-FR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Europe/Paris',
  }).format(new Date(date));
}

const SIZE_LABELS: Record<Size, string> = {
  [Size.XXS]: 'XXS',
  [Size.XS]: 'XS',
  [Size.S]: 'S',
  [Size.M]: 'M',
  [Size.L]: 'L',
  [Size.XL]: 'XL',
  [Size.XXL]: 'XXL',
  [Size.OneSize]: 'Taille unique',
};

export function formatSize(size: Size): string {
  return SIZE_LABELS[size];
}

const FAMILY_LABELS: Record<ClothingFamily, string> = {
  [ClothingFamily.COA]: 'Manteau',
  [ClothingFamily.JAC]: 'Veste',
  [ClothingFamily.TSH]: 'T-shirt',
  [ClothingFamily.SWE]: 'Sweat',
  [ClothingFamily.VES]: 'Gilet',
  [ClothingFamily.JEA]: 'Jean',
  [ClothingFamily.PAN]: 'Pantalon',
  [ClothingFamily.SHO]: 'Short',
  [ClothingFamily.SKI]: 'Jupe',
  [ClothingFamily.DRE]: 'Robe',
  [ClothingFamily.SHI]: 'Chemise',
  [ClothingFamily.BLO]: 'Blouse',
  [ClothingFamily.SHE]: 'Chaussures',
  [ClothingFamily.BEL]: 'Ceinture',
  [ClothingFamily.BAG]: 'Sac',
  [ClothingFamily.JEW]: 'Bijou',
};

export function formatFamily(family: ClothingFamily): string {
  return FAMILY_LABELS[family];
}

const ZONE_LABELS: Record<ZoneType, string> = {
  [ZoneType.RTW]: 'Prêt-à-porter',
  [ZoneType.FittingRooms]: 'Cabines',
  [ZoneType.Checkout]: 'Caisse',
  [ZoneType.Reception]: 'Accueil',
  [ZoneType.Custom]: 'Personnalisée',
};

export function formatZone(zone: ZoneType): string {
  return ZONE_LABELS[zone];
}

const REQUEST_STATUS_LABELS: Record<RequestStatus, string> = {
  [RequestStatus.Pending]: 'En attente',
  [RequestStatus.InProgress]: 'En cours',
  [RequestStatus.AwaitingSellerResponse]: 'En attente du vendeur',
  [RequestStatus.Delivered]: 'Livré',
  [RequestStatus.Unavailable]: 'Indisponible',
  [RequestStatus.Cancelled]: 'Annulée',
};

export function formatRequestStatus(status: RequestStatus): string {
  return REQUEST_STATUS_LABELS[status];
}

const REQUEST_LINE_STATUS_LABELS: Record<RequestLineStatus, string> = {
  [RequestLineStatus.Pending]: 'En attente',
  [RequestLineStatus.Found]: 'Trouvé',
  [RequestLineStatus.NotFound]: 'Introuvable',
  [RequestLineStatus.AlternativeProposed]: 'Alternative proposée',
};

export function formatRequestLineStatus(status: RequestLineStatus): string {
  return REQUEST_LINE_STATUS_LABELS[status];
}
