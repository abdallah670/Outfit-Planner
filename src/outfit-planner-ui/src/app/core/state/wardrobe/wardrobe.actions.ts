// -`loadClothingItems` / `loadClothingItemsSuccess` / `loadClothingItemsFailure` -
//   `loadClothingItemById` / `loadClothingItemByIdSuccess` -
//   `loadClothingItemsByCategory` / `loadClothingItemsByCategorySuccess` -
//   `createClothingItem` / `createClothingItemSuccess` -
//   `updateClothingItem` / `updateClothingItemSuccess` -
//   `deleteClothingItem` / `deleteClothingItemSuccess` -
//   `recordWear` / `recordWearSuccess`;
import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';
export const WardrobeActions = createActionGroup({
  source: 'wardrobe',
  events: {
    'Load Clothing Items': emptyProps(),
    'Load Clothing Items Success': props<{ items: ClothingItem[] }>(),
    'Load Clothing Items Failure': props<{ error: string }>(),

    'Load Clothing Item By Id': props<{ id: string }>(),
    'Load Clothing Item By Id Success': props<{ item: ClothingItem }>(),
    'Load Clothing Item By Id Failure': props<{ error: string }>(),

    'Load Clothing Items By Category': props<{ category: string }>(),
    'Load Clothing Items By Category Success': props<{
      items: ClothingItem[];
    }>(),
    'Load Clothing Items By Category Failure': props<{ error: string }>(),

    'Create Clothing Item': props<{
      item: Partial<ClothingItem>;
      image?: File;
    }>(),
    'Create Clothing Item Success': props<{ item: ClothingItem }>(),
    'Create Clothing Item Failure': props<{ error: string }>(),

    'Update Clothing Item': props<{
      id: string;
      item: Partial<ClothingItem>;
      image?: File;
    }>(),
    'Update Clothing Item Success': props<{ item: ClothingItem }>(),
    'Update Clothing Item Failure': props<{ error: string }>(),

    'Delete Clothing Item': props<{ id: string }>(),
    'Delete Clothing Item Success': props<{ id: string }>(),
    'Delete Clothing Item Failure': props<{ error: string }>(),

    'Record Wear': props<{ id: string }>(),
    'Record Wear Success': props<{ item: ClothingItem }>(),
    'Record Wear Failure': props<{ error: string }>(),
  },
});
