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
  OutfitComment,
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
   * Like an outfit
   */
  likeOutfit(outfitId: string): Observable<{ outfitId: string; voteCount: number }> {
    return this.socialRepository.likeOutfit(outfitId);
  }

  /**
   * Unlike an outfit
   */
  unlikeOutfit(outfitId: string): Observable<{ outfitId: string; voteCount: number }> {
    return this.socialRepository.unlikeOutfit(outfitId);
  }

  /**
   * Add a comment
   */
  addComment(outfitId: string, content: string): Observable<OutfitComment> {
    return this.socialRepository.commentOnOutfit(outfitId, content);
  }

  /**
   * Get comments for an outfit
   */
  getComments(outfitId: string, page = 1, pageSize = 20): Observable<{ items: OutfitComment[]; totalCount: number }> {
    return this.socialRepository.getOutfitVotes(outfitId, page, pageSize);
  }
}
