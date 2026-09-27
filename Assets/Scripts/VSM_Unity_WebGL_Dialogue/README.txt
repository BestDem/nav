Unity ↔ VSM API

Scenario screen MVP (2026-09-26)
- Main/Canvas/DIalogue is discovered even when its old parent is inactive.
- Runtime wiring moves it to an independent overlay Canvas called
  VSM_SCENARIO_CHAT. Legacy children are hidden, not removed from the scene file.
- ScenarioChatView builds the reference layout: transparent overlay, right-hand
  scrollable history, blue passenger bubbles, white conductor bubbles, bottom
  composer with Send and microphone controls. Only API messages are displayed.
- The scenario starts hidden and starts its dialogue only on first opening.
- Aim at a scenario passenger within 3 metres and press F to open their dialogue.
  Walls block interaction. Ordinary/assessed passengers have no F prompt.
  Escape hides the chat without opening settings in the same frame.
- MenuInputGate gives each menu its own input lock. Player movement, jump and
  mouse look cannot run while a menu holds a lock. Closing settings does not
  unlock the player if the scenario is still open.
- «Завершить» calls conclude/assess. «Свернуть» keeps the session and pending
  requests alive. To start another scenario, approach its assigned passenger.
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

Passenger scenarios (2026-09-27):
- Contract: https://vsm-edu.ru/api/docs.json. Session creation/resumption returns
  the session id; GET /api/game/sessions/{id}/scenarios supplies ten unique codes.
- ControllerPeople assigns each code once, before interaction, with at most
  max(3, BaseTimer.MaxEvilPeople) active scenario passengers at once. Others are
  ordinary. Vacancies are refilled immediately; if everyone already had a scenario,
  replacements arrive at the next stop.
- Each passenger keeps their own history and scenario through hide/reopen.
  GET /scenarios/{code} restores started/completed attempts after scene reload;
  only pending attempts receive POST /start. History role/content fields were
  verified against the sibling backend GameAiController and ScenarioAttempt.
- POST /respond sends {answer}; conclude is allowed after a decision or expiry.
  The conclusion reply is shown. The marker disappears on confirmed conclusion,
  even if assess needs a retry. Assessment completion is counted once per code.
- Timers use decisionTimeLimitSeconds/decisionDeadlineAt/decisionReceivedAt.
  This is the server's FIRST-decision deadline, not a separate local chat timer.
  Before a deadline exists, or after a decision is accepted, the full ring is
  an active-scenario marker. Scenarios without a time limit also use this marker.
- The five seats are populated; at stops only ordinary or assessed passengers
  leave. Unfinished passengers and a passenger with an open results UI stay.
  Stops continue until all ten are assessed. The tenth confirmed assessment
  immediately stops play and opens the completion menu with a return-to-menu button.
- Returning to the menu releases unfinished assignments for restored passengers;
  selecting a new trip after ten assessments creates/resumes a new server session.
- No availableEvents are fabricated: world-action verification is not implemented
  by the text dialogue; such events must correspond to actions actually performed.
- Run python3 Tests/ScenarioFlow/run.py for deterministic production-logic tests
  with simulated Unity lifecycle/HTTP; no live scenario scores are changed.
  Full rendering, raycast targeting and live AI still need a Unity Play Mode check.

Service classes do not change backend requests. Bootstrap creates VSM_WEB_BRIDGE
before the menu; existing scene service objects are discarded by singleton guards.

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

No tokens are embedded in builds or put in URLs.
WebGL authentication uses the website token. Local test-token loading is disabled.
Without a token, interaction explains that website authorization is pending.

Verified API routes:
POST /api/game/sessions
GET /api/game/sessions/{id}/scenarios
POST /api/game/sessions/{id}/scenarios/{code}/start
GET /api/game/sessions/{id}/scenarios/{code}
POST .../respond with {"answer":"..."}
POST .../conclude
POST .../assess
Response DTOs: VSMModels.cs. No admin endpoints are called.

Live acceptance check:
- Log in on the website, load WebGL, verify one session creation and scenarios GET.
- Interact with a dialogue passenger, send answers, verify conclude/assess and UI.
- Test double-click, slow response, 401, lost connection, close/reopen.
- Reload game on account switch; verify assessment belongs to the logged-in user.
- Do not automatically retry POST after ambiguous network failure: first confirm
  whether the backend already applied it. Manual retry is available in the UI.

Touch controls and completion screen (source changes, no new player build):
- PlayerController installs MobileGameControls on entering gameplay on every platform.
- «Показать интерфейс» toggles movement, jump, interaction and settings.
  The toggle remains tappable on touch devices; M toggles it on desktop outside menus.
- Hold direction buttons to walk; swipe anywhere outside UI to look; «Поговорить» uses the
  same 3 m, nearest-collider interaction as F. UI input is cleared on hide, menu
  entry and focus loss. Keyboard movement still works.
- On desktop, moving the mouse turns the camera without holding a button.
  The cursor is hidden and locked during desktop gameplay, and released for menus/chat.
  Touch devices keep pointer locking disabled so screen buttons remain tappable.
- Touch look has no visible panel and works even with the controls hidden.
- Completion uses an overlay Canvas at sorting order 1000. The tenth assessment
  closes the scenario overlay and immediately shows the final screen, including
  the last assessment in a scrollable panel and return-to-menu button.
- ScenarioFlow now includes regression checks for completion with an open chat,
  single presentation, input/time restoration and preservation of assessment text.
- Device touch and visual layout still require Play Mode/device acceptance checks.
