import { Temporal } from '@js-temporal/polyfill';
import {
  ClothingFamily,
  RequestLineStatus,
  RequestStatus,
  Size,
  ZoneType,
} from '../constants/enums';

export function formatDate(date: string): string {
  const instant = Temporal.Instant.from(
    date.endsWith('Z') ? date : date + 'Z'
  );
  const zdt = instant.toZonedDateTimeISO('Europe/Paris');
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${pad(zdt.day)}/${pad(zdt.month)}/${zdt.year} ${pad(zdt.hour)}:${pad(zdt.minute)}`;
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
  [RequestStatus.Processed]: 'Traitée',
  [RequestStatus.PartiallyProcessed]: 'Traitée partiellement',
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
  [RequestLineStatus.AlternativeDenied]: 'Refusée',
};

export function formatRequestLineStatus(status: RequestLineStatus): string {
  return REQUEST_LINE_STATUS_LABELS[status];
}
