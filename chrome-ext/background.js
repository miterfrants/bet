import {
    RESPONSE_STATUS,
    API
} from './config.js';

import {
    extendStringProtoType
} from './util/extended-prototype.js';

import {
    ExtDataService
} from './data-service/ext.js';
extendStringProtoType(); ;
ExtDataService.Init(API, RESPONSE_STATUS);
window.ExtDataService = ExtDataService;

chrome.runtime.onMessage.addListener((req, sender, sendResponse) => {
    if (req.isSyncGithub) {
        injectGithub(req.userId, req.token, req.earnCoins, req.betCoins);
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

function injectGithub (userId, token, earnCoins, betCoins) {
    chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {
        chrome.tabs.executeScript(tabs[0].id, {
            code: `
      window['homo.bargainingChip.token'] = '${token}';
      window['homo.bargainingChip.earnCoins'] = ${earnCoins};
      window['homo.bargainingChip.betCoins'] = ${betCoins};
      window['homo.bargainingChip.userId'] = ${userId};
      window['homo.bargainingChip.apiEndpoint'] = ${API.ENDPOINT};
    `
        }, function () {
            chrome.tabs.executeScript(tabs[0].id, { file: '/inject-github.js' });
        });
    });
}
