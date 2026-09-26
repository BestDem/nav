const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
function setup(supported = true) {
  const calls = [];
  class Recognition {
    start() { this.started = true; }
    abort() { this.aborted = true; this.onend(); }
  }
  const context = {
    LibraryManager: { library: {} },
    mergeInto: Object.assign,
    UTF8ToString: x => x,
    SendMessage: (...args) => calls.push(args),
    window: supported ? { SpeechRecognition: Recognition } : {}
  };
  vm.createContext(context);
  vm.runInContext(fs.readFileSync('Assets/Plugins/WebGL/VSMSpeech.jslib', 'utf8'), context);
  const lib = context.LibraryManager.library;
  context.VSMSpeech = lib.$VSMSpeech;
  return { lib, calls, context };
}
{
  const {lib,calls} = setup(false);
  lib.VSM_StartDictation('Chat');
  assert.deepEqual(calls, [['Chat', 'OnDictationError', 'unsupported']]);
}
{
  const {lib,calls,context} = setup();
  lib.VSM_StartDictation('Chat');
  const speech = context.VSMSpeech.active;
  assert.equal(speech.lang, 'ru-RU');
  assert.equal(speech.started, true);
  lib.VSM_StartDictation('Chat');
  assert.equal(context.VSMSpeech.active, speech);
  const result = [{transcript: 'Добрый день'}]; result.isFinal = true;
  speech.onresult({resultIndex:0, results:[result]});
  assert.deepEqual(calls[0], ['Chat', 'OnDictationText', 'Добрый день']);
  speech.onend();
  assert.equal(context.VSMSpeech.active, null);
  assert.equal(calls[1][1], 'OnDictationEnd');
}
{
  const {lib,calls,context} = setup();
  lib.VSM_StartDictation('Chat');
  const speech = context.VSMSpeech.active;
  lib.VSM_StopDictation();
  const result = [{transcript:'Late text'}]; result.isFinal = true;
  speech.onresult({resultIndex:0, results:[result]});
  assert.equal(speech.aborted, true);
  assert.equal(calls.length,0, 'Closed screen must ignore late callbacks');
}
{
  const {lib,calls,context} = setup();
  lib.VSM_StartDictation('Chat');
  context.VSMSpeech.active.onerror({error:'not-allowed'});
  assert.deepEqual(calls[0], ['Chat','OnDictationError','not-allowed']);
}
console.log('Speech bridge: 4 checks passed');
