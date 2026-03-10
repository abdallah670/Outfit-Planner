import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
  CommandResponse,
  PollOption,
  PollStatus,
} from '../../domain/entities/validation-poll.entity';

// DTOs matching the API response structure
interface ValidationPollDto {
  id: string;
  userId: string;
  question: string;
  context: string;
  expiresAt: string;
  status: string;
  options: PollOptionDto[];
  totalVotes: number;
  createdAt: string;
}

interface PollOptionDto {
  id: string;
  pollId?: string;
  outfitId?: string;
  description: string;
  displayOrder: number;
  voteCount: number;
  outfitThumbnail?: string;
}

/**
 * DTO for trending outfit from API
 */
interface TrendingOutfitDto {
  id: string;
  userId: string;
  userName: string;
  userAvatar: string;
  imageUrl: string;
  likes: number;
  occasion: string;
  createdAt: string;
}

/**
 * Domain entity for trending outfit
 */
export interface TrendingOutfit {
  id: string;
  userId: string;
  userName: string;
  userAvatar: string;
  imageUrl: string;
  likes: number;
  occasion: string;
  createdAt: Date;
}

/**
 * DTO for outfit engagement
 */
interface OutfitEngagementDto {
  voteCount: number;
  commentCount: number;
  reactionCount: number;
  userHasVoted: boolean;
  userReaction?: string;
}

/**
 * DTO for vote comment
 */
interface VoteCommentDto {
  id: string;
  userName: string;
  userAvatar?: string;
  content: string;
  rating: number;
  createdAt: string;
  reactions: VoteReactionDto[];
}

interface VoteReactionDto {
  userId: string;
  reactionType: string;
}

/**
 * Domain entity for outfit engagement
 */
export interface OutfitEngagement {
  voteCount: number;
  commentCount: number;
  reactionCount: number;
  userHasVoted: boolean;
  userReaction?: string;
}

/**
 * Domain entity for vote comment
 */
export interface VoteComment {
  id: string;
  userName: string;
  userAvatar?: string;
  content: string;
  rating: number;
  createdAt: Date;
  reactions: VoteReaction[];
}

export interface VoteReaction {
  userId: string;
  reactionType: string;
}

export interface OutfitVoteResult {
  outfitId: string;
  voteCount: number;
  userHasVoted: boolean;
}

export interface UpdatePollRequest {
  question?: string;
  context?: string;
  expiresAt?: string;
  options?: UpdatePollOptionRequest[];
}

export interface UpdatePollOptionRequest {
  id?: string;
  description?: string;
  displayOrder: number;
  outfitId?: string;
}

@Injectable({
  providedIn: 'root',
})
export class SocialDataSource {
  private readonly apiUrl = `${environment.baseUrl}/social`;
  private readonly outfitPollApiUrl = `${environment.baseUrl}/outfit-polls`;

  constructor(private http: HttpClient) {}

  // ============ Outfit Poll (Like/Comment) Methods ============

  /**
   * Like an outfit (creates a vote)
   */
  likeOutfit(outfitId: string): Observable<OutfitVoteResult> {
    return this.http.post<OutfitVoteResult>(
      `${this.outfitPollApiUrl}/outfits/${outfitId}/like`,
      {},
    );
  }

  /**
   * Unlike an outfit (removes vote)
   */
  unlikeOutfit(outfitId: string): Observable<OutfitVoteResult> {
    return this.http.delete<OutfitVoteResult>(
      `${this.outfitPollApiUrl}/outfits/${outfitId}/like`,
    );
  }

  /**
   * Comment on an outfit
   */
  commentOnOutfit(outfitId: string, content: string): Observable<VoteComment> {
    return this.http
      .post<VoteCommentDto>(
        `${this.outfitPollApiUrl}/outfits/${outfitId}/comment`,
        { content },
      )
      .pipe(map((dto: VoteCommentDto) => this.mapVoteCommentDtoToEntity(dto)));
  }

  /**
   * Get outfit engagement stats
   */
  getOutfitEngagement(outfitId: string): Observable<OutfitEngagement> {
    return this.http.get<OutfitEngagementDto>(
      `${this.outfitPollApiUrl}/outfits/${outfitId}/engagement`,
    );
  }

  /**
   * Get votes/comments for an outfit
   */
  getOutfitVotes(
    outfitId: string,
    page = 1,
    pageSize = 20,
  ): Observable<{ items: VoteComment[]; totalCount: number }> {
    return this.http
      .get<{
        items: VoteCommentDto[];
        totalCount: number;
      }>(
        `${this.outfitPollApiUrl}/outfits/${outfitId}/votes?page=${page}&pageSize=${pageSize}`,
      )
      .pipe(
        map((result: { items: VoteCommentDto[]; totalCount: number }) => ({
          items: result.items.map((dto: VoteCommentDto) => this.mapVoteCommentDtoToEntity(dto)),
          totalCount: result.totalCount,
        })),
      );
  }

