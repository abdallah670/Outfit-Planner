import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { POLLS_REPOSITORY, PollsRepository } from '../repositories/polls.repository';
import {
  Poll,
  CreatePollRequest,
  UpdatePollRequest,
  CastVoteRequest,
} from '../entities/poll.entity';
import { CommandResponse } from '../entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class PollsUseCases {
  constructor(
    @Inject(POLLS_REPOSITORY) private readonly pollsRepository: PollsRepository,
  ) {}

  // getPolls(): Observable<Poll[]> {
  //   return this.pollsRepository.getPolls
  // }

  getMyPolls(): Observable<Poll[]> {
    return this.pollsRepository.getUserPolls();
  }

  getPollById(id: string): Observable<Poll> {
    return this.pollsRepository.getPollById(id);
  }

  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.pollsRepository.createPoll(dto);
  }

  voteOnPoll(pollId: string, optionId: string): Observable<CommandResponse> {
    return this.pollsRepository.vote(pollId, {optionId});
  }

  removeVote( optionId: string): Observable<void> {
    return this.pollsRepository.removeVote(optionId);
  }

  updatePoll(pollId: string, request: UpdatePollRequest): Observable<Poll> {
    return this.pollsRepository.updatePoll(pollId, request);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.pollsRepository.deletePoll(pollId);
  }

  closePoll(pollId: string): Observable<void> {
    return this.pollsRepository.closePoll(pollId);
  }

  // uploadPollImage(file: File): Observable<string> {
  //   return this.pollsRepository.uploadPollImage(file);
  // }

  getRecentPollWithComments(cursor?: string, pageSize?: number): Observable<{ poll: Poll; comments: any[]; commentsCursor?: string | null; hasMoreComments?: boolean }> {
    return this.pollsRepository.getRecentPollWithComments(cursor, pageSize);
  }
}
