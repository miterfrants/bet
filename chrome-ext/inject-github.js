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
            },
            Get: async (apiEndpoint, token, callback) => {
                const fetchAction = await fetch(`${apiEndpoint}/coins/bet`, {
                    method: 'GET',
                    headers: {
                        Authorization: 'Bearer ' + token,
                        'Content-Type': 'application/json'
                    }
                });
                const resp = await fetchAction.json();
                if (callback) {
                    callback(resp);
                }
                return resp;
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

if (location.origin === 'https://github.com' && location.pathname === '/miterfrants/itemhub/issues') {
    (async () => {
        // add coins
        const variablePrefix = 'homo.bet.';
        const token = window[`${variablePrefix}token`];
        const userId = window[`${variablePrefix}userId`];
        const name = window[`${variablePrefix}name`];
        const apiEndpoint = window[`${variablePrefix}apiEndpoint`];
        let betCoins = 0;
        const respOfBetCoins = await Homo.Bet.Get(apiEndpoint, token);
        betCoins = respOfBetCoins.qty;
        const coinIconHtml = '<div class="overflow-hidden" style="width:20px; height: 20px; margin-left: 10px; margin-right: 10px"><img style="width: 100%; height: 100%; object-fit: contain;" src="https://bet.homo.tw/assets/imgs/coin.png" /> </div> X ';
        const elNotification = document.querySelector('notification-indicator');
        const elDetailMenu = document.querySelector('details-menu');
        const elHeaderItem = elNotification ? elNotification.parentNode : elDetailMenu.parentNode;
        const elHeader = elHeaderItem.parentNode;

        if (elHeader.dataset.injected !== 'true') {
            const elHeaderItemBetIcon = document.createElement('div');
            ['Header-item', 'mr-0', 'mr-md-3', 'flex-order-1', 'flex-md-order-none'].forEach(item => {
                elHeaderItemBetIcon.classList.add(item);
            });
            elHeaderItemBetIcon.innerHTML = `${coinIconHtml}<span class="homo-bet-coins">${betCoins}</span> `;
            elHeaderItemBetIcon.style.whiteSpace = 'nowrap';
            elHeader.insertBefore(elHeaderItemBetIcon, elHeaderItem);
            elHeaderItemBetIcon.querySelector('.homo-bet-coins').dataset[`${variablePrefix}betCoins`] = betCoins;
            elHeader.dataset.injected = 'true';
        }

        const elIssues = document.querySelectorAll('[aria-label="Issues"] > div > div');
        elIssues.forEach(async (elIssue) => {
            const elInjectContainer = elIssue.querySelector('div>div:nth-child(3)');
            const elLink = elIssue.querySelector('div>a');
            const id = elLink.id.split('_')[1];
            const globalId = elIssue.dataset.id;

            let fetchAction = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/by-external-id/${id}`, {
                headers: {
                    Authorization: 'Bearer ' + token
                }
            });

            const fetchByGlobalId = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/by-external-id/${globalId}`, {
                headers: {
                    Authorization: 'Bearer ' + token
                }
            });

            if (fetchAction.status === 404 && fetchByGlobalId === 404) {
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
                await createAction.json();
                fetchAction = await fetch(`${apiEndpoint}/organizations/2/projects/6/tasks/by-external-id/${id}`, {
                    headers: {
                        Authorization: 'Bearer ' + token
                    }
                });
            }

            const resp = await fetchAction.json();
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
                elButtons.classList.add('mt-2');
                elButtons.style.alignItems = 'center';
                elButtons.innerHTML = `
                    <style>
                        .homo-btn {
                            background: none;
                            border: 1px solid #cfd98c;
                            height: 30px;
                            border-radius: 10px;
                            color: #cfd98c;
                            display: inline-flex;
                            align-items: center;
                        }
                        .p-25 {
                            padding: 13px !important;
                        }
                    </style>
                    <div class="d-flex" style="align-items: center">
                        <button class="d-none homo-bargaining-add p-25" style="background: transparent; width: 50px; height: 50px; object-fit: cover; border: none;">
                            <img src="https://bet.homo.tw/assets/imgs/add.png" style="width: 100%; height: 100%;" />
                        </button>
                        <span class="your-bet"></span>
                        <button class="d-none homo-bargaining-minus ml-3 p-25" style="background: transparent; width: 50px; height: 50px; object-fit: cover; border: none;">
                            <img src="https://bet.homo.tw/assets/imgs/minus.png" style="width: 100%; height: 100%;" />
                        </button>
                        <button class="btn-claim d-none homo-btn ml-4">
                            <div class="d-flex position-relative px-2">
                                <div class="p-1" style="position: absolute; height: 50px; width: 50px; object-fit: cover; top: -27px; left: -10px;">
                                    <img src="https://bet.homo.tw/assets/imgs/hand-up.png" style="width: 100%; height: 100%;" />
                                </div>
                                <div class="ml-5">Claim</div>
                            </div>
                        </button>
                        <button class="d-none btn-mark-finish text-sm d-inline-flex homo-btn ml-4 px-3">
                            <div class="p-1" style="position: relative; height: 40px; width: 40px; object-fit: cover; transform: translate(0, -8px)">
                                <img src="https://bet.homo.tw/assets/imgs/verify.png" style="left: 0; width: 100%; height: 100%; position: absolute" />
                            </div>
                            <div class="ml-2">Mark Finish</div>
                        </button>
                        <button class="d-none btn-done homo-btn text-sm ml-4 px-3">
                            <div class="p-1" style="position: relative; height: 30px; width: 30px; object-fit: cover; transform: translate(0, -8px)">
                                <img src="https://bet.homo.tw/assets/imgs/done.png" style="left: 0; width: 100%; height: 100%; position: absolute" />
                            </div>
                            <div class="ml-2">Done</div>
                        </button>
                    </div>
                    <div class="d-flex">
                        <div class="assignee" style="color: #cfd98c;"></div>
                        <div class="exptected-finish-at ml-3" style="color: #cfd98c;"></div>
                    </div>
                `;
                elInjectContainer.append(elButtons);
            }

            if (resp.assigneeId) {
                elIssue.querySelector('.btn-claim').classList.add('d-none');
                elIssue.querySelector('.assignee').classList.remove('d-none');
                elIssue.querySelector('.assignee').innerHTML = `${resp.assignee?.lastName}${resp.assignee?.firstName}`;
                elIssue.querySelector('.exptected-finish-at').classList.remove('d-none');
                elIssue.querySelector('.exptected-finish-at').innerHTML = resp.expectedFinishAt.substring(0, resp.expectedFinishAt.indexOf('T'));
                elIssue.querySelector('.homo-bargaining-minus').classList.add('d-none');
                elIssue.querySelector('.homo-bargaining-add').classList.remove('d-none');
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
                if (isAssignee && !beMarkedFinish) {
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
                    if (isNaN(days)) {
                        alert('為填入預計完成時間');
                        return;
                    }
                    Homo.Task.Assign(apiEndpoint, token, taskId, days, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            elIssue.querySelector('.btn-claim').classList.add('d-none');
                            elIssue.querySelector('.homo-bargaining-minus').classList.add('d-none');
                            elIssue.querySelector('.assignee').classList.remove('d-none');
                            elIssue.querySelector('.assignee').innerHTML = name;
                            elIssue.querySelector('.exptected-finish-at').classList.remove('d-none');
                            const result = new Date();
                            result.setDate(result.getDate() + Number(days));
                            elIssue.querySelector('.exptected-finish-at').innerHTML = result.toLocaleDateString();
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
