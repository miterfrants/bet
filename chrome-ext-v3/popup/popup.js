import { RESPONSE_STATUS } from '../config.js';
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
            if (respEarnCoins.data.errorKey === 'TOKEN_EXPIRED') {
                chrome.storage.sync.set({
                    token: '',
                    userInfo: ''
                });
                init();
                return;
            }
            chrome.storage.sync.set({
                betCoins: respBetCoins.data.qty,
                earnCoins: respEarnCoins.data.qty
            });
            document.querySelector('.bet-coins').innerHTML = respBetCoins.data.qty;
            document.querySelector('.earn-coins').innerHTML = respEarnCoins.data.qty;

            document.querySelector('.auth .profile img').src = storage.userInfo.profile;
            document.querySelector('.auth .name').innerHTML = `${storage.userInfo.lastName}${storage.userInfo.firstName}`;

            const respOfGetShareholding = await Data.GetShareholding(storage.token);
            const respOfGetCoinsPerWeek = await Data.GetCoinsPerWeek(storage.token);
            console.log(respOfGetShareholding);
            console.log(respOfGetCoinsPerWeek);
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

document.querySelectorAll('.store .good button').forEach(item => {
    item.addEventListener('click', (event) => {
        // move to runway
        const elParent = event.currentTarget.parentNode.parentNode;
        const template = elParent.querySelector('.good').dataset.template;
        const cost = elParent.querySelector('.good').dataset.cost;
        const name = elParent.querySelector('.good').dataset.name;
        const runway = document.querySelector('.store .runway');
        runway.classList.add('confirm');
        const confirm = runway.querySelector('.confirm');
        confirm.dataset.template = template;
        confirm.dataset.name = name;
        confirm.dataset.cost = cost;
        confirm.dataset.value = 1;
        confirm.querySelector('h2').innerHTML = template.replace('{number}', 1);
    });
});

document.querySelector('.btn-cancel-confirm').addEventListener('click', (e) => {
    const runway = document.querySelector('.store .runway');
    runway.classList.remove('confirm');
});

document.querySelector('.confirm .add').addEventListener('click', async (e) => {
    const elConfirm = e.currentTarget.parentNode.parentNode;
    const template = elConfirm.dataset.template;
    const value = Number(elConfirm.dataset.value);
    const cost = Number(elConfirm.dataset.cost);
    const storage = await chrome.storage.sync.get(['earnCoins']);
    if ((value + 1) * cost > storage.earnCoins) {
        return null;
    }
    elConfirm.querySelector('h2').innerHTML = template.replace('{number}', value + 1);
    elConfirm.dataset.value = value + 1;
});

document.querySelector('.confirm .minus').addEventListener('click', async (e) => {
    const elConfirm = e.currentTarget.parentNode.parentNode;
    const template = elConfirm.dataset.template;
    const value = Number(elConfirm.dataset.value);
    if (value <= 1) {
        return null;
    }
    elConfirm.querySelector('h2').innerHTML = template.replace('{number}', value - 1);
    elConfirm.dataset.value = value - 1;
});

document.querySelector('.confirm .btn-buy').addEventListener('click', (e) => {
    const elConfirm = e.currentTarget.parentNode.parentNode;
    chrome.runtime.sendMessage({ action: 'buy', name: elConfirm.dataset.name, value: Number(elConfirm.dataset.value) }, (resp) => {
        if (resp.status === RESPONSE_STATUS.OK) {
            alert('購買成功');
            elConfirm.parentNode.classList.remove('confirm');
            init();
        } else {
            alert('發生錯誤');
        }
    });
});
