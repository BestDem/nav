Unity ↔ VSM API

Scenario screen MVP (2026-09-26)
- Main/Canvas/DIalogue is discovered even when its old parent is inactive.
- Runtime wiring moves it to an independent overlay Canvas called
  VSM_SCENARIO_CHAT. Legacy children are hidden, not removed from the scene file.
- ScenarioChatView builds the reference layout: transparent overlay, right-hand
  scrollable history, blue passenger bubbles, white conductor bubbles, bottom
  composer with Send and microphone controls. Only API messages are displayed.
- The scenario starts hidden and starts its dialogue only on first opening.
- F opens/hides the scenario (except while typing into its input). Escape hides it without opening settings in the
  same frame. When the scenario is hidden, Escape opens normal settings.
- MenuInputGate gives each menu its own input lock. Player movement, jump and
  mouse look cannot run while a menu holds a lock. Closing settings does not
  unlock the player if the scenario is still open.
- «Завершить» calls conclude/assess. «Свернуть» keeps the session and pending
  requests alive. A completed scenario can be followed by another pending one.
- Pending conductor text appears immediately; errors preserve the input.
- Scene assets and API credentials are not rewritten by the layout builder.

Voice input:
- Microphone invokes the browser Web Speech API in WebGL, with ru-RU recognition.
- Supported browsers may use their own remote speech service; this is not VSM's
  AI endpoint. Browser compatibility and microphone permissions vary:
  https://developer.mozilla.org/en-US/docs/Web/API/SpeechRecognition
- A final transcript is inserted into the input for review, never auto-submitted.
- Closing the scenario cancels recognition and ignores late callbacks.
- Unsupported browsers and Unity Editor display an explanation; keyboard input
  remains available. No microphone recording occurs in Editor.
- Run `node Tests/voice-bridge.test.cjs` for browser-bridge contract checks.

Validation: Editor and WebGL C# branches compile with the project's Unity
references; speech bridge mock checks pass. Rendering an isolated Unity preview
was declined, so the layout and actual microphone still need an in-editor /
browser visual and functional check. Stop Play, wait for import, restart Play.

Implemented: authentication before session creation; scenario assignment on first
passenger interaction; start → respond → conclude → assess; runtime dialogue UI,
loading/error states, assessment, movement lock and duplicate-send protection.
PeopleDIalogue connects dynamically spawned passengers. Service classes do not
change backend requests yet. Bootstrap creates VSM_WEB_BRIDGE before the menu.
Existing scene service objects are discarded by singleton guards.

Website integration (same page as Unity canvas):
1. Define window.vsmGetAccessToken before createUnityInstance. It must return the
   current Bearer token (string or Promise<string>) from the site's auth state:

   window.vsmGetAccessToken = async () => auth.getAccessToken();

   auth.getAccessToken is an EXAMPLE: replace it with the site's actual accessor.
   The WebGL .jslib invokes it once when Unity's bridge starts.

2. Alternatively, after createUnityInstance resolves, pass the token explicitly:

   unityInstance.SendMessage("VSM_WEB_BRIDGE", "SetAccessToken", accessToken);

   Use the same call after a token refresh. Do not send another user's token into
   a running session: reload the game when switching accounts or logging out.

3. If the token really lives in a JavaScript-readable cookie, the website accessor
   can read that exact cookie. Its actual name must be supplied by the backend.
   HttpOnly cookies cannot be read by JavaScript or Unity. Keep HttpOnly enabled;
   in that case the backend needs cookie authentication or an authenticated token
   exchange endpoint. Neither is assumed by this Bearer client.

4. Prefer hosting the game on https://vsm-edu.ru. For another origin the backend
   must allow that specific origin, GET/POST/OPTIONS, Authorization and Content-Type
   in CORS. For iframe embedding, the accessor must exist in the frame containing
   Unity; no unrestricted postMessage listener is installed.

No passwords or tokens are persisted, embedded in builds, or put in URLs.
Authentication uses the website token; no Editor test-token fallback is provided.
Without a token, interaction explains that website authorization is pending.

Current contract inherited from the existing Unity integration:
POST /api/game/sessions
GET /api/game/sessions/{id}/scenarios
POST /api/game/sessions/{id}/scenarios/{code}/start
POST .../respond with {"answer":"..."}
POST .../conclude
POST .../assess
Response DTOs: VSMModels.cs. No admin endpoints are called.

Verification limitation (2026-09-26): vsm-edu.ru failed DNS resolution from the
working environment, including an unrestricted retry. The adjacent backend
repository confirms Bearer auth, but lacks the new game API. Therefore these
routes/DTOs still require verification against the deployed docs.json.

Live acceptance check:
- Log in on the website, load WebGL, verify one session creation and scenarios GET.
- Interact with a dialogue passenger, send answers, verify conclude/assess and UI.
- Test double-click, slow response, 401, lost connection, close/reopen.
- Reload game on account switch; verify assessment belongs to the logged-in user.
- Do not automatically retry POST after ambiguous network failure: first confirm
  whether the backend already applied it. Manual retry is available in the UI.
