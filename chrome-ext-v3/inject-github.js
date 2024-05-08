window.coinIconHtml = '<div class="overflow-hidden" style="width:20px; height: 20px; margin-left: 10px; margin-right: 10px"><img style="width: 100%; height: 100%; object-fit: contain;" src="https://bet.homo.tw/assets/imgs/coin.png" /> </div> X ';
window.injectHead = (betCoins) => {
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
        elHeaderItemBetIcon.innerHTML = `${coinIconHtml}<span class="homo-bet-coins" style="padding-left:8px;">${betCoins}</span> `;
        elHeaderItemBetIcon.style.whiteSpace = 'nowrap';
        elHeader.insertBefore(elHeaderItemBetIcon, elHeaderItem);
        elHeaderItemBetIcon.querySelector('.homo-bet-coins').dataset[`${window.variablePrefix}betCoins`] = betCoins;
        elHeader.dataset.injected = 'true';
    }
};

window.injectIssueButton = async (elIssue) => {
    const elInjectContainer = elIssue.querySelector('div>div:nth-child(3)');
    const elLink = elIssue.querySelector('div>a');
    const externalId = elLink.id.split('_')[1];
    const storage = await chrome.storage.sync.get(['userInfo']);
    chrome.runtime.sendMessage({ action: 'get-issue-status', externalId }, (issueStatus) => {
        const elTitle = elIssue.querySelector('.markdown-title');
        const qty = issueStatus.ownerLockedBet + issueStatus.ownerFreeBet + issueStatus.excludeOwnerBet;
        elIssue.dataset[`${window.variablePrefix}ownerLockedBet`] = issueStatus.ownerLockedBet;
        elIssue.dataset[`${window.variablePrefix}ownerFreeBet`] = issueStatus.ownerFreeBet;
        elIssue.dataset[`${window.variablePrefix}excludeOwnerBet`] = issueStatus.excludeOwnerBet;
        elIssue.dataset[`${window.variablePrefix}currentCoinLogId`] = issueStatus.currentCoinLogId;
        elIssue.dataset[`${window.variablePrefix}externalId`] = issueStatus.id;
        let title = '';
        if (elIssue.dataset.injected !== 'true') {
            elTitle.style.whiteSpace = 'nowrap';
            elTitle.classList.add('d-flex');
            title = elTitle.innerHTML;
            elTitle.dataset.title = title;
        } else {
            title = elTitle.dataset.title;
        }

        elTitle.innerHTML = `${title} ${window.coinIconHtml} <span class="subtotal">${qty}</span>`;
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

            if (issueStatus.assigneeId) {
                elIssue.querySelector('.btn-claim').classList.add('d-none');
                elIssue.querySelector('.assignee').classList.remove('d-none');
                elIssue.querySelector('.assignee').innerHTML = `${issueStatus.assignee?.lastName}${issueStatus.assignee?.firstName}`;
                elIssue.querySelector('.exptected-finish-at').classList.remove('d-none');
                elIssue.querySelector('.exptected-finish-at').innerHTML = issueStatus.expectedFinishAt.substring(0, issueStatus.expectedFinishAt.indexOf('T'));
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

            if (issueStatus.status < 3) {
                const isAssignee = storage.userInfo.id === issueStatus.assigneeId;
                const beMarkedFinish = issueStatus.status === 2;
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
                    let ownerFreeBet = Number(elIssue.dataset[`${window.variablePrefix}ownerFreeBet`]);
                    const elBetCoins = document.querySelector('.homo-bet-coins');
                    let betCoins = Number(elBetCoins.dataset[`${window.variablePrefix}betCoins`]);
                    if (betCoins <= 0) {
                        return;
                    }
                    ownerFreeBet += 1;
                    betCoins -= 1;
                    const ownerLockedBet = Number(elIssue.dataset[`${window.variablePrefix}ownerLockedBet`]);
                    const excludeOwnerBet = Number(elIssue.dataset[`${window.variablePrefix}excludeOwnerBet`]);
                    elIssue.dataset[`${window.variablePrefix}ownerFreeBet`] = ownerFreeBet;
                    elBetCoins.dataset[`${window.variablePrefix}betCoins`] = betCoins;
                    elBetCoins.innerHTML = betCoins;
                    elIssue.querySelector('.subtotal').innerHTML = ownerFreeBet + ownerLockedBet + excludeOwnerBet;
                    const externalId = Number(elIssue.dataset[`${window.variablePrefix}externalId`]);
                    chrome.runtime.sendMessage({ action: 'update-coin', externalId, freeCoins: ownerFreeBet, betCoins });
                });

                elIssue.querySelector('.homo-bargaining-minus').addEventListener('click', (e) => {
                    e.stopPropagation();
                    e.preventDefault();
                    const elIssue = e.currentTarget.closest('.issue');
                    let ownerFreeBet = Number(elIssue.dataset[`${window.variablePrefix}ownerFreeBet`]);
                    const elBetCoins = document.querySelector('.homo-bet-coins');
                    let betCoins = Number(elBetCoins.dataset[`${window.variablePrefix}betCoins`]);
                    if (ownerFreeBet <= 0) {
                        return;
                    }
                    ownerFreeBet -= 1;
                    betCoins += 1;
                    const ownerLockedBet = Number(elIssue.dataset[`${window.variablePrefix}ownerLockedBet`]);
                    const excludeOwnerBet = Number(elIssue.dataset[`${window.variablePrefix}excludeOwnerBet`]);
                    elIssue.dataset[`${window.variablePrefix}ownerFreeBet`] = ownerFreeBet;
                    elBetCoins.dataset[`${window.variablePrefix}betCoins`] = betCoins;
                    elBetCoins.innerHTML = betCoins;
                    elIssue.querySelector('.subtotal').innerHTML = ownerFreeBet + ownerLockedBet + excludeOwnerBet;
                    const externalId = Number(elIssue.dataset[`${window.variablePrefix}externalId`]);
                    chrome.runtime.sendMessage({ action: 'update-coin', externalId, freeCoins: ownerFreeBet, betCoins });
                });

                elIssue.querySelector('.btn-claim').addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const externalId = Number(elIssue.dataset[`${window.variablePrefix}externalId`]);
                    const days = prompt('預計完成的時間 days');
                    if (isNaN(days)) {
                        alert('為填入預計完成時間');
                    }
                    chrome.runtime.sendMessage({ action: 'claim', externalId, workDays: days }, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            elIssue.querySelector('.btn-claim').classList.add('d-none');
                            elIssue.querySelector('.homo-bargaining-minus').classList.add('d-none');
                            elIssue.querySelector('.assignee').classList.remove('d-none');
                            chrome.storage.sync.get(['userInfo']).then((storage) => {
                                elIssue.querySelector('.assignee').innerHTML = `${storage.userInfo.lastName}${storage.userInfo.firstName}`;
                            });
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
                    const externalId = Number(elIssue.dataset[`${window.variablePrefix}externalId`]);
                    chrome.runtime.sendMessage({ action: 'mark-finish', externalId }, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            elIssue.querySelector('.btn-mark-finish').classList.add('d-none');
                            return;
                        }
                        alert(resp.message);
                    });
                });

                elIssue.querySelector('.btn-done').addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const externalId = Number(elIssue.dataset[`${window.variablePrefix}externalId`]);
                    chrome.runtime.sendMessage({ action: 'done', externalId }, (resp) => {
                        if (resp.status && resp.status === 'OK') {
                            elIssue.querySelector('.btn-done').classList.add('d-none');
                            return;
                        }
                        alert(resp.message);
                    });
                });
            }
            elIssue.dataset.injected = 'true';
        }
    });
};

window.variablePrefix = 'homo.bet.';
if (location.origin === 'https://github.com' && location.pathname === '/miterfrants/itemhub/issues') {
    (async () => {
        const storage = await chrome.storage.sync.get(['token', 'userInfo', 'earnCoins', 'betCoins']);
        window.injectHead(storage.betCoins);
        const elIssues = document.querySelectorAll('[aria-label="Issues"] > div > div');
        elIssues.forEach(async (elIssue) => {
            window.injectIssueButton(elIssue);
        });
    })();
}
