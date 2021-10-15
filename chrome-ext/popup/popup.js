import {
    Data
} from '../popup/data.js';

import {
    UI
} from '../popup/ui.js';

init();
function init () {
    chrome.storage.sync.get(['token', 'userInfo'], async (storage) => {
        if (storage.token) {
            const respEarnCoins = await Data.GetEarnCoinsAsync(storage.token);
            const respBetCoins = await Data.GetBetCoinsAsync(storage.token);
            document.querySelector('.bet-coins').innerHTML = respBetCoins.data.qty;
            document.querySelector('.earn-coins').innerHTML = respEarnCoins.data.qty;

            document.querySelector('.auth .profile img').src = storage.userInfo.profile;
            document.querySelector('.auth .name').innerHTML = `${storage.userInfo.lastName}${storage.userInfo.firstName}`;
        }
        UI.Init(storage);
    });
}

document.querySelector('.auth-google').addEventListener('click', async () => {
    try {
        const resp = await Data.AuthAsync();
        chrome.storage.sync.set({
            token: resp.data.token,
            userInfo: resp.data.userInfo
        });
        init();
    } catch (error) {}
});

document.querySelector('.sync-git-hub-issue').addEventListener('click', async () => {
    chrome.storage.sync.get(['token', 'userInfo'], async (storage) => {
        Data.SyncGithub(storage.userInfo.id, storage.token, document.querySelector('.bet-coins').innerHTML, document.querySelector('.earn-coins').innerHTML);
    });
});
