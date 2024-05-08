let _API, _RESPONSE_STATUS;
export const ExtDataService = {
    Init: (API, RESPONSE_STATUS) => {
        _API = API;
        _RESPONSE_STATUS = RESPONSE_STATUS;
    },
    Coins: {
        GetEarn: async (data, sendResponse) => {
            const fetchOption = {
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + data.token
                }
            };
            const resp = await _fetch(_API.COINS_EARN, fetchOption);
            sendResponse({
                status: _RESPONSE_STATUS.OK,
                data: await resp.json()
            });
        },
        GetBet: async (data, sendResponse) => {
            const fetchOption = {
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + data.token
                }
            };
            const resp = await _fetch(_API.COINS_BET, fetchOption);
            sendResponse({
                status: _RESPONSE_STATUS.OK,
                data: await resp.json()
            });
        }
    }
};

const _fetch = (url, option, withCatch) => {
    if (option.cache) {
        console.warn('Cound not declate cache in option params');
    }
    const newOption = {
        ...option,
        headers: {
            ...option.headers,
            'Content-Type': 'application/json'
        }
    };
    if (!withCatch) {
        newOption.cache = 'no-cache';
    } else {
        newOption.cache = 'cache';
    }
    return fetch(url, newOption);
};
