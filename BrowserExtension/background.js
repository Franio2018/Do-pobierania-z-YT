const APP_URL = "http://127.0.0.1:17823";

let backupTimer = 0;
let flushTimer = 0;

chrome.alarms.create("syncApp", { periodInMinutes: 1 });

chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  if (message?.type === "listened") {
    saveListen(message)
      .then(() => postToApp(message))
      .then((ok) => {
        if (!ok)
          scheduleOfflineBackup();
        sendResponse({ ok: true });
      })
      .catch(() => sendResponse({ ok: false }));
    return true;
  }

  if (message?.type === "export") {
    exportListens(true)
      .then(() => sendResponse({ ok: true }))
      .catch(() => sendResponse({ ok: false }));
    return true;
  }

  if (message?.type === "sync") {
    flushToApp()
      .then(() => sendResponse({ ok: true }))
      .catch(() => sendResponse({ ok: false }));
    return true;
  }

  if (message?.type === "status") {
    Promise.all([chrome.storage.local.get({ listens: [] }), pingApp()]).then(([data, appOnline]) => {
      sendResponse({
        count: Array.isArray(data.listens) ? data.listens.length : 0,
        appOnline
      });
    });
    return true;
  }

  return false;
});

chrome.alarms.onAlarm.addListener((alarm) => {
  if (alarm.name === "syncApp")
    flushToApp();
});

async function saveListen(item) {
  const data = await chrome.storage.local.get({ listens: [] });
  const listens = Array.isArray(data.listens) ? data.listens : [];
  const existing = listens.find((entry) => entry.videoId === item.videoId);
  const listenedAt = new Date().toISOString();
  if (existing) {
    existing.listenCount = (existing.listenCount || 1) + 1;
    existing.title = item.title || existing.title;
    existing.url = item.url || existing.url;
    existing.listenedAt = listenedAt;
  } else {
    listens.push({
      videoId: item.videoId,
      title: item.title || item.videoId,
      url: item.url,
      listenCount: 1,
      listenedAt
    });
  }
  await chrome.storage.local.set({ listens });
}

async function postToApp(item) {
  try {
    const response = await fetch(APP_URL + "/listened", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        videoId: item.videoId,
        title: item.title,
        url: item.url
      })
    });
    return response.ok;
  } catch {
    return false;
  }
}

async function pingApp() {
  try {
    const response = await fetch(APP_URL + "/health");
    return response.ok;
  } catch {
    return false;
  }
}

async function flushToApp() {
  if (flushTimer)
    return;
  flushTimer = 1;
  try {
    if (!(await pingApp())) {
      await exportListens(false);
      return;
    }

    const data = await chrome.storage.local.get({ listens: [] });
    const listens = Array.isArray(data.listens) ? data.listens : [];
    const response = await fetch(APP_URL + "/listens", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        version: 1,
        exportedAt: new Date().toISOString(),
        listens
      })
    });
    if (!response.ok)
      scheduleOfflineBackup();
  } catch {
    scheduleOfflineBackup();
  } finally {
    flushTimer = 0;
  }
}

function scheduleOfflineBackup() {
  clearTimeout(backupTimer);
  backupTimer = setTimeout(() => {
    exportListens(false).catch(() => {});
  }, 2000);
}

async function exportListens(saveAs) {
  const data = await chrome.storage.local.get({ listens: [] });
  const payload = {
    version: 1,
    exportedAt: new Date().toISOString(),
    listens: Array.isArray(data.listens) ? data.listens : []
  };
  if (!saveAs && payload.listens.length === 0)
    return;

  const json = JSON.stringify(payload, null, 2);
  const fingerprint = payload.listens.map((item) => `${item.videoId}:${item.listenCount}`).join("|");
  if (!saveAs) {
    const stored = await chrome.storage.local.get({ lastBackupFingerprint: "" });
    if (stored.lastBackupFingerprint === fingerprint)
      return;
  }

  const url = "data:application/json;charset=utf-8," + encodeURIComponent(json);

  if (!saveAs)
    await removePreviousPendingDownloads();

  await chrome.downloads.download({
    url,
    filename: "pending-listens.json",
    saveAs: !!saveAs,
    conflictAction: "overwrite"
  });
  if (!saveAs)
    await chrome.storage.local.set({ lastBackupFingerprint: fingerprint });
}

async function removePreviousPendingDownloads() {
  const found = await chrome.downloads.search({
    filenameRegex: "(^|\\\\|/)pending-listens( \\([0-9]+\\))?\\.json$"
  });
  for (const item of found) {
    try {
      await chrome.downloads.removeFile(item.id);
    } catch {
      // File may already be gone.
    }
    try {
      await chrome.downloads.erase({ id: item.id });
    } catch {
      // Ignore history cleanup failures.
    }
  }
}
