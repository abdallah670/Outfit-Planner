/**
 * Outfit post entity
 */
export interface OutfitPost {
  id: string;
  userId: string;
  userName: string;
  userAvatar: string;
  outfitId: string;
  outfitName: string;
  outfitImageUrl: string;
  likes: number;
  comments: number;
  createdAt: Date;
}
