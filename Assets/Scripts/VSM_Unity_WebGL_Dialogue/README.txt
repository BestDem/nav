VSM Unity WebGL integration

1. Create one persistent object VSM_API:
   - VSMApiClient
   - VSMWebAuthBridge
   - VSMGameSession

2. The object with VSMWebAuthBridge MUST be named:
   VSM_WEB_BRIDGE

3. Website authorizes the user. Unity does not ask for login/password.

4. Website passes the existing access token:
   unityInstance.SendMessage(
       "VSM_WEB_BRIDGE",
       "SetAccessToken",
       accessToken
   );

5. After token:
   POST /api/game/sessions
   GET /api/game/sessions/{id}/scenarios

6. The normal player endpoint returns exactly 10 scenarios.
   VSMGameSession.TakeRandomScenario() selects one and removes it locally.

7. Put PassengerDialogue on each passenger.
   Assign a GameScenario with SetScenario(...).

8. On player interaction:
   passenger.OpenDialogue();

9. Dialogue UI calls:
   passenger.SendPlayerAnswer(inputField.text);

10. API response updates:
    CurrentText
    CurrentEmotion
    ShouldContinue

11. When shouldContinue == false:
    conclude -> assess

12. PassengerDialogue.OnDialogueFinished fires.
    DialogueResultsUI subscribes and opens the result Canvas.

13. The assess endpoint returns:
    score
    safetyScore
    loyaltyScore
    outcome
    criteria
    strengths
    mistakes
    summary

The assessment is stored server-side. /api/users/me exposes accumulated
profile/statistics for the user's account.

IMPORTANT:
Do not use /api/admin/scenarios from the normal player client.
That endpoint requires ROLE_ADMIN. The player's 10 assigned scenarios
are the correct source of scenario codes.
