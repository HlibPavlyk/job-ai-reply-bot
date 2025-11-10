using DjinniAIReplyBot.Application.Abstractions.ExternalServices;
using DjinniAIReplyBot.Application.Abstractions.Services;
using DjinniAIReplyBot.Domain.Enums;
using OpenAI.Chat;

namespace DjinniAIReplyBot.Application.Services;

public class ChatGptService : IChatGptService
{
    private readonly IChatGptClient  _chatGptClient;

    public ChatGptService(IChatGptClient chatGptClient)
    {
        _chatGptClient = chatGptClient;
    }
    
    public async Task<string?> ParseResumeAsync(long chatId, string resumeText)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(@"You are an HR assistant specialized in extracting and formatting resume information for job applications.

INSTRUCTIONS:
- Extract and present information in a clear, structured format suitable for Telegram
- Prioritize SKILLS section with detailed enumeration
- Keep Education, Experience, and Projects sections brief (2-3 lines max each)
- Only include sections that are present in the resume - do not fabricate information
- Use clear section headers with emojis for readability
- Keep total response under 4000 characters (Telegram limit)

OUTPUT FORMAT:
📋 *[Candidate Name]*

💼 *Skills*
• [List all technical and soft skills in detail]

🎓 *Education* (if present)
[Brief summary]

💻 *Experience* (if present)
[Brief summary with years of experience]

🚀 *Projects* (if present)
[Brief summary of key projects]

If the resume is poorly formatted or incomplete, extract what you can and note any missing critical information."),
            new UserChatMessage($"Here is the resume to process:\n\n{resumeText}")
        };

        return await _chatGptClient.GenerateNewChatResponseAsync(chatId, messages);
    }

    public async Task<string?> GenerateCoverLetterAsync(long chatId, string jobDescription, string resumeText, ReplyLanguage replyLanguage, string? additionalNotes = null)
    {
        var languageInstruction = replyLanguage == ReplyLanguage.En
            ? "Write in English"
            : "Write in Ukrainian language (Пишіть українською мовою)";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage($@"You are a professional career assistant who creates personalized cover letters. {languageInstruction}.

GOAL:
Generate a professional cover letter that demonstrates strong alignment between the candidate's skills and the job requirements.

REQUIRED TEMPLATE STRUCTURE:
Use this exact format and structure:

Dear [Hiring Manager/Recipient's Name],

I am writing to express my interest in the [Job Position] at [Company Name]. With a background as a [Current/Previous Job Title] and a strong proficiency in [MATCHED Skills from Resume that appear in Job Requirements], I am confident in my ability to deliver technical solutions that meet your team's objectives.

In my previous role at [Previous Company/Experience], I successfully [specific achievement using MATCHED skills/technologies]. My technical expertise in [list 2-3 MATCHED skills that align with job requirements] has equipped me with the ability to analyze complex problems and implement efficient, scalable solutions.

I am particularly interested in the opportunity at [Company Name] because [reason based on job description]. I am eager to apply my technical skills in [MATCHED technology/field mentioned in BOTH resume and job description] and contribute to advancing your company's projects.

I look forward to discussing how my technical background and experience can be an asset to your team. Please feel free to contact me to arrange a meeting.

Best regards,
[Candidate Name from Resume]

CRITICAL RULES - SKILLS MATCHING:
- MANDATORY: Identify skills that exist in BOTH the resume AND job description
- MANDATORY: Include at least 3-5 matched skills explicitly in the cover letter
- In the first paragraph [Key Skills], list the most important matched skills
- In the second paragraph, mention specific matched technologies/skills in the achievement
- In the third paragraph, reference matched skills/technologies again
- DO NOT mention skills that are in the job description but NOT in the resume
- DO NOT make up experience with technologies not mentioned in the resume

GENERAL RULES:
- Extract [Candidate Name], [Current Job Title], [Previous Company], [achievements] from the provided resume
- Extract [Job Position], [Company Name] from the job description
- Keep total length to 250-350 words
- Make the achievement/responsibility specific and quantifiable if possible
- Professional and confident tone, but not arrogant
- If resume lacks information, use general but appropriate placeholder text"),
            new UserChatMessage($"**Job Description:**\n{jobDescription}"),
            new UserChatMessage($"**My Resume:**\n{resumeText}")
        };

        if (!string.IsNullOrEmpty(additionalNotes))
        {
            messages.Add(new UserChatMessage($"**Additional Instructions:**\n{additionalNotes}"));
        }

        return await _chatGptClient.GenerateNewChatResponseAsync(chatId, messages);
    }

    public Task<string?> RegenerateCoverLetterAsync(long chatId, string userRevision)
    {
        if(string.IsNullOrEmpty(userRevision))
        {
            throw new ArgumentException("User revision cannot be null or empty.", nameof(userRevision));
        }

        var revisionPrompt = $@"**Revision Request:**
{userRevision}

Please revise the cover letter according to the feedback above while:
- Maintaining the same template structure and format
- Keeping the professional and confident tone
- Staying within 250-350 words
- Preserving the alignment between resume skills and job requirements
- Only changing the parts mentioned in the feedback";

        return _chatGptClient.ContinueChatResponseAsync(chatId, revisionPrompt);
    }
}