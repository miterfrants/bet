const apiEndPoint = 'https://bet.homo.tw/api/v1';
const API = {
    ENDPOINT: apiEndPoint,
    AUTH: apiEndPoint + '/auth/auth-from-chrome-ext',
    COINS_EARN: apiEndPoint + '/coins/earn',
    COINS_BET: apiEndPoint + '/coins/bet'
};

chrome.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
    if (changeInfo.status === 'complete' &&
            tab.url === 'https://github.com/miterfrants/itemhub/issues'
    ) {
        console.log('index github', tab.url, tab.id);
        chrome.scripting.executeScript({
            target: { tabId },
            files: ['inject-github.js']
        });
    }
});

chrome.runtime.onMessage.addListener((req, sender, sendResponse) => {
    if (req.action === 'get-issue-status') {
        chrome.storage.sync.get(['token']).then(storage => {
            getIssueStatus(storage.token, req.externalId).then(issueStatus => {
                sendResponse(issueStatus);
            });
        });
    } else if (req.action === 'update-coin') {
        chrome.storage.sync.get(['token']).then(storage => {
            debounceUpdateCoinLog(storage.token, req.externalId, req.freeCoins, req.betCoins, sendResponse);
        });
    } else if (req.action === 'claim') {
        chrome.storage.sync.get(['token']).then(storage => {
            claim(storage.token, req.externalId, req.workDays, sendResponse);
        });
    } else if (req.action === 'mark-finish') {
        chrome.storage.sync.get(['token']).then(storage => {
            markFinish(storage.token, req.externalId, sendResponse);
        });
    } else if (req.action === 'done') {
        chrome.storage.sync.get(['token']).then(storage => {
            done(storage.token, req.externalId, sendResponse);
        });
    } else if (req.action === 'buy') {
        chrome.storage.sync.get(['token']).then(storage => {
            buy(storage.token, req.name, req.value, sendResponse);
        });
    } else if (req.action === 'transfer') {
        chrome.storage.sync.get(['token']).then(storage => {
            transfer(storage.token, req.receiverId, req.qty, sendResponse);
        });
    }
    return true;
});

const coinLogUpdateTimerArray = {};
function debounceUpdateCoinLog (token, externalId, freeCoins, betCoins, callback) {
    clearTimeout(coinLogUpdateTimerArray[externalId]);
    coinLogUpdateTimerArray[externalId] = setTimeout(async () => {
        await updateCoinLog(token, externalId, freeCoins, betCoins, callback);
    }, 2000);
}

async function transfer (token, receiverId, qty, callback) {
    const resp = await fetch(`${API.ENDPOINT}/coins/transfer`, {
        method: 'POST',
        headers: {
            Authorization: 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            receiverId,
            qty
        })
    });
    const respOfBuy = await resp.json();
    callback(respOfBuy);
}

async function buy (token, name, value, callback) {
    const resp = await fetch(`${API.ENDPOINT}/goods/buy`, {
        method: 'POST',
        headers: {
            Authorization: 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            name,
            value
        })
    });
    const respOfBuy = await resp.json();
    callback(respOfBuy);
}

async function claim (token, externalId, workDays, callback) {
    const resp = await fetch(`${API.ENDPOINT}/organizations/2/projects/6/tasks/${externalId}/assign`, {
        method: 'POST',
        headers: {
            Authorization: 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            workDays
        })
    });
    const respOfClaim = await resp.json();
    callback(respOfClaim);
}

async function markFinish (token, externalId, callback) {
    const resp = await fetch(`${API.ENDPOINT}/organizations/2/projects/6/tasks/${externalId}/mark-finish`, {
        method: 'POST',
        headers: {
            Authorization: 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    });
    const respOfMarkFinish = await resp.json();
    callback(respOfMarkFinish);
}

async function done (token, externalId, callback) {
    const resp = await fetch(`${API.ENDPOINT}/organizations/2/projects/6/tasks/${externalId}/done`, {
        method: 'POST',
        headers: {
            Authorization: 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    });
    const respOfDone = await resp.json();
    callback(respOfDone);
}

async function updateCoinLog (token, externalId, freeCoins, betCoins, callback) {
    const updateAction = await fetch(`${API.ENDPOINT}/organizations/2/projects/6/tasks/${externalId}/update-current-coin-log`, {
        method: 'POST',
        headers: {
            Authorization: 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            qty: -freeCoins
        })
    });
    if (updateAction.status === 200) {
        chrome.storage.sync.set({
            betCoins
        });
    }
    const resp = await updateAction.json();
    callback(resp);
}

async function getIssueStatus (token, externalId) {
    const option = {
        method: 'GET',
        headers: {
            Authorization: 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    };
    const resp = await fetch(`${API.ENDPOINT}/organizations/2/projects/6/tasks/by-external-id/${externalId}`, option);
    if (resp.status === 200) {
        return await resp.json();
    }
    if (resp.status === 404) {
        const createAction = await fetch(`${API.ENDPOINT}/organizations/2/projects/6/tasks`, {
            method: 'POST',
            headers: {
                Authorization: 'Bearer ' + token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                externalId,
                type: 0,
                name: ''
            })
        });
        await createAction.json();
        const respOfNewIssue = await fetch(`${API.ENDPOINT}/organizations/2/projects/6/tasks/by-external-id/${externalId}`, {
            headers: {
                Authorization: 'Bearer ' + token
            }
        });
        return await respOfNewIssue.json();
    }
}
