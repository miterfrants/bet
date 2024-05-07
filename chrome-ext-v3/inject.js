(async () => {
    await chrome.runtime.sendMessage({ isSyncGithub: true });
})();
