import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
  CommandResponse,
} from '../entities/validation-poll.entity';

export const SOCIAL_REPOSITORY = new InjectionToken<SocialRepository>('SocialRepository');

/**
 * Repository interface for social-related operations
 */
export interface SocialRepository {
  /**
   * Get all polls for the current user
   */
  getPolls(): Observable<ValidationPoll[]>;

  /**
   * Get a specific poll by ID
   */
  getPollById(id: string): Observable<ValidationPoll>;

  /**
   * Create a new poll
   */
  createPoll(dto: CreatePollRequest): Observable<CommandResponse>;

  /**
   * Cast a vote on a poll
   */
  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse>;
}
