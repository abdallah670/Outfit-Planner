import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PollsRepository, POLLS_REPOSITORY } from '../../domain/repositories/polls.repository';
import { PollsDataSource } from '../../data/datasources/polls.datasource';
import {
  Poll,
  CreatePollRequest,
  UpdatePollRequest,
  CastVoteRequest,
} from '../../domain/entities/poll.entity';
import { CommandResponse } from '../../domain/entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class PollsRepositoryImpl implements PollsRepository {
  private pollsDataSource = inject(PollsDataSource);

  

  getUserPolls(): Observable<Poll[]> {
    return this.pollsDataSource.getUserPolls();
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

  removeVote(optionId:string): Observable<void> {
    return this.pollsDataSource.removeVote(optionId);
  }

  updatePoll(pollId: string, request: UpdatePollRequest): Observable<Poll> {
    return this.pollsDataSource.updatePoll(pollId, request);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.pollsDataSource.deletePoll(pollId);
  }

  closePoll(pollId: string): Observable<void> {
    return this.pollsDataSource.closePoll(pollId);
  }

  getRecentPollWithComments(cursor?: string, pageSize?: number): Observable<{ poll: Poll; comments: any[] }> {
    return this.pollsDataSource.getRecentPollWithComments(cursor, pageSize);
  }
}

export const pollsRepositoryProvider = {
  provide: POLLS_REPOSITORY,
  useClass: PollsRepositoryImpl,
};
