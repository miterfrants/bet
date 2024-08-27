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
            console.log(respOfGetShareholding.data);
            document.querySelector('.shareholding-rate').innerHTML = `${Math.round(respOfGetShareholding.data.mine / respOfGetShareholding.data.all * 10000) / 100} %`;
            document.querySelector('.coins-per-week').innerHTML = respOfGetCoinsPerWeek.data.coinsPerWeek + 10;

            const respOfUsers = await Data.GetUsers(storage.token);
            document.querySelector('.receiver').innerHTML = respOfUsers.data.filter(item => item.id !== storage.userInfo.id).map(item => `<option value="${item.id}">${item.username}</option>`).join('');
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

document.querySelector('.transfer-coins').addEventListener('keyup', (e) => {
    if (e.currentTarget.value !== '') {
        document.querySelector('.transfer-viewport button').removeAttribute('disabled');
    } else {
        document.querySelector('.transfer-viewport button').setAttribute('disabled', '');
    }
});

document.querySelector('.transfer-viewport button').addEventListener('click', (e) => {
    const receiverId = document.querySelector('.receiver').value;
    const receiverName = document.querySelector('.receiver option:checked').innerHTML;
    const qty = Number(document.querySelector('.transfer-coins').value);
    const elTransferViewport = document.querySelector('.transfer-viewport');
    elTransferViewport.classList.add('confirm');
    const elWarning = elTransferViewport.querySelector('h3');
    elWarning.innerHTML = `請確認轉帳 $ ${qty} 給 ${receiverName} `;
    elWarning.dataset.receiverId = receiverId;
    elWarning.dataset.qty = qty;
});

document.querySelectorAll('.btn-cancel-confirm').forEach(item => {
    item.addEventListener('click', (e) => {
        const runway = e.currentTarget.parentNode.parentNode.parentNode;
        runway.classList.remove('confirm');
    });
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

document.querySelector('.store .confirm .btn-buy').addEventListener('click', (e) => {
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

document.querySelector('.transfer-viewport .confirm .btn-buy').addEventListener('click', (e) => {
    const elConfirm = e.currentTarget.parentNode.parentNode;
    const elWarning = elConfirm.querySelector('h3');
    chrome.runtime.sendMessage({ action: 'transfer', qty: Number(elWarning.dataset.qty), receiverId: Number(elWarning.dataset.receiverId) }, (resp) => {
        if (resp.status === RESPONSE_STATUS.OK) {
            alert('轉帳成功');
            elConfirm.parentNode.classList.remove('confirm');
            init();
        } else {
            alert('發生錯誤');
        }
    });
});
