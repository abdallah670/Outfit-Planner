import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { POLLS_REPOSITORY, PollsRepository } from '../repositories/polls.repository';
import {
  Poll,
  CreatePollRequest,
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

  getPolls(): Observable<Poll[]> {
    return this.pollsRepository.getPolls();
  }

  getPollById(id: string): Observable<Poll> {
    return this.pollsRepository.getPollById(id);
  }

  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.pollsRepository.createPoll(dto);
  }

  voteOnPoll(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.pollsRepository.vote(pollId, dto);
  }

  updatePoll(pollId: string, request: any): Observable<void> {
    return this.pollsRepository.updatePoll(pollId, request);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.pollsRepository.deletePoll(pollId);
  }

  closePoll(pollId: string): Observable<void> {
    return this.pollsRepository.closePoll(pollId);
  }

  uploadPollImage(file: File): Observable<string> {
    return this.pollsRepository.uploadPollImage(file);
  }
}
