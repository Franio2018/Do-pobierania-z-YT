const APP_URL = "http://127.0.0.1:17823";

chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  if (message?.type === "listened") {
    saveListen(message)
      .then(() => postToApp(message))
      .then(() => sendResponse({ ok: true }))
      .catch(() => sendResponse({ ok: false }));
    return true;
  }

  if (message?.type === "export") {
    exportListens()
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
    await fetch(APP_URL + "/listened", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        videoId: item.videoId,
        title: item.title,
        url: item.url
      })
    });
  } catch {
    // Desktop app is optional while YouTube is playing.
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

async function exportListens() {
  const data = await chrome.storage.local.get({ listens: [] });
  const payload = {
    version: 1,
    exportedAt: new Date().toISOString(),
    listens: Array.isArray(data.listens) ? data.listens : []
  };
  const json = JSON.stringify(payload, null, 2);
  const url = "data:application/json;charset=utf-8," + encodeURIComponent(json);
  await chrome.downloads.download({
    url,
    filename: "pending-listens.json",
    saveAs: true,
    conflictAction: "overwrite"
  });
}
