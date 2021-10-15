let _API, _RESPONSE_STATUS;
export const ExtDataService = {
    Init: (API, RESPONSE_STATUS) => {
        _API = API;
        _RESPONSE_STATUS = RESPONSE_STATUS;
    },
    Auth: {
        Post: async (data, sendResponse) => {
            const resp = await new Promise(function (resolve, reject) {
                chrome.identity.getAuthToken({
                    interactive: true
                }, async (code) => {
                    // check token is validated
                    const fetchOption = {
                        method: 'POST',
                        body: JSON.stringify({
                            code: code,
                            provider: 2
                        })
                    };
                    const resp = await _fetch(_API.AUTH, fetchOption);

                    if (resp.status === 200) {
                        const userInfo = await resp.json();
                        const token = userInfo.token;
                        delete userInfo.token;
                        resolve({
                            status: _RESPONSE_STATUS.OK,
                            data: {
                                token: token,
                                userInfo: userInfo
                            }
                        });
                    } else {
                        resolve({
                            status: _RESPONSE_STATUS.FAILED,
                            data: {
                                errorMsg: 'auth fail'
                            }
                        });
                    }
                });
            });
            sendResponse(resp);
        }
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