  /**
   * React to a vote/comment
   */
  reactToVote(
    voteId: string,
    reactionType: string,
  ): Observable<void> {
    return this.http.post<void>(
      `${this.outfitPollApiUrl}/votes/${voteId}/react`,
      { reactionType },
    );
  }

  // ============ Existing Poll Methods ============

  /**
   * Get all polls for the current user
   */
  getPolls(): Observable<ValidationPoll[]> {
    return this.http
      .get<ValidationPollDto[]>(`${this.apiUrl}/polls`)
      .pipe(
        map((polls: ValidationPollDto[]) =>
          polls.map((p: ValidationPollDto) => this.mapPollDtoToEntity(p)),
        ),
      );
  }

  /**
   * Get a specific poll by ID
   */
  getPollById(id: string): Observable<ValidationPoll> {
    return this.http
      .get<ValidationPollDto>(`${this.apiUrl}/polls/${id}`)
      .pipe(map((poll: ValidationPollDto) => this.mapPollDtoToEntity(poll)));
  }

  /**
   * Create a new poll
   */
  createPoll(dto: CreatePollRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}/polls`, dto);
  }

  /**
   * Cast a vote on a poll
   */
  vote(pollId: string, dto: CastVoteRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}/polls/${pollId}/vote`, dto);
  }

  /**
   * Update a poll
   */
  updatePoll(pollId: string, request: UpdatePollRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/polls/${pollId}`, request);
  }

  /**
   * Delete a poll
   */
  deletePoll(pollId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/polls/${pollId}`);
  }

  /**
   * Close a poll
   */
  closePoll(pollId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/polls/${pollId}/close`, {});
  }

  /**
   * Get trending outfits from the community
   */
  getTrendingOutfits(): Observable<TrendingOutfit[]> {
    return this.http
      .get<TrendingOutfitDto[]>(`${this.apiUrl}/trending-outfits`)
      .pipe(
        map((outfits: TrendingOutfitDto[]) =>
          outfits.map((o: TrendingOutfitDto) => this.mapTrendingOutfitDtoToEntity(o)),
        ),
      );
  }

  /**
   * Map API DTO to domain entity
   */
  private mapPollDtoToEntity(dto: ValidationPollDto): ValidationPoll {
    return {
      id: dto.id,
      userId: dto.userId,
      question: dto.question,
      context: dto.context,
      expiresAt: new Date(dto.expiresAt),
      status: this.mapStatusToEnum(dto.status),
      options: dto.options.map((o: PollOptionDto) => this.mapOptionDtoToEntity(o)),
      totalVotes: dto.totalVotes,
      createdAt: new Date(dto.createdAt),
    };
  }

  /**
   * Map API status string to PollStatus enum
   */
  private mapStatusToEnum(status: string): PollStatus {
    switch (status.toLowerCase()) {
      case 'active':
        return PollStatus.Active;
      case 'expired':
        return PollStatus.Expired;
      case 'closed':
        return PollStatus.Closed;
      default:
        return PollStatus.Active;
    }
  }

  /**
   * Map option DTO to domain entity
   */
  private mapOptionDtoToEntity(dto: PollOptionDto): PollOption {
    return {
      id: dto.id,
      pollId: dto.pollId,
      outfitId: dto.outfitId,
      description: dto.description,
      displayOrder: dto.displayOrder,
      voteCount: dto.voteCount,
      outfitThumbnail: dto.outfitThumbnail,
    };
  }

  /**
   * Map trending outfit DTO to domain entity
   */
  private mapTrendingOutfitDtoToEntity(dto: TrendingOutfitDto): TrendingOutfit {
    return {
      id: dto.id,
      userId: dto.userId,
      userName: dto.userName,
      userAvatar: dto.userAvatar,
      imageUrl: dto.imageUrl,
      likes: dto.likes,
      occasion: dto.occasion,
      createdAt: new Date(dto.createdAt),
    };
  }

  /**
   * Map vote comment DTO to domain entity
   */
  private mapVoteCommentDtoToEntity(dto: VoteCommentDto): VoteComment {
    return {
      id: dto.id,
      userName: dto.userName,
      userAvatar: dto.userAvatar,
      content: dto.content,
      rating: dto.rating,
      createdAt: new Date(dto.createdAt),
      reactions: dto.reactions.map((r) => ({
        userId: r.userId,
        reactionType: r.reactionType,
      })),
    };
  }
}
