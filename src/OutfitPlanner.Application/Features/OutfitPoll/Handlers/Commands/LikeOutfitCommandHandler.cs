using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.OutfitPoll.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Application.Features.OutfitPoll.Handlers.Commands;

public class LikeOutFitCommandHandler : IRequestHandler<LikeOutFitCommand, OutFitVoteResultDto>
{
    private readonly AppDbContext _context;

    public LikeOutFitCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OutFitVoteResultDto> Handle(LikeOutFitCommand request, CancellationToken cancellationToken)
    {
        // Find or create outfit poll
        var poll = await GetOrCreateOutFITPollAsync(request.OutFITId);
        if (poll == null)
            return new OutFitVoteResultDto { ErrorMessage = "OUTFIT not found" };

        var option = poll.Options.FirstOrDefault();
        if (option == null)
            return new OutFitVoteResultDto { ErrorMessage = "Invalid outfit poll configuration" };

        // Check if user already voted
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.OptionId == option.Id && v.VoterId == request.UserId, cancellationToken);

        if (existingVote != null)
        {
            // Already liked, return current state
            return new OutFitVoteResultDto
            {
                Success = true,
                Message = "Already liked",
                UserHasVoted = true,
                VoteCount = await GetVotecountAsync(option.Id),
                RatingGiven= existingVote.Rating ?? 0 
            };
            
            
            
            
            

        
    
}
private async Task<int>GetVotecountAsync(Guid optionid){
return await _context.Votes.Countasync(v=>v.Optionid==optionid);
}

private async Task <Validation Poll?>GetorCreateoutFItPOllAsynC(Guid outFItID){
var poll=await_context.Validationpolls.include(p=>p.options).Firstordefaultasync(p=>p.Context.contains($"\"outFITID\":\"{outFITID}\"")|| p.Options.Any(o=>o.outFITID==outFITID));
if(poll!=null)returnpoll;
//createnewpollforthe out FIT 
poll=new Validation Poll{
 Id=Guid.NewGuid(),
 Question="OUT FITRating",
 Context=$"{{\"OUT FIT ID\":\"{OUT FIT ID}\"}}",Status=PollStatus.Active,
 ExpiresAt=DateTime.UtcNow.AddDays(30),CreatedAt=DateTime.UtcNow,
 Options=new List<PollOption>{new PollOption{ Id=Guid.Newguid(),DisplayOrder=1 ,CreatedAt DateTime.UtcNow}}
};
_context.Validation Polls.Add(poll);await_context.SaveChangesasync(cancellation Token);return.poll;}
}
