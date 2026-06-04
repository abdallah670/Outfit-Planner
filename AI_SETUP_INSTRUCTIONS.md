# AI Setup Instructions for Outfit Planner

## Overview
This guide explains how to get a free AI API key and configure it for the Outfit Planner application's AI Fashion Assistant feature using environment variables.

## Free AI API Options

### 1. OpenAI (Recommended)
**Best for:** General purpose AI with good fashion/style understanding
**Free Tier:** $5 free credit on signup (expires after 90 days)
**Rate Limit:** 3500 requests per minute (RPM)
**Model:** GPT-3.5-turbo (free), GPT-4o (with credits)

### 2. Google Gemini
**Best for:** Creative fashion advice and image analysis
**Free Tier:** 15 RPM (requests per minute) on free tier
**Model:** Gemini 1.5 Flash, Gemini Pro
**Requires:** Google Cloud account

### 3. Anthropic Claude
**Best for:** Detailed fashion analysis and wardrobe suggestions
**Free Tier:** 40 RPM on free tier
**Model:** Claude 3 Sonnet, Claude Haiku

### 4. Hugging Face
**Best for:** Custom fashion models and open-source options
**Free Tier:** 1000 requests per day
**Models:** 1000+ open source models available

## Quick Setup (Environment Variable Approach)

### Step 1: Get Your API Key
1. Visit [OpenAI Platform](https://platform.openai.com/)
2. Create an account or sign in
3. Navigate to "API Keys" in the left menu
4. Click "Create new secret key"
5. Give it a descriptive name (e.g., "Outfit-Planner-Dev")
6. Copy the key (starts with `sk-`)

### Step 2: Set Environment Variable (Recommended Method)

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY="sk-your-actual-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=sk-your-actual-api-key-here
```

**macOS/Linux:**
```bash
export OPENAI_API_KEY="sk-your-actual-api-key-here"
```

### Step 3: Alternative Environment Variable Names

The system checks for the following environment variables in order:
1. `OPENAI_API_KEY` (recommended for OpenAI)
2. `AI_API_KEY` (generic fallback)

### Step 4: Optional Configuration in appsettings.json

If you prefer not to use environment variables, you can still configure in `src/OutfitPlanner.Api/appsettings.json`:

```json
"AI": {
  "ApiKey": "sk-your-actual-api-key-here",  // Fallback if no env var
  "Endpoint": "https://api.openai.com/v1/chat/completions",
  "ModelName": "gpt-3.5-turbo",
  "MaxTokens": 1024,
  "Temperature": 0.7,
  "MaxHistoryMessages": 10,
  "CacheMinutes": 30
}
```

## Alternative Provider Setup

### Google Gemini Setup

1. Get API Key from [Google AI Studio](https://aistudio.google.com/)
2. Set environment variable:
```powershell
$env:AI_API_KEY="your-gemini-api-key"
```
3. Update appsettings.json:
```json
"AI": {
  "ApiKey": "your-gemini-api-key",
  "Endpoint": "https://generativelanguage.googleapis.com/v1beta/models",
  "ModelName": "gemini-1.5-flash",
  "MaxTokens": 1024,
  "Temperature": 0.7,
  "MaxHistoryMessages": 10,
  "CacheMinutes": 30
}
```

### Anthropic Claude Setup

1. Get API Key from [Anthropic Console](https://console.anthropic.com/)
2. Set environment variable:
```powershell
$env:AI_API_KEY="your-claude-api-key"
```
3. Update appsettings.json:
```json
"AI": {
  "ApiKey": "your-claude-api-key",
  "Endpoint": "https://api.anthropic.com/v1/messages",
  "ModelName": "claude-3-sonnet-20240229",
  "MaxTokens": 1024,
  "Temperature": 0.7,
  "MaxHistoryMessages": 10,
  "CacheMinutes": 30
}
```

## Testing Your Setup

After setting up the API key:

1. Start the application
2. Navigate to the AI Assistant page (`/ai-assistant`)
3. Try sending a message like "What should I wear for a date night?"
4. You should see a response from the AI

## Environment Variable Persistence

### For Windows (Permanent)
Add to your PowerShell profile:
```powershell
# Add to $PROFILE
$env:OPENAI_API_KEY="sk-your-actual-api-key-here"
```

Or use Command Prompt and set in System Properties > Environment Variables.

### For macOS/Linux (Permanent)
Add to your shell profile (~/.bashrc, ~/.zshrc, ~/.profile):
```bash
export OPENAI_API_KEY="sk-your-actual-api-key-here"
```

## Troubleshooting

### Common Issues

**"AI API key not found" Error:**
- Verify your environment variable is set correctly
- Check variable name spelling (case-sensitive)
- Ensure no extra spaces in the value
- Try restarting your terminal/IDE

**"Invalid API Key" Error:**
- Verify your API key is correctly copied (no extra spaces)
- Check that you haven't exceeded your free tier limits
- Ensure your account is active and has billing setup

**"Rate Limit Exceeded" Error:**
- You've reached your free tier limits
- Consider upgrading to a paid plan or switching providers
- Add delays between requests in your application

**Connection Issues:**
- Check your internet connection
- Verify the endpoint URL is correct
- Ensure no firewall is blocking the API calls

### Monitoring Usage

- **OpenAI:** Check usage at [OpenAI Usage Dashboard](https://platform.openai.com/usage)
- **Google Gemini:** Monitor in [Google Cloud Console](https://console.cloud.google.com/)
- **Anthropic:** View usage in [Anthropic Console](https://console.anthropic.com/)

## Cost Considerations

- **Free tiers are great for development and testing**
- **Production use requires paid plans**
- **Monitor usage regularly to avoid unexpected charges**
- **Consider implementing usage limits in your application**

## Security Best Practices

1. **Use environment variables instead of hardcoding keys**
2. **Never commit API keys to version control**
3. **Use environment-specific variables for different environments**
4. **Rotate keys periodically**
5. **Monitor for unusual usage patterns**

## Environment Variable Examples

### Development (.env file)
Create a `.env` file in your project root:
```
OPENAI_API_KEY=sk-dev-key-here
```

### Production (Server/Container)
Set in your deployment environment:
```bash
docker run -e OPENAI_API_KEY=sk-prod-key-here your-app
```

### CI/CD Pipeline
Add to your pipeline configuration:
```yaml
environment:
  OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
```

## Next Steps

Once your AI is configured, you can:
- Test the AI Assistant feature
- Customize prompts for fashion-specific advice
- Integrate with other AI services for image analysis
- Implement advanced features like outfit recommendations