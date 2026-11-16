mergeInto(LibraryManager.library, {
  $EmbeddingPage: {
    leaveCooldownMs: 1000,
    lastLeaveTime: 0,

    leave: function () {
      if (window.self === window.top) return;

      var now = Date.now();
      if (now - EmbeddingPage.lastLeaveTime < EmbeddingPage.leaveCooldownMs) return;

      EmbeddingPage.lastLeaveTime = now;
      window.history.back();
    },
  },

  LeaveEmbeddingPage__deps: ["$EmbeddingPage"],
  LeaveEmbeddingPage: function () {
    EmbeddingPage.leave();
  },

  LeaveEmbeddingPageOnPointerUnlock__deps: ["$EmbeddingPage"],
  LeaveEmbeddingPageOnPointerUnlock: function () {
    var isUnlockRequested = false;
    var exitPointerLock = document.exitPointerLock.bind(document);

    document.exitPointerLock = function () {
      if (document.pointerLockElement) isUnlockRequested = true;
      exitPointerLock();
    };

    document.addEventListener("pointerlockchange", function () {
      if (document.pointerLockElement) {
        isUnlockRequested = false;
        return;
      }

      var isUnlockedByPlayer = !isUnlockRequested && document.hasFocus();
      isUnlockRequested = false;
      if (isUnlockedByPlayer) EmbeddingPage.leave();
    });
  },
});
