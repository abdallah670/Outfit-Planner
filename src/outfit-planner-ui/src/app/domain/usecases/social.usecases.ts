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
import {
  TrendingOutfit,
  OutfitEngagement,
  VoteComment,
  AddVoteCommentRequest,
} from '../entities/social-engagement.entity';

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

  /**
   * Get trending outfits
   */
  getTrendingOutfits(): Observable<TrendingOutfit[]> {
    return this.socialRepository.getTrendingOutfits();
  }

  /**
   * React to a vote
   */
  reactToVote(voteId: string, reactionType: string): Observable<void> {
    return this.socialRepository.reactToVote(voteId, reactionType);
  }

  /**
   * Add a comment to a vote
   */
  addVoteComment(request: AddVoteCommentRequest): Observable<VoteComment> {
    return this.socialRepository.addVoteComment(request);
  }

  /**
   * Like a vote comment
   */
  likeVoteComment(commentId: string): Observable<void> {
    return this.socialRepository.likeVoteComment(commentId);
  }

  /**
   * Get comments for a vote
   */
  getVoteComments(voteId: string, maxDepth?: number): Observable<VoteComment[]> {
    return this.socialRepository.getVoteComments(voteId, maxDepth);
  }
}
