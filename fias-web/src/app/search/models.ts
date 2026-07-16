export interface FiasSearchRequest {
  query?: string;
  typeName?: string;
  levelId?: number | null;
  regionCode?: string;
  onlyActive: boolean;
  page: number;
  pageSize: number;
}

export interface FiasAddressSearchResult {
  objectId: number;
  objectGuid: string;
  parentObjectId?: number | null;
  name: string;
  typeName: string;
  fullName: string;
  fullAddress: string;
  levelId: number;
  isActive: boolean;
  regionCode?: string | null;
}

export interface FiasSearchResponse {
  page: number;
  pageSize: number;
  total: number;
  items: FiasAddressSearchResult[];
}
