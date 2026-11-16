function YG_Send(objectName, methodName, payload) {
  var inst = window.unityInstance || window.gameInstance;
  if (!inst) {
    console.log("Yandex SDK reply before Unity instance: " + payload);
    return;
  }
  inst.SendMessage(objectName, methodName, payload);
}

function YG_ReadPlayer(ysdk, kind, objectName, methodName) {
  ysdk.getPlayer({ signed: false }).then(function (player) {
    var name = "";
    var id = "";
    var auth = 0;
    try { name = player.getName() || ""; } catch (e) { name = ""; }
    try { id = player.getUniqueID() || ""; } catch (e) { id = ""; }
    try { auth = player.isAuthorized && player.isAuthorized() ? 1 : 0; } catch (e) { auth = 0; }
    name = String(name).replace(/\|/g, " ");
    id = String(id).replace(/\|/g, "");
    YG_Send(objectName, methodName, kind + "|" + name + "|" + id + "|" + auth);
  }).catch(function () {
    YG_Send(objectName, methodName, kind + "|Гость||0");
  });
}

mergeInto(LibraryManager.library, {
  YG_Init: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);
    if (typeof YaGames === "undefined") {
      YG_Send(objectName, methodName, "error|SDK Яндекс Игр не найден. Игра запущена вне площадки.");
      return;
    }
    YaGames.init().then(function (ysdk) {
      window.__ysdk = ysdk;
      YG_ReadPlayer(ysdk, "ready", objectName, methodName);
    }).catch(function (err) {
      var message = err && err.message ? err.message : "Не удалось инициализировать SDK";
      YG_Send(objectName, methodName, "error|" + String(message).replace(/\|/g, " "));
    });
  },

  YG_Authorize: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);
    var ysdk = window.__ysdk;
    if (!ysdk || !ysdk.auth || !ysdk.auth.openAuthDialog) {
      YG_Send(objectName, methodName, "error|Окно авторизации недоступно");
      return;
    }
    ysdk.auth.openAuthDialog().then(function () {
      YG_ReadPlayer(ysdk, "auth", objectName, methodName);
    }).catch(function () {
      YG_Send(objectName, methodName, "cancel|Авторизация отменена");
    });
  },

  YG_GameReady: function () {
    var ysdk = window.__ysdk;
    if (ysdk && ysdk.features && ysdk.features.LoadingAPI && ysdk.features.LoadingAPI.ready)
      ysdk.features.LoadingAPI.ready();
  },

  YG_ShowRewarded: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);
    var ysdk = window.__ysdk;
    if (!ysdk || !ysdk.adv || !ysdk.adv.showRewardedVideo) {
      YG_Send(objectName, methodName, "rewarded|error");
      return;
    }
    var granted = false;
    ysdk.adv.showRewardedVideo({
      callbacks: {
        onRewarded: function () { granted = true; },
        onClose: function () { YG_Send(objectName, methodName, granted ? "rewarded|1" : "rewarded|0"); },
        onError: function () { YG_Send(objectName, methodName, "rewarded|error"); }
      }
    });
  },

  YG_ShowInterstitial: function (objectNamePtr, methodNamePtr) {
    var objectName = UTF8ToString(objectNamePtr);
    var methodName = UTF8ToString(methodNamePtr);
    var ysdk = window.__ysdk;
    if (!ysdk || !ysdk.adv || !ysdk.adv.showFullscreenAdv) {
      YG_Send(objectName, methodName, "fullscreen|done");
      return;
    }
    ysdk.adv.showFullscreenAdv({
      callbacks: {
        onClose: function () { YG_Send(objectName, methodName, "fullscreen|done"); },
        onError: function () { YG_Send(objectName, methodName, "fullscreen|done"); }
      }
    });
  }
});
