import {
    RESPONSE_STATUS
} from '../config.js';
export const Data = {
    AuthAsync: () => {
        return new Promise(function (resolve, reject) {
            chrome.runtime.sendMessage({
                service: 'ExtDataService',
                module: 'Auth',
                action: 'Post'
            }, (resp) => {
                if (resp.status === RESPONSE_STATUS.OK) {
                    resolve(resp);
                } else {
                    alert(resp.data.message);
                    reject(resp.data.message);
                }
            });
        });
    },
    GetEarnCoinsAsync: (token) => {
        return new Promise(function (resolve, reject) {
            chrome.runtime.sendMessage({
                service: 'ExtDataService',
                module: 'Coins',
                action: 'GetEarn',
                data: {
                    token
                }
            }, (resp) => {
                if (resp.status === RESPONSE_STATUS.OK) {
                    resolve(resp);
                } else {
                    alert(resp.data.message);
                    reject(resp.data.message);
                }
            });
        });
    },
    GetBetCoinsAsync: (token) => {
        return new Promise(function (resolve, reject) {
            chrome.runtime.sendMessage({
                service: 'ExtDataService',
                module: 'Coins',
                action: 'GetBet',
                data: {
                    token
                }
            }, (resp) => {
                if (resp.status === RESPONSE_STATUS.OK) {
                    resolve(resp);
                } else {
                    alert(resp.data.message);
                    reject(resp.data.message);
                }
            });
        });
    },
    SyncGithub: (userId, name, token, betCoins, earnCoins) => {
        chrome.runtime.sendMessage({
            isSyncGithub: true,
            userId,
            name,
            token,
            betCoins,
            earnCoins
        });
    }
};
