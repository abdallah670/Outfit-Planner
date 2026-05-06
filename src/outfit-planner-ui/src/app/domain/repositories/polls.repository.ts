import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Poll,
  CreatePollRequest,
  UpdatePollRequest,
  CastVoteRequest,
} from '../entities/poll.entity';
import { CommandResponse } from '../entities/response.entity';

export const POLLS_REPOSITORY = new InjectionToken<PollsRepository>('PollsRepository');

export interface PollsRepository {
  getPolls(): Observable<Poll[]>;

  getUserPolls(): Observable<Poll[]>;

  getPollById(id: string): Observable<Poll>;

  createPoll(dto: CreatePollRequest): Observable<CommandResponse>;

  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse>;

  updatePoll(pollId: string, request: UpdatePollRequest): Observable<Poll>;

  deletePoll(pollId: string): Observable<void>;

  closePoll(pollId: string): Observable<void>;

  uploadPollImage(file: File): Observable<string>;
  
  getRecentPollWithComments(cursor?: string, pageSize?: number): Observable<{ poll: Poll; comments: any[]; commentsCursor?: string | null; hasMoreComments?: boolean }>;
}
