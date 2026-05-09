using System;
using System.Collections.Generic;
using System.Text;

namespace OutfitPlanner.Application.Responses
{
    public class BaseCommandResponse
    {
        public Guid Id { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public object? Data { get; set; }
    }
}
