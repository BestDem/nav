mergeInto(LibraryManager.library, {
  $VSMSpeech: { active: null },
  VSM_StartDictation__deps: ['$VSMSpeech'],
  VSM_StartDictation: function(objectName) {
    var target = UTF8ToString(objectName);
    var Recognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!Recognition) { SendMessage(target, 'OnDictationError', 'unsupported'); return; }
    if (VSMSpeech.active) return;
    var recognition = new Recognition();
    VSMSpeech.active = recognition;
    recognition.lang = 'ru-RU';
    recognition.continuous = false;
    recognition.interimResults = false;
    recognition.onresult = function(event) {
      if (VSMSpeech.active !== recognition) return;
      var text = '';
      for (var i = event.resultIndex; i < event.results.length; i++) {
        if (event.results[i].isFinal) text += event.results[i][0].transcript + ' ';
      }
      if (text.trim()) SendMessage(target, 'OnDictationText', text.trim());
    };
    recognition.onerror = function(event) {
      if (VSMSpeech.active === recognition) SendMessage(target, 'OnDictationError', event.error);
    };
    recognition.onend = function() {
      if (VSMSpeech.active !== recognition) return;
      VSMSpeech.active = null;
      SendMessage(target, 'OnDictationEnd', '');
    };
    try { recognition.start(); }
    catch (error) {
      VSMSpeech.active = null;
      SendMessage(target, 'OnDictationError', 'start-failed');
    }
  },
  VSM_StopDictation__deps: ['$VSMSpeech'],
  VSM_StopDictation: function() {
    var recognition = VSMSpeech.active;
    VSMSpeech.active = null;
    if (recognition) recognition.abort();
  }
});
