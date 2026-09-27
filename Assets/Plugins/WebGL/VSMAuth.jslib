mergeInto(LibraryManager.library, {
  VSM_RequestWebsiteToken: function(objectName) {
    var target = UTF8ToString(objectName);
    // Host implements this function using its existing authenticated state.
    // Never assume a cookie name, expose HttpOnly cookies, or put tokens in URLs.
    if (typeof window.vsmGetAccessToken !== 'function') {
      console.warn('VSM: configure window.vsmGetAccessToken or send SetAccessToken after Unity is ready.');
      return;
    }
    Promise.resolve().then(function() {
      return window.vsmGetAccessToken();
    }).then(function(token) {
      if (typeof token === 'string' && token.trim())
        SendMessage(target, 'SetAccessToken', token.trim());
    }).catch(function() { console.warn('VSM: website authentication unavailable.'); });
  }
});
