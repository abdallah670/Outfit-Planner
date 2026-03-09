import { Injectable } from '@angular/core';
import {
  SocialRepository,
  SOCIAL_REPOSITORY,
} from '../../domain/repositories/social.repository';
import { SocialDataSource } from '../datasources/social.datasource';
import { Observable } from 'rxjs';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
  CommandResponse,
} from '../../domain/entities/validation-poll.entity';

@Injectable({
  providedIn: 'root',
})
export class SocialRepositoryImpl implements SocialRepository {
  constructor(private readonly socialDataSource: SocialDataSource) {}

  getPolls(): Observable<ValidationPoll[]> {
    return this.socialDataSource.getPolls();
  }

  getPollById(id: string): Observable<ValidationPoll> {
    return this.socialDataSource.getPollById(id);
  }

  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.socialDataSource.createPoll(dto);
  }

  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.socialDataSource.vote(pollId, dto);
  }
}

export const socialRepositoryProvider = {
  provide: SOCIAL_REPOSITORY,
  useClass: SocialRepositoryImpl,
};
