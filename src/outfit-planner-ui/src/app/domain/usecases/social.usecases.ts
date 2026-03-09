import { Inject, Injectable } from '@angular/core';
import {
  SocialRepository,
  SOCIAL_REPOSITORY,
} from '../repositories/social.repository';
import { Observable } from 'rxjs';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
  CommandResponse,
} from '../entities/validation-poll.entity';

@Injectable({
  providedIn: 'root',
})
export class SocialUseCases {
  constructor(
    @Inject(SOCIAL_REPOSITORY) private readonly socialRepository: SocialRepository,
  ) {}

  /**
   * Get all polls for the current user
   */
  getPolls(): Observable<ValidationPoll[]> {
    return this.socialRepository.getPolls();
  }

  /**
   * Get a specific poll by ID
   */
  getPollById(id: string): Observable<ValidationPoll> {
    return this.socialRepository.getPollById(id);
  }

  /**
   * Create a new poll
   */
  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.socialRepository.createPoll(dto);
  }

  /**
   * Cast a vote on a poll
   */
  voteOnPoll(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.socialRepository.vote(pollId, dto);
  }
}
