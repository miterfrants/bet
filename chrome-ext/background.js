import {
    RESPONSE_STATUS,
    API
} from './config.js';

import {
    ExtDataService
} from './data-service/ext.js';

ExtDataService.Init(API, RESPONSE_STATUS);
window.ExtDataService = ExtDataService;

chrome.runtime.onMessage.addListener((req, sender, sendResponse) => {
    if (req.isSyncGithub) {
        chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {
            injectGithub(tabs[0].id, req.userId, req.name, req.token, req.earnCoins, req.betCoins);
        });
        return;
    }
    chrome.storage.sync.get(['token', 'userInfo'], function (storage) {
        const data = {
            username: storage.userInfo ? storage.userInfo.name : null,
            token: storage.token || null,
            profile_url: storage.userInfo ? storage.userInfo.picture : null
        };
        window[req.service][req.module][req.action]({
            ...req.data,
            ...data
        }, sendResponse, true);
    });
    return true;
});

chrome.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
    if (changeInfo.status === 'complete' && tab.url === 'https://github.com/miterfrants/itemhub/issues') {
        chrome.storage.sync.get(['token', 'userInfo'], async (storage) => {
            injectGithub(
                tabId,
                storage.userInfo.id,
                `${storage.userInfo.lastName}${storage.userInfo.firstName}`,
                storage.token,
                -1,
                -1
            );
        });
    }
});

function injectGithub (tabId, userId, name, token, earnCoins, betCoins) {
    chrome.tabs.executeScript(tabId, {
        code: `
      window['homo.bet.token'] = '${token}';
      window['homo.bet.earnCoins'] = ${earnCoins};
      window['homo.bet.betCoins'] = ${betCoins};
      window['homo.bet.userId'] = ${userId};
      window['homo.bet.apiEndpoint'] = '${API.ENDPOINT}';
      window['homo.bet.name'] = '${name}';
    `
    }, function () {
        chrome.tabs.executeScript(tabId, { file: '/inject-github.js' });
    });
}
