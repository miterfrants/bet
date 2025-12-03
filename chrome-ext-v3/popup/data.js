import { API, RESPONSE_STATUS } from '../config.js';
export const Data = {
    AuthAsync: async () => {
        const respOfAuthToken = await chrome.identity.getAuthToken({
            interactive: true,
        });
        const fetchOption = {
            method: 'POST',
            body: JSON.stringify({
                code: respOfAuthToken.token,
                provider: 2,
            }),
        };
        const resp = await _fetch(API.AUTH, fetchOption);

        if (resp.status === 200) {
            const userInfo = await resp.json();
            const token = userInfo.token;
            delete userInfo.token;
            return {
                status: RESPONSE_STATUS.OK,
                data: {
                    token: token,
                    userInfo: userInfo,
                },
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'auth fail',
                },
            };
        }
    },
    GetEarnCoinsAsync: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.COINS_EARN, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get earn coins error',
                },
            };
        }
    },
    GetBetCoinsAsync: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.COINS_BET, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get bet coins error',
                },
            };
        }
    },
    SyncGithub: (userId, name, token, betCoins, earnCoins) => {
        chrome.runtime.sendMessage({
            isSyncGithub: true,
            userId,
            name,
            token,
            betCoins,
            earnCoins,
        });
    },
    GetShareholding: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.SHAREHOLDING, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get bet coins error',
                },
            };
        }
    },
    GetCoinsPerWeek: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.COINS_PER_WEEK, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get bet coins error',
                },
            };
        }
    },
    GetThisMonthSickLeaveDays: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.THIS_MONTH_SICK_LEAVE_DAYS, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get bet coins error',
                },
            };
        }
    },
    GetThisMonthMenstruationLeaveDays: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(
            API.THIS_MONTH_MENSTRUATION_LEAVE_DAYS,
            fetchOption
        );
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get bet coins error',
                },
            };
        }
    },
    GetUsers: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.USERS, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get bet coins error',
                },
            };
        }
    },
    // 取得商店卡片
    GetCards: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.CARDS, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get cards error',
                },
            };
        }
    },
    // 取得使用者擁有的卡片
    GetMyCards: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.CARDS_MY_CARDS, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get my cards error',
                },
            };
        }
    },
    // 取得已裝備的卡片
    GetEquippedCards: async (token) => {
        const fetchOption = {
            method: 'GET',
            headers: {
                Authorization: 'Bearer ' + token,
            },
        };
        const resp = await _fetch(API.CARDS_EQUIPPED, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: 'get equipped cards error',
                },
            };
        }
    },
    // 購買卡片
    BuyCard: async (token, cardId) => {
        const fetchOption = {
            method: 'POST',
            headers: {
                Authorization: 'Bearer ' + token,
            },
            body: JSON.stringify({
                cardId: cardId,
            }),
        };
        const resp = await _fetch(API.CARDS_BUY, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: data.message || 'buy card error',
                },
            };
        }
    },
    // 裝備卡片
    EquipCard: async (token, userCardId) => {
        const fetchOption = {
            method: 'POST',
            headers: {
                Authorization: 'Bearer ' + token,
            },
            body: JSON.stringify({
                userCardId: userCardId,
            }),
        };
        const resp = await _fetch(API.CARDS_EQUIP, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: data.message || 'equip card error',
                },
            };
        }
    },
    // 卸下卡片
    UnequipCard: async (token, userCardId) => {
        const fetchOption = {
            method: 'POST',
            headers: {
                Authorization: 'Bearer ' + token,
            },
            body: JSON.stringify({
                userCardId: userCardId,
            }),
        };
        const resp = await _fetch(API.CARDS_UNEQUIP, fetchOption);
        if (resp.status === 200) {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.OK,
                data,
            };
        } else {
            const data = await resp.json();
            return {
                status: RESPONSE_STATUS.FAILED,
                data: {
                    errorMsg: data.message || 'unequip card error',
                },
            };
        }
    },
};

const _fetch = (url, option, withCatch) => {
    if (option.cache) {
        console.warn('Cound not declate cache in option params');
    }
    const newOption = {
        ...option,
        headers: {
            ...option.headers,
            'Content-Type': 'application/json',
        },
    };
    if (!withCatch) {
        newOption.cache = 'no-cache';
    } else {
        newOption.cache = 'cache';
    }
    return fetch(url, newOption);
};
