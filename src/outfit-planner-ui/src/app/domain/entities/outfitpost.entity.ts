/**
 * Outfit post entity
 */

export interface CreateOutfitPostRequest {
  outfitId: string;
  caption?: string;
  tags?: string[];
  visibility: number;
}

export interface UpdateOutfitPostRequest {
  outfitId?: string;
  caption?: string;
  visibility: number;
  tags?: string[];
}
