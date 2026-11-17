mergeInto(LibraryManager.library, {
  YG_Init: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);

    function sendToUnity(payload) {
      function trySend() {
        var inst = window.unityInstance || window.gameInstance;
        if (inst && typeof inst.SendMessage === "function") {
          inst.SendMessage(objectName, methodName, payload);
          return true;
        }
        return false;
      }
      if (!trySend()) {
        var retries = 0;
        var interval = setInterval(function () {
          retries++;
          if (trySend() || retries > 100) {
            clearInterval(interval);
          }
        }, 50);
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

    var attempts = 0;
    function checkAndInit() {
      if (window.__ysdk) {
        readPlayer(window.__ysdk, "ready");
        return;
      }

      if (typeof YaGames !== "undefined") {
        YaGames.init().then(function (ysdk) {
          window.__ysdk = ysdk;
          window.ysdk = ysdk;
          readPlayer(ysdk, "ready");
        }).catch(function (err) {
          var message = err && err.message ? err.message : "Не удалось инициализировать SDK";
          sendToUnity("error|" + String(message).replace(/\|/g, " "));
        });
        return;
      }

      attempts++;
      if (attempts < 20) {
        setTimeout(checkAndInit, 100);
      } else {
        sendToUnity("error|SDK Яндекс Игр не найден. Игра запущена вне площадки.");
      }
    }

    checkAndInit();
  },

  YG_Authorize: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);

    function sendToUnity(payload) {
      function trySend() {
        var inst = window.unityInstance || window.gameInstance;
        if (inst && typeof inst.SendMessage === "function") {
          inst.SendMessage(objectName, methodName, payload);
          return true;
        }
        return false;
      }
      if (!trySend()) {
        var retries = 0;
        var interval = setInterval(function () {
          retries++;
          if (trySend() || retries > 100) {
            clearInterval(interval);
          }
        }, 50);
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
    try {
      ysdk.adv.showRewardedVideo({
        callbacks: {
          onOpen: function () {
            console.log("[YG_ShowRewarded] onOpen");
          },
          onRewarded: function () {
            console.log("[YG_ShowRewarded] onRewarded");
            granted = true;
          },
          onClose: function () {
            console.log("[YG_ShowRewarded] onClose, granted: " + granted);
            sendToUnity(granted ? "rewarded|1" : "rewarded|0");
          },
          onError: function (err) {
            console.warn("[YG_ShowRewarded] onError: ", err);
            sendToUnity("rewarded|error");
          }
        }
      });
    } catch (e) {
      console.warn("[YG_ShowRewarded] exception: ", e);
      sendToUnity("rewarded|error");
    }
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

    try {
      ysdk.adv.showFullscreenAdv({
        callbacks: {
          onOpen: function () {
            console.log("[YG_ShowInterstitial] onOpen");
          },
          onClose: function (wasShown) {
            console.log("[YG_ShowInterstitial] onClose, wasShown: " + wasShown);
            sendToUnity("fullscreen|done");
          },
          onError: function (err) {
            console.warn("[YG_ShowInterstitial] onError: ", err);
            sendToUnity("fullscreen|done");
          }
        }
      });
    } catch (e) {
      console.warn("[YG_ShowInterstitial] exception: ", e);
      sendToUnity("fullscreen|done");
    }
  }
});
