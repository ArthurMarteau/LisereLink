export enum RequestStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  AwaitingSellerResponse = 'AwaitingSellerResponse',
  Processed = 'Processed',
  PartiallyProcessed = 'PartiallyProcessed',
  Unavailable = 'Unavailable',
  Cancelled = 'Cancelled',
}

export enum RequestLineStatus {
  Pending = 'Pending',
  Found = 'Found',
  NotFound = 'NotFound',
  AlternativeProposed = 'AlternativeProposed',
  AlternativeDenied = 'AlternativeDenied',
}

export enum UserRole {
  Seller = 'Seller',
  Stockist = 'Stockist',
  Admin = 'Admin',
}

export enum ZoneType {
  RTW = 'RTW',
  FittingRooms = 'FittingRooms',
  Checkout = 'Checkout',
  Reception = 'Reception',
  Custom = 'Custom',
}

export enum ClothingFamily {
  COA = 'COA',
  JAC = 'JAC',
  TSH = 'TSH',
  SWE = 'SWE',
  VES = 'VES',
  JEA = 'JEA',
  PAN = 'PAN',
  SHO = 'SHO',
  SKI = 'SKI',
  DRE = 'DRE',
  SHI = 'SHI',
  BLO = 'BLO',
  SHE = 'SHE',
  BEL = 'BEL',
  BAG = 'BAG',
  JEW = 'JEW',
}

export enum Size {
  XXS = 'XXS',
  XS = 'XS',
  S = 'S',
  M = 'M',
  L = 'L',
  XL = 'XL',
  XXL = 'XXL',
  OneSize = 'OneSize',
}
