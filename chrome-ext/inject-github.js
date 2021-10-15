
if (!window.Homo) {
    let coinLogUpdateTimer;
    window.Homo = {
        Bet: {
            DebounceUpdateCoinLog: (apiEndpoint, token, taskId, qty, callback) => {
                clearTimeout(coinLogUpdateTimer);
                coinLogUpdateTimer = setTimeout(async () => {
                    await Homo.Bet.UpdateCoinLog(apiEndpoint, token, taskId, qty, callback);
                }, 2000);
            },
            UpdateCoinLog: async (apiEndpoint, token, taskId, qty, callback) => {
                const updateAction = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/${taskId}/update-current-coin-log`, {
                    method: 'POST',
                    headers: {
                        Authorization: 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        qty: -qty
                    })
                });
                const resp = await updateAction.json();
                callback(resp);
            }
        },
        Task: {
            Assign: async (apiEndpoint, token, taskId, workDays, callback) => {
                const updateAction = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/${taskId}/assign`, {
                    method: 'POST',
                    headers: {
                        Authorization: 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        workDays
                    })
                });
                const resp = await updateAction.json();
                callback(resp);
            },
            MarkFinish: async (apiEndpoint, token, taskId, callback) => {
                const action = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/${taskId}/mark-finish`, {
                    method: 'POST',
                    headers: {
                        Authorization: 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    }
                });
                const resp = await action.json();
                callback(resp);
            },
            Done: async (apiEndpoint, token, taskId, callback) => {
                const action = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/${taskId}/done`, {
                    method: 'POST',
                    headers: {
                        Authorization: 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    }
                });
                const resp = await action.json();
                callback(resp);
            }

        }
    };
}

