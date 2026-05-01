import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PollsRepository, POLLS_REPOSITORY } from '../../domain/repositories/polls.repository';
import { PollsDataSource } from '../../data/datasources/polls.datasource';
import {
  Poll,
  CreatePollRequest,
  CastVoteRequest,
} from '../../domain/entities/poll.entity';
import { CommandResponse } from '../../domain/entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class PollsRepositoryImpl implements PollsRepository {
  private pollsDataSource = inject(PollsDataSource);

  getPolls(): Observable<Poll[]> {
    return this.pollsDataSource.getPolls();
  }

  getPollById(id: string): Observable<Poll> {
    return this.pollsDataSource.getPollById(id);
  }

  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.pollsDataSource.createPoll(dto);
  }

  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.pollsDataSource.vote(pollId, dto);
  }

  updatePoll(pollId: string, request: any): Observable<void> {
    return this.pollsDataSource.updatePoll(pollId, request);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.pollsDataSource.deletePoll(pollId);
  }

  closePoll(pollId: string): Observable<void> {
    return this.pollsDataSource.closePoll(pollId);
  }

  uploadPollImage(file: File): Observable<string> {
    return this.pollsDataSource.uploadPollImage(file);
  }

  getRecentPollWithComments(cursor?: string, pageSize?: number): Observable<{ poll: Poll; comments: any[] }> {
    return this.pollsDataSource.getRecentPollWithComments(cursor, pageSize);
  }
}

export const pollsRepositoryProvider = {
  provide: POLLS_REPOSITORY,
  useClass: PollsRepositoryImpl,
};
