const statusEl = document.getElementById("status");
const exportBtn = document.getElementById("export");

chrome.runtime.sendMessage({ type: "status" }, (response) => {
  if (!response) {
    statusEl.textContent = "Extension worker is not ready.";
    return;
  }
  const app = response.appOnline ? "desktop app connected" : "desktop app offline";
  statusEl.textContent = `${response.count} recorded listen(s), ${app}.`;
});

exportBtn.addEventListener("click", () => {
  exportBtn.disabled = true;
  chrome.runtime.sendMessage({ type: "export" }, () => {
    exportBtn.disabled = false;
  });
});
