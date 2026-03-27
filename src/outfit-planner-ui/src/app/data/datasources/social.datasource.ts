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
import {
  TrendingOutfit,
  OutfitEngagement,
  OutfitComment,
} from '../../domain/entities/social-engagement.entity';

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
  outfitId: string;
  outfitImageUrl: string;
  userName: string;
  userAvatarUrl: string;
  likeCount: number;
  commentCount: number;
  trendingScore: number;
  rankPosition: number;
  createdAt: string;
}

/**
 * DTO for outfit engagement
 */
interface OutfitEngagementDto {
  outfitId: string;
  likeCount: number;
  commentCount: number;
  userHasLiked: boolean;
}

/**
 * DTO for outfit comment
 */
interface OutfitCommentDto {
  id: string;
  outfitId: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  content: string;
  createdAt: string;
  isDeleted: boolean;
}

export interface OutfitVoteResult {
  outfitId: string;
  voteCount: number;
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
  commentOnOutfit(outfitId: string, content: string): Observable<OutfitComment> {
    return this.http
      .post<OutfitCommentDto>(
        `${this.outfitPollApiUrl}/outfits/${outfitId}/comment`,
        { content },
      )
      .pipe(map((dto: OutfitCommentDto) => this.mapCommentDtoToEntity(dto)));
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
  ): Observable<{ items: OutfitComment[]; totalCount: number }> {
    return this.http
      .get<{
        items: OutfitCommentDto[];
        totalCount: number;
      }>(
        `${this.outfitPollApiUrl}/outfits/${outfitId}/votes?page=${page}&pageSize=${pageSize}`,
      )
      .pipe(
        map((result: { items: OutfitCommentDto[]; totalCount: number }) => ({
          items: result.items.map((dto: OutfitCommentDto) => this.mapCommentDtoToEntity(dto)),
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
      userId: '', // Not in DTO anymore for simplicity or fetch via userName
      userName: dto.userName,
      userAvatar: dto.userAvatarUrl,
      imageUrl: dto.outfitImageUrl,
      likes: dto.likeCount,
      comments: dto.commentCount,
      occasion: 'Casual', // Default or handle if added to DTO
      trendingScore: dto.trendingScore,
      createdAt: new Date(dto.createdAt),
    };
  }

  /**
   * Map comment DTO to domain entity
   */
  private mapCommentDtoToEntity(dto: OutfitCommentDto): OutfitComment {
    return {
      id: dto.id,
      outfitId: dto.outfitId,
      userId: dto.userId,
      userName: dto.userName,
      userAvatarUrl: dto.userAvatarUrl,
      content: dto.content,
      createdAt: new Date(dto.createdAt),
      isDeleted: dto.isDeleted
    };
  }
}
