mergeInto(LibraryManager.library, {
  YG_Init: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);

    function sendToUnity(payload) {
      var inst = window.unityInstance || window.gameInstance;
      if (inst && typeof inst.SendMessage === "function") {
        inst.SendMessage(objectName, methodName, payload);
      } else {
        console.log("[YG_Init] SendMessage before Unity instance: " + payload);
      }
    }

    function readPlayer(ysdk, kind) {
      var lang = window.__yandexLang || "ru";
      try {
        if (ysdk && ysdk.environment && ysdk.environment.i18n && ysdk.environment.i18n.lang) {
          lang = ysdk.environment.i18n.lang;
          window.__yandexLang = lang;
        }
      } catch (e) {
        console.warn("[YG_Init] Ошибка чтения i18n.lang: ", e);
      }

      if (!ysdk || !ysdk.getPlayer) {
        sendToUnity(kind + "|Гость||0|" + lang);
        return;
      }

      ysdk.getPlayer({ signed: false }).then(function (player) {
        var name = "";
        var id = "";
        var auth = 0;
        try { name = player.getName() || ""; } catch (e) { name = ""; }
        try { id = player.getUniqueID() || ""; } catch (e) { id = ""; }
        try { auth = player.isAuthorized && player.isAuthorized() ? 1 : 0; } catch (e) { auth = 0; }
        name = String(name).replace(/\|/g, " ");
        id = String(id).replace(/\|/g, "");
        sendToUnity(kind + "|" + name + "|" + id + "|" + auth + "|" + lang);
      }).catch(function () {
        sendToUnity(kind + "|Гость||0|" + lang);
      });
    }

    if (window.__ysdk) {
      readPlayer(window.__ysdk, "ready");
      return;
    }

    if (typeof YaGames === "undefined") {
      sendToUnity("error|SDK Яндекс Игр не найден. Игра запущена вне площадки.");
      return;
    }

    YaGames.init().then(function (ysdk) {
      window.__ysdk = ysdk;
      window.ysdk = ysdk;
      readPlayer(ysdk, "ready");
    }).catch(function (err) {
      var message = err && err.message ? err.message : "Не удалось инициализировать SDK";
      sendToUnity("error|" + String(message).replace(/\|/g, " "));
    });
  },

  YG_Authorize: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);

    function sendToUnity(payload) {
      var inst = window.unityInstance || window.gameInstance;
      if (inst && typeof inst.SendMessage === "function") {
        inst.SendMessage(objectName, methodName, payload);
      }
    }

    var ysdk = window.__ysdk || window.ysdk;
    if (!ysdk || !ysdk.auth || !ysdk.auth.openAuthDialog) {
      sendToUnity("error|Окно авторизации недоступно");
      return;
    }

    ysdk.auth.openAuthDialog().then(function () {
      ysdk.getPlayer({ signed: false }).then(function (player) {
        var name = "";
        var id = "";
        var auth = 0;
        try { name = player.getName() || ""; } catch (e) { name = ""; }
        try { id = player.getUniqueID() || ""; } catch (e) { id = ""; }
        try { auth = player.isAuthorized && player.isAuthorized() ? 1 : 0; } catch (e) { auth = 0; }
        name = String(name).replace(/\|/g, " ");
        id = String(id).replace(/\|/g, "");
        sendToUnity("auth|" + name + "|" + id + "|" + auth);
      }).catch(function () {
        sendToUnity("auth|Гость||0");
      });
    }).catch(function () {
      sendToUnity("cancel|Авторизация отменена");
    });
  },

  YG_GameReady: function () {
    var ysdk = window.__ysdk || window.ysdk;
    if (ysdk && ysdk.features && ysdk.features.LoadingAPI && ysdk.features.LoadingAPI.ready) {
      ysdk.features.LoadingAPI.ready();
    }
  },

  YG_ShowRewarded: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);

    function sendToUnity(payload) {
      var inst = window.unityInstance || window.gameInstance;
      if (inst && typeof inst.SendMessage === "function") {
        inst.SendMessage(objectName, methodName, payload);
      }
    }

    var ysdk = window.__ysdk || window.ysdk;
    if (!ysdk || !ysdk.adv || !ysdk.adv.showRewardedVideo) {
      sendToUnity("rewarded|error");
      return;
    }

    var granted = false;
    ysdk.adv.showRewardedVideo({
      callbacks: {
        onRewarded: function () { granted = true; },
        onClose: function () { sendToUnity(granted ? "rewarded|1" : "rewarded|0"); },
        onError: function () { sendToUnity("rewarded|error"); }
      }
    });
  },

  YG_ShowInterstitial: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);

    function sendToUnity(payload) {
      var inst = window.unityInstance || window.gameInstance;
      if (inst && typeof inst.SendMessage === "function") {
        inst.SendMessage(objectName, methodName, payload);
      }
    }

    var ysdk = window.__ysdk || window.ysdk;
    if (!ysdk || !ysdk.adv || !ysdk.adv.showFullscreenAdv) {
      sendToUnity("fullscreen|done");
      return;
    }

    ysdk.adv.showFullscreenAdv({
      callbacks: {
        onClose: function () { sendToUnity("fullscreen|done"); },
        onError: function () { sendToUnity("fullscreen|done"); }
      }
    });
  }
});
