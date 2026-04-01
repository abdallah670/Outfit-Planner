import { Injectable } from '@angular/core';
import {
  SocialRepository,
  SOCIAL_REPOSITORY,
} from '../../domain/repositories/social.repository';
import {
  SocialDataSource,
  UpdatePollRequest,
} from '../datasources/social.datasource';
import { Observable } from 'rxjs';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
  CommandResponse,
} from '../../domain/entities/validation-poll.entity';
import {
  TrendingOutfit,
  OutfitEngagement,
  VoteComment,
  AddVoteCommentRequest,
} from '../../domain/entities/social-engagement.entity';

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

  getTrendingOutfits(): Observable<TrendingOutfit[]> {
    return this.socialDataSource.getTrendingOutfits();
  }

  // ============ Vote Engagement Methods ============

  reactToVote(voteId: string, reactionType: string): Observable<void> {
    return this.socialDataSource.reactToVote(voteId, reactionType);
  }

  addVoteComment(request: AddVoteCommentRequest): Observable<VoteComment> {
    return this.socialDataSource.addVoteComment(request);
  }

  likeVoteComment(commentId: string): Observable<void> {
    return this.socialDataSource.likeVoteComment(commentId);
  }

  getVoteComments(voteId: string, maxDepth?: number): Observable<VoteComment[]> {
    return this.socialDataSource.getVoteComments(voteId, maxDepth);
  }

  // ============ Poll Management Methods ============

  updatePoll(pollId: string, request: UpdatePollRequest): Observable<void> {
    return this.socialDataSource.updatePoll(pollId, request);
  }

  deletePoll(pollId: string): Observable<void> {
    return this.socialDataSource.deletePoll(pollId);
  }

  closePoll(pollId: string): Observable<void> {
    return this.socialDataSource.closePoll(pollId);
  }
}

export const socialRepositoryProvider = {
  provide: SOCIAL_REPOSITORY,
  useClass: SocialRepositoryImpl,
};
