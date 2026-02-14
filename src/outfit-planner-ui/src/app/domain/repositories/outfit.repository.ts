import { Observable } from 'rxjs';
import { Outfit, WeatherCondition } from '../entities/outfit.entity';

export interface OutfitRepository {
  getAll(): Observable<Outfit[]>;
  getById(id: string): Observable<Outfit>;
  create(outfit: Partial<Outfit>): Observable<Outfit>;
  update(id: string, outfit: Partial<Outfit>): Observable<Outfit>;
  delete(id: string): Observable<boolean>;
  getSuggestions(weather: WeatherCondition, occasion: string): Observable<Outfit[]>;
  getTodaysOutfit(): Observable<Outfit>;
  recordWear(id: string): Observable<Outfit>;
}
