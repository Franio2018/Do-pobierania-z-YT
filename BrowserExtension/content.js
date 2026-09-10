(() => {
  const THRESHOLD = 0.8;
  const reported = new Set();
  let currentVideoId = null;
  let attachedVideo = null;

  function videoIdFromUrl(url) {
    try {
      const parsed = new URL(url);
      if (parsed.hostname.includes("youtu.be")) {
        return parsed.pathname.replace("/", "").slice(0, 11);
      }
      const v = parsed.searchParams.get("v");
      if (v && v.length === 11) return v;
      const parts = parsed.pathname.split("/").filter(Boolean);
      const markers = ["shorts", "embed", "live"];
      for (let i = 0; i < parts.length - 1; i++) {
        if (markers.includes(parts[i].toLowerCase()) && parts[i + 1].length >= 11) {
          return parts[i + 1].slice(0, 11);
        }
      }
    } catch {
      return null;
    }
    return null;
  }

  function watchTitle() {
    const heading = document.querySelector("h1.ytd-watch-metadata yt-formatted-string, h1 yt-formatted-string");
    if (heading && heading.textContent.trim()) {
      return heading.textContent.trim();
    }
    return document.title.replace(/\s+-\s+YouTube$/, "").trim();
  }

  function reportIfNeeded(video) {
    if (!currentVideoId || reported.has(currentVideoId) || !video || !video.duration) {
      return;
    }
    if (video.currentTime / video.duration < THRESHOLD) {
      return;
    }

    reported.add(currentVideoId);
    chrome.runtime.sendMessage({
      type: "listened",
      videoId: currentVideoId,
      title: watchTitle(),
      url: "https://www.youtube.com/watch?v=" + currentVideoId
    });
  }

  function attach(video) {
    if (attachedVideo === video) {
      return;
    }
    if (attachedVideo) {
      attachedVideo.removeEventListener("timeupdate", onTimeUpdate);
      attachedVideo.removeEventListener("ended", onEnded);
    }
    attachedVideo = video;
    if (!video) {
      return;
    }
    video.addEventListener("timeupdate", onTimeUpdate);
    video.addEventListener("ended", onEnded);
  }

  function onTimeUpdate(event) {
    reportIfNeeded(event.target);
  }

  function onEnded(event) {
    reportIfNeeded(event.target);
  }

  function sync() {
    const videoId = videoIdFromUrl(location.href);
    if (videoId !== currentVideoId) {
      currentVideoId = videoId;
    }
    attach(document.querySelector("video"));
  }

  document.addEventListener("yt-navigate-finish", sync);
  setInterval(sync, 1500);
  sync();
})();