if (location.origin === 'https://github.com' && location.pathname === '/miterfrants/homo-iot-hub/issues') {
    (() => {
        // add coins
        const variablePrefix = 'homo.bargainingChip.';
        const token = window[`${variablePrefix}token`];
        const userId = window[`${variablePrefix}userId`];
        const apiEndpoint = window[`${variablePrefix}apiEndpoint`];
        const betCoins = Number(window[`${variablePrefix}betCoins`]);
        const coinIconHtml = '<div class="overflow-hidden" style="width:20px; height: 20px; margin-left: 10px; margin-right: 10px"><img style="width: 100%; height: 100%; object-fit: contain;" src="https://bet.homo.tw/assets/imgs/coin.png" /> </div> X ';
        const elNotification = document.querySelector('notification-indicator');
        const elNotificationParent = elNotification.parentNode;
        const elHeader = elNotificationParent.parentNode;

        if (elHeader.dataset.injected !== 'true') {
            const elHeaderItemBetIcon = document.createElement('div');
            ['Header-item', 'mr-0', 'mr-md-3', 'flex-order-1', 'flex-md-order-none'].forEach(item => {
                elHeaderItemBetIcon.classList.add(item);
            });
            elHeaderItemBetIcon.innerHTML = `${coinIconHtml}<span class="homo-bet-coins">${betCoins}</span> `;
            elHeaderItemBetIcon.style.whiteSpace = 'nowrap';
            elHeader.insertBefore(elHeaderItemBetIcon, elNotificationParent);
            elHeaderItemBetIcon.querySelector('.homo-bet-coins').dataset[`${variablePrefix}betCoins`] = betCoins;
            elHeader.dataset.injected = 'true';
        }

        const elIssues = document.querySelectorAll('[aria-label="Issues"] > div > div');
        elIssues.forEach(async (elIssue) => {
            const id = elIssue.dataset.id;
            let resp;
            const fetchAction = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/by-external-id/${id}`, {
                headers: {
                    Authorization: 'Bearer ' + token
                }
            });
            if (fetchAction.status === 404) {
                const createAction = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks`, {
                    method: 'POST',
                    headers: {
                        Authorization: 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        externalId: id,
                        type: 0,
                        name: ''
                    })
                });
                resp = await createAction.json();
            } else {
                resp = await fetchAction.json();
            }

            const elTitle = elIssue.querySelector('.markdown-title');

            const qty = resp.ownerLockedBet + resp.ownerFreeBet + resp.excludeOwnerBet;
            elIssue.dataset[`${variablePrefix}ownerLockedBet`] = resp.ownerLockedBet;
            elIssue.dataset[`${variablePrefix}ownerFreeBet`] = resp.ownerFreeBet;
            elIssue.dataset[`${variablePrefix}excludeOwnerBet`] = resp.excludeOwnerBet;
            elIssue.dataset[`${variablePrefix}currentCoinLogId`] = resp.currentCoinLogId;
            elIssue.dataset[`${variablePrefix}taskId`] = resp.id;
            let title = '';
            if (elIssue.dataset.injected !== 'true') {
                elTitle.style.whiteSpace = 'nowrap';
                elTitle.classList.add('d-flex');
                title = elTitle.innerHTML;
                elTitle.dataset.title = title;
            } else {
                title = elTitle.dataset.title;
            }

            elTitle.innerHTML = `${title} ${coinIconHtml} <span class="subtotal">${qty}</span>`;
            elIssue.classList.add('issue');

            if (elIssue.querySelectorAll('.buttons').length === 0) {
                const elButtons = document.createElement('div');
                elButtons.classList.add('buttons');
                elButtons.innerHTML = `
                    <button class="d-none homo-bargaining-add px-3 py-1">+</button>
                    <span class="your-bet"></span>
                    <button class="d-none homo-bargaining-minus ml-3 px-3 py-1">-</button>
                    <button class="btn-claim d-none">Claim</button>
                    <div class="assignee d-none"></div>
                    <div class="exptected-finish-at d-none"></div>
                    <button class="d-none btn-mark-finish">Mark Finish</button>
                    <button class="d-none btn-done">Done</button>
                `;
                elIssue.append(elButtons);
            }

            if (resp.assigneeId) {
                elIssue.querySelector('.btn-claim').classList.add('d-none');
                elIssue.querySelector('.assignee').classList.remove('d-none');
                elIssue.querySelector('.assignee').innerHTML = `${resp.assignee?.lastName}${resp.assignee?.firstName}`;
                elIssue.querySelector('.exptected-finish-at').classList.remove('d-none');
                elIssue.querySelector('.exptected-finish-at').innerHTML = resp.expectedFinishAt.substring(0, resp.expectedFinishAt.indexOf('T'));
                elIssue.querySelector('.homo-bargaining-minus').classList.add('d-none');
                elIssue.querySelector('.homo-bargaining-add').classList.add('d-none');
            } else {
                elIssue.querySelector('.btn-claim').classList.remove('d-none');
                elIssue.querySelector('.assignee').classList.add('d-none');
                elIssue.querySelector('.assignee').innerHTML = '';
                elIssue.querySelector('.exptected-finish-at').classList.remove('d-none');
                elIssue.querySelector('.exptected-finish-at').innerHTML = '';
                elIssue.querySelector('.homo-bargaining-minus').classList.remove('d-none');
                elIssue.querySelector('.homo-bargaining-add').classList.remove('d-none');
            }

            if (resp.status < 3) {
                const isAssignee = userId === resp.assigneeId;
                const beMarkedFinish = resp.status === 2;
                if (isAssignee) {
                    elIssue.querySelector('.btn-mark-finish').classList.remove('d-none');
                } else {
                    elIssue.querySelector('.btn-mark-finish').classList.add('d-none');
                }
                if (!isAssignee && beMarkedFinish) {
                    elIssue.querySelector('.btn-done').classList.remove('d-none');
                } else {
                    elIssue.querySelector('.btn-done').classList.add('d-none');
                }
            }

            if (elIssue.dataset.injected !== 'true') {
                elIssue.querySelector('.homo-bargaining-add').addEventListener('click', (e) => {
                    e.stopPropagation();
                    e.preventDefault();
                    const elIssue = e.currentTarget.closest('.issue');
                    let ownerFreeBet = Number(elIssue.dataset[`${variablePrefix}ownerFreeBet`]);
                    const elBetCoins = document.querySelector('.homo-bet-coins');
                    let betCoins = Number(elBetCoins.dataset[`${variablePrefix}betCoins`]);
                    if (betCoins <= 0) {
                        return;
                    }
                    ownerFreeBet += 1;
                    betCoins -= 1;
                    const ownerLockedBet = Number(elIssue.dataset[`${variablePrefix}ownerLockedBet`]);
                    const excludeOwnerBet = Number(elIssue.dataset[`${variablePrefix}excludeOwnerBet`]);
                    elIssue.dataset[`${variablePrefix}ownerFreeBet`] = ownerFreeBet;
                    elBetCoins.dataset[`${variablePrefix}betCoins`] = betCoins;
                    elBetCoins.innerHTML = betCoins;
                    elIssue.querySelector('.subtotal').innerHTML = ownerFreeBet + ownerLockedBet + excludeOwnerBet;
                    const taskId = Number(elIssue.dataset[`${variablePrefix}taskId`]);
                    Homo.Bet.DebounceUpdateCoinLog(apiEndpoint, token, taskId, ownerFreeBet, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            return;
                        }
                        alert(resp.message);
                    });
                });

                elIssue.querySelector('.homo-bargaining-minus').addEventListener('click', (e) => {
                    e.stopPropagation();
                    e.preventDefault();
                    const elIssue = e.currentTarget.closest('.issue');
                    let ownerFreeBet = Number(elIssue.dataset[`${variablePrefix}ownerFreeBet`]);
                    const elBetCoins = document.querySelector('.homo-bet-coins');
                    let betCoins = Number(elBetCoins.dataset[`${variablePrefix}betCoins`]);
                    if (ownerFreeBet <= 0) {
                        return;
                    }
                    ownerFreeBet -= 1;
                    betCoins += 1;
                    const ownerLockedBet = Number(elIssue.dataset[`${variablePrefix}ownerLockedBet`]);
                    const excludeOwnerBet = Number(elIssue.dataset[`${variablePrefix}excludeOwnerBet`]);
                    elIssue.dataset[`${variablePrefix}ownerFreeBet`] = ownerFreeBet;
                    elBetCoins.dataset[`${variablePrefix}betCoins`] = betCoins;
                    elBetCoins.innerHTML = betCoins;
                    elIssue.querySelector('.subtotal').innerHTML = ownerFreeBet + ownerLockedBet + excludeOwnerBet;
                    const taskId = Number(elIssue.dataset[`${variablePrefix}taskId`]);
                    Homo.Bet.DebounceUpdateCoinLog(apiEndpoint, token, taskId, ownerFreeBet, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            return;
                        }
                        alert(resp.message);
                    });
                });

                elIssue.querySelector('.btn-claim').addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const taskId = Number(elIssue.dataset[`${variablePrefix}taskId`]);
                    const days = prompt('預計完成的時間 days');
                    Homo.Task.Assign(apiEndpoint, token, taskId, days, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            return;
                        }
                        alert(resp.message);
                    });
                });

                elIssue.querySelector('.btn-mark-finish').addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const taskId = Number(elIssue.dataset[`${variablePrefix}taskId`]);
                    Homo.Task.MarkFinish(apiEndpoint, token, taskId, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            elIssue.querySelector('.btn-mark-finish').classList.add('d-none');
                            return;
                        }
                        alert(resp.message);
                    });
                });

                elIssue.querySelector('.btn-done').addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const taskId = Number(elIssue.dataset[`${variablePrefix}taskId`]);
                    Homo.Task.Done(apiEndpoint, token, taskId, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            elIssue.querySelector('.btn-done').classList.add('d-none');
                            return;
                        }
                        alert(resp.message);
                    });
                });
            }
            elIssue.dataset.injected = 'true';
        });
    })();
}
