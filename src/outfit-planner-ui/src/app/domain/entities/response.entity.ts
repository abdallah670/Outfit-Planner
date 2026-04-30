export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
export interface CursorPagedResult<T> {
  items: T[];
  nextCursor: string | null;
  pageSize: number;
  hasMore: boolean;
}
/**
 * Response from command operations (create poll, vote)
 */
export interface CommandResponse {
  id: string;
  success: boolean;
  message: string;
  errors: string[];
}


