using System;

[Serializable] public class PlayerAnswer { public string answer; }

[Serializable] public class GameSessionResponse { public string status; public GameSession data; }

[Serializable] public class GameSession
{
    public string id;
    public string type;
    public string status;
    public GameProgress progress;
}

[Serializable] public class GameProgress { public int completed; public int required; }

[Serializable] public class GameScenarioListResponse
{
    public string status;
    public GameScenarioListData data;
}

[Serializable] public class GameScenarioListData
{
    public GameSession session;
    public GameScenario[] scenarios;
}

[Serializable] public class GameScenario
{
    public int positionInSession;
    public string code;
    public string title;
    public string urgency;
    public string status;
    public int score;
    public int safetyScore;
    public int loyaltyScore;
}

[Serializable] public class GameDialogueResponse
{
    public string status;
    public GameDialogueData data;
}

[Serializable] public class GameDialogueData
{
    public GameScenario scenario;
    public DialogueResult result;
}

[Serializable] public class DialogueResult
{
    public string passengerReply;
    public bool shouldContinue;
    public string situationState;
}

[Serializable] public class GameConclusionResponse
{
    public string status;
    public GameConclusionData data;
}

[Serializable] public class GameConclusionData
{
    public GameScenario scenario;
    public ConclusionResult result;
}

[Serializable] public class ConclusionResult
{
    public string passengerReply;
    public bool completed;
    public string outcome;
}

[Serializable] public class GameAssessmentResponse
{
    public string status;
    public GameAssessmentData data;
}

[Serializable] public class GameAssessmentData
{
    public GameScenario scenario;
    public AssessmentResult result;
}

[Serializable] public class AssessmentResult
{
    public int score;
    public int safetyScore;
    public int loyaltyScore;
    public string outcome;
    public AssessmentCriterion[] criteria;
    public string[] strengths;
    public string[] mistakes;
    public string summary;
}

[Serializable] public class AssessmentCriterion
{
    public string criterion;
    public int score;
    public string comment;
}
