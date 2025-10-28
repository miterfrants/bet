window.coinIconHtml =
    '<div class="overflow-hidden" style="width:20px; height: 20px; margin-left: 10px; margin-right: 10px"><img style="width: 100%; height: 100%; object-fit: contain;" src="https://bet.homo.tw/assets/imgs/coin.png" /> </div> X ';

// 檢查 issue 是否建立超過 7 天
window.isIssueOlderThanSevenDays = (elIssue) => {
    const relativeTimeElement = elIssue.querySelector('relative-time');
    if (!relativeTimeElement) {
        return false;
    }
    
    const datetimeStr = relativeTimeElement.getAttribute('datetime');
    if (!datetimeStr) {
        return false;
    }
    
    // 解析 GMT+0 時間
    const issueCreatedDate = new Date(datetimeStr);
    
    // 取得現在時間 (GMT+8)
    const now = new Date();
    
    // 計算 7 天前的時間
    const sevenDaysAgo = new Date(now.getTime() - (7 * 24 * 60 * 60 * 1000));
    
    // 檢查 issue 建立時間是否早於 7 天前
    return issueCreatedDate < sevenDaysAgo;
};

window.injectHead = (betCoins) => {
    const coinIconHtml =
        '<div class="overflow-hidden" style="width:20px; height: 20px; margin-left: 10px; margin-right: 10px"><img style="width: 100%; height: 100%; object-fit: contain;" src="https://bet.homo.tw/assets/imgs/coin.png" /> </div> X ';
    const elNotification = document.querySelector('notification-indicator');
    const elDetailMenu = document.querySelector('details-menu');
    const elHeaderItem = elNotification
        ? elNotification.parentNode
        : elDetailMenu.parentNode;
    const elHeader = elHeaderItem.parentNode;

    if (elHeader.dataset.injected !== 'true') {
        const elHeaderItemBetIcon = document.createElement('div');
        [
            'Header-item',
            'mr-0',
            'mr-md-3',
            'flex-order-1',
            'flex-md-order-none',
        ].forEach((item) => {
            elHeaderItemBetIcon.classList.add(item);
        });
        elHeaderItemBetIcon.innerHTML = `${coinIconHtml}<span class="homo-bet-coins" style="padding-left:8px;">${betCoins}</span> `;
        elHeaderItemBetIcon.style.whiteSpace = 'nowrap';
        elHeader.insertBefore(elHeaderItemBetIcon, elHeaderItem);
        elHeaderItemBetIcon.querySelector('.homo-bet-coins').dataset[
            `${window.variablePrefix}betCoins`
        ] = betCoins;
        elHeader.dataset.injected = 'true';
    }
};
window.githubProjectStatusChanged = (e, projectId) => {
    const optionId = e.currentTarget.value;
    const elIssue = e.currentTarget.closest('.issue');
    const elShouldBeDisabled = [
        elIssue.querySelector('.github-projects'),
        elIssue.querySelector('.github-project-status'),
    ];
    elShouldBeDisabled.forEach((item) => {
        item.setAttribute('disabled', 'disabled');
    });

    chrome.runtime.sendMessage(
        {
            action: 'update-github-project-status',
            projectId,
            statusFieldId: e.currentTarget.getAttribute('status-field-id'),
            optionId,
            connectionId: elIssue
                .querySelector('.github-projects')
                .getAttribute('connection-id'),
        },
        () => {
            elShouldBeDisabled.forEach((item) => {
                item.removeAttribute('disabled', 'disabled');
            });
        }
    );
};
window.renderGithubProjectStatusDropDown = (
    elIssue,
    githubProjects,
    githubProjectId,
    extraData
) => {
    const currentGithubProject = githubProjects.find(
        (item) => item.id === githubProjectId
    );
    const githubProjectStatusOptions = currentGithubProject
        ? currentGithubProject.status
              .map((item) => {
                  const selected = extraData.githubOptionId === item.id;
                  return `<option value="${item.id}" ${
                      selected ? 'selected' : ''
                  }>${item.name}</option>`;
              })
              .join('')
        : '';
    const githubProjectStatusDropdownList = `<select status-field-id="${currentGithubProject?.statusFieldId}" class="form-control ml-4 github-project-status" ><option>無</option>${githubProjectStatusOptions}</select>`;
    const existsElement = elIssue.querySelector('.github-project-status');
    if (existsElement) {
        existsElement.outerHTML = githubProjectStatusDropdownList;
    } else {
        elIssue
            .querySelector('.github-projects')
            .insertAdjacentHTML('afterend', githubProjectStatusDropdownList);
    }
    const elProjectStatus = elIssue.querySelector('.github-project-status');
    elProjectStatus.addEventListener('change', (e) => {
        window.githubProjectStatusChanged(e, githubProjectId);
    });
    return elProjectStatus;
};

window.injectHTMLToIssueElement = async (
    elIssue,
    extraData,
    githubProjects
) => {
    const elInjectContainer = elIssue.querySelector(
        '[class^=MainContent-module__inner]'
    );
    const storage = await chrome.storage.sync.get(['userInfo']);
    const elTitle = elIssue.querySelector(
        '[data-testid="issue-pr-title-link"]'
    );
    const qty =
        extraData.ownerLockedBet +
        extraData.ownerFreeBet +
        extraData.excludeOwnerBet;
    elIssue.dataset[`${window.variablePrefix}ownerLockedBet`] =
        extraData.ownerLockedBet;
    elIssue.dataset[`${window.variablePrefix}ownerFreeBet`] =
        extraData.ownerFreeBet;
    elIssue.dataset[`${window.variablePrefix}excludeOwnerBet`] =
        extraData.excludeOwnerBet;
    elIssue.dataset[`${window.variablePrefix}currentCoinLogId`] =
        extraData.currentCoinLogId;
    elIssue.dataset[`${window.variablePrefix}externalId`] = extraData.id;
    let title = '';
    if (elIssue.dataset.injected !== 'true') {
        elTitle.style.whiteSpace = 'nowrap';
        elTitle.classList.add('d-flex');
        title = elTitle.innerHTML;
        elTitle.dataset.title = title;
    } else {
        title = elTitle.dataset.title;
    }

    elTitle.innerHTML = `${title} <div style="display: flex;">${window.coinIconHtml} <span style="padding-left: 10px;" class="subtotal">${qty}</span></div>`;
    elIssue.classList.add('issue');
    // generate github projects dropdown
    const githubProjectOptions = githubProjects
        .map((item) => {
            const selected = extraData.githubProjectId === item.id;
            return `<option value="${item.id}" ${selected ? 'selected' : ''}>${
                item.name
            }</option>`;
        })
        .join('');
    const githubProjectDropdownList = `<select connection-id="${extraData.githubConnectionId}" class="form-control ml-4 github-projects"><option>無</option>${githubProjectOptions}</select>`;

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
                    ${githubProjectDropdownList}
                </div>
                <div class="d-flex">
                    <div class="assignee" style="color: #cfd98c;"></div>
                    <div class="exptected-finish-at ml-3" style="color: #cfd98c;"></div>
                </div>
            `;
        elInjectContainer.append(elButtons);
        window.renderGithubProjectStatusDropDown(
            elIssue,
            githubProjects,
            extraData.githubProjectId,
            extraData
        );

        // binding github project change
        elIssue
            .querySelector('.github-projects')
            .addEventListener('change', (e) => {
                const githubProjectId = e.currentTarget.value;
                if (!githubProjectId) {
                    return;
                }
                const elProjectDdl = elIssue.querySelector('.github-projects');
                elProjectDdl.setAttribute('disabled', 'disabled');
                // change options
                const elProjectStatus =
                    window.renderGithubProjectStatusDropDown(
                        elIssue,
                        githubProjects,
                        githubProjectId,
                        extraData
                    );
                elProjectStatus.setAttribute('disabled', 'disabled');

                chrome.runtime.sendMessage(
                    {
                        action: 'add-to-project',
                        projectId: githubProjectId,
                        issueId: extraData.githubIssueId,
                        originalProjectId: extraData.githubProjectId,
                        originalConnectionId: extraData.githubConnectionId,
                    },
                    (resp) => {
                        elProjectStatus.removeAttribute('disabled');
                        elProjectDdl.removeAttribute('disabled');
                        elProjectDdl.setAttribute(
                            'connection-id',
                            resp.connectionId
                        );
                        const currentGithubProject = githubProjects.find(
                            (item) => item.githubProjectId === githubProjectId
                        );
                        elProjectStatus.value = '無';
                        elProjectStatus.setAttribute(
                            'status-field-id',
                            currentGithubProject.statusFieldId
                        );
                    }
                );
            });

        if (extraData.assigneeId) {
            elIssue.querySelector('.btn-claim').classList.add('d-none');
            elIssue.querySelector('.assignee').classList.remove('d-none');
            elIssue.querySelector(
                '.assignee'
            ).innerHTML = `${extraData.assignee?.lastName}${extraData.assignee?.firstName}`;
            elIssue
                .querySelector('.exptected-finish-at')
                .classList.remove('d-none');
            elIssue.querySelector('.exptected-finish-at').innerHTML =
                extraData.expectedFinishAt.substring(
                    0,
                    extraData.expectedFinishAt.indexOf('T')
                );
            elIssue
                .querySelector('.homo-bargaining-minus')
                .classList.add('d-none');
            elIssue
                .querySelector('.homo-bargaining-add')
                .classList.remove('d-none');
        } else {
            // 檢查 issue 是否建立超過 7 天
            const isOlderThanSevenDays = window.isIssueOlderThanSevenDays(elIssue);
            
            if (isOlderThanSevenDays) {
                elIssue.querySelector('.btn-claim').classList.remove('d-none');
            } else {
                elIssue.querySelector('.btn-claim').classList.add('d-none');
            }
            
            elIssue.querySelector('.assignee').classList.add('d-none');
            elIssue.querySelector('.assignee').innerHTML = '';
            elIssue
                .querySelector('.exptected-finish-at')
                .classList.remove('d-none');
            elIssue.querySelector('.exptected-finish-at').innerHTML = '';
            elIssue
                .querySelector('.homo-bargaining-minus')
                .classList.remove('d-none');
            elIssue
                .querySelector('.homo-bargaining-add')
                .classList.remove('d-none');
        }

        if (extraData.status < 3) {
            const isAssignee = storage.userInfo.id === extraData.assigneeId;
            const beMarkedFinish = extraData.status === 2;
            if (isAssignee && !beMarkedFinish) {
                elIssue
                    .querySelector('.btn-mark-finish')
                    .classList.remove('d-none');
            } else {
                elIssue
                    .querySelector('.btn-mark-finish')
                    .classList.add('d-none');
            }
            if (!isAssignee && beMarkedFinish) {
                elIssue.querySelector('.btn-done').classList.remove('d-none');
            } else {
                elIssue.querySelector('.btn-done').classList.add('d-none');
            }
        }

        if (elIssue.dataset.injected !== 'true') {
            elIssue
                .querySelector('.homo-bargaining-add')
                .addEventListener('click', (e) => {
                    e.stopPropagation();
                    e.preventDefault();
                    const elIssue = e.currentTarget.closest('.issue');
                    let ownerFreeBet = Number(
                        elIssue.dataset[`${window.variablePrefix}ownerFreeBet`]
                    );
                    const elBetCoins =
                        document.querySelector('.homo-bet-coins');
                    let betCoins = Number(
                        elBetCoins.dataset[`${window.variablePrefix}betCoins`]
                    );
                    if (betCoins <= 0) {
                        return;
                    }
                    ownerFreeBet += 1;
                    betCoins -= 1;
                    const ownerLockedBet = Number(
                        elIssue.dataset[
                            `${window.variablePrefix}ownerLockedBet`
                        ]
                    );
                    const excludeOwnerBet = Number(
                        elIssue.dataset[
                            `${window.variablePrefix}excludeOwnerBet`
                        ]
                    );
                    elIssue.dataset[`${window.variablePrefix}ownerFreeBet`] =
                        ownerFreeBet;
                    elBetCoins.dataset[`${window.variablePrefix}betCoins`] =
                        betCoins;
                    elBetCoins.innerHTML = betCoins;
                    elIssue.querySelector('.subtotal').innerHTML =
                        ownerFreeBet + ownerLockedBet + excludeOwnerBet;
                    const externalId = Number(
                        elIssue.dataset[`${window.variablePrefix}externalId`]
                    );
                    chrome.runtime.sendMessage({
                        action: 'update-coin',
                        externalId,
                        freeCoins: ownerFreeBet,
                        betCoins,
                    });
                });

            elIssue
                .querySelector('.homo-bargaining-minus')
                .addEventListener('click', (e) => {
                    e.stopPropagation();
                    e.preventDefault();
                    const elIssue = e.currentTarget.closest('.issue');
                    let ownerFreeBet = Number(
                        elIssue.dataset[`${window.variablePrefix}ownerFreeBet`]
                    );
                    const elBetCoins =
                        document.querySelector('.homo-bet-coins');
                    let betCoins = Number(
                        elBetCoins.dataset[`${window.variablePrefix}betCoins`]
                    );
                    if (ownerFreeBet <= 0) {
                        return;
                    }
                    ownerFreeBet -= 1;
                    betCoins += 1;
                    const ownerLockedBet = Number(
                        elIssue.dataset[
                            `${window.variablePrefix}ownerLockedBet`
                        ]
                    );
                    const excludeOwnerBet = Number(
                        elIssue.dataset[
                            `${window.variablePrefix}excludeOwnerBet`
                        ]
                    );
                    elIssue.dataset[`${window.variablePrefix}ownerFreeBet`] =
                        ownerFreeBet;
                    elBetCoins.dataset[`${window.variablePrefix}betCoins`] =
                        betCoins;
                    elBetCoins.innerHTML = betCoins;
                    elIssue.querySelector('.subtotal').innerHTML =
                        ownerFreeBet + ownerLockedBet + excludeOwnerBet;
                    const externalId = Number(
                        elIssue.dataset[`${window.variablePrefix}externalId`]
                    );
                    chrome.runtime.sendMessage({
                        action: 'update-coin',
                        externalId,
                        freeCoins: ownerFreeBet,
                        betCoins,
                    });
                });

            elIssue
                .querySelector('.btn-claim')
                .addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const shouldBeDisableElements = [
                        elIssue.querySelector('.github-projects'),
                        elIssue.querySelector('.github-project-status'),
                    ];
                    const externalId = Number(
                        elIssue.dataset[`${window.variablePrefix}externalId`]
                    );
                    const days = prompt('預計完成的時間 days');
                    if (isNaN(days)) {
                        alert('為填入預計完成時間');
                    }
                    shouldBeDisableElements.forEach((item) => {
                        if (item) {
                            item.setAttribute('disabled', 'disabled');
                        }
                    });
                    chrome.runtime.sendMessage(
                        { action: 'claim', externalId, workDays: days },
                        (resp) => {
                            shouldBeDisableElements.forEach((item) => {
                                if (item) {
                                    item.removeAttribute('disabled');
                                }
                            });
                            if (resp.status && resp.status === 'OK') {
                                elIssue
                                    .querySelector('.btn-claim')
                                    .classList.add('d-none');
                                elIssue
                                    .querySelector('.homo-bargaining-minus')
                                    .classList.add('d-none');
                                elIssue
                                    .querySelector('.assignee')
                                    .classList.remove('d-none');
                                chrome.storage.sync
                                    .get(['userInfo'])
                                    .then((storage) => {
                                        elIssue.querySelector(
                                            '.assignee'
                                        ).innerHTML = `${storage.userInfo.lastName}${storage.userInfo.firstName}`;
                                    });
                                elIssue
                                    .querySelector('.exptected-finish-at')
                                    .classList.remove('d-none');
                                const result = new Date();
                                result.setDate(result.getDate() + Number(days));
                                elIssue.querySelector(
                                    '.exptected-finish-at'
                                ).innerHTML = result.toLocaleDateString();
                                return;
                            }
                            alert(resp.message);
                        }
                    );
                });

            elIssue
                .querySelector('.btn-mark-finish')
                .addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const externalId = Number(
                        elIssue.dataset[`${window.variablePrefix}externalId`]
                    );
                    chrome.runtime.sendMessage(
                        { action: 'mark-finish', externalId },
                        (resp) => {
                            if (resp.status && resp.status === 'OK') {
                                elIssue
                                    .querySelector('.btn-mark-finish')
                                    .classList.add('d-none');
                                return;
                            }
                            alert(resp.message);
                        }
                    );
                });

            elIssue
                .querySelector('.btn-done')
                .addEventListener('click', (e) => {
                    const elIssue = e.currentTarget.closest('.issue');
                    const externalId = Number(
                        elIssue.dataset[`${window.variablePrefix}externalId`]
                    );
                    chrome.runtime.sendMessage(
                        { action: 'done', externalId },
                        (resp) => {
                            if (resp.status && resp.status === 'OK') {
                                elIssue
                                    .querySelector('.btn-done')
                                    .classList.add('d-none');
                                return;
                            }
                            alert(resp.message);
                        }
                    );
                });
        }
        elIssue.dataset.injected = 'true';
    }
};

window.injectIssuesButton = async (elIssues) => {
    const externalIds = [];
    elIssues.forEach((elIssue) => {
        const elLink = elIssue.querySelector('div>h3 a');
        if (!elLink) {
            return;
        }
        const externalId = elLink
            .getAttribute('href')
            .replace('/homo-tw/itemhub/issues/', '');
        elIssue.dataset.id = externalId;
        externalIds.push(externalId);
    });
    chrome.runtime.sendMessage(
        { action: 'get-tasks', externalIds },
        (issues) => {
            chrome.runtime.sendMessage(
                { action: 'get-github-projects' },
                (githubProjects) => {
                    issues.forEach((issue) => {
                        const elIssue = document.querySelector(
                            `[data-id="${issue.externalId}"]`
                        );
                        window.injectHTMLToIssueElement(
                            elIssue,
                            issue,
                            githubProjects
                        );
                    });
                }
            );
        }
    );
};

window.injectButtonsToIssuePage = async (issueId, extraData) => {
    const storage = await chrome.storage.sync.get(['userInfo']);
    const elSidebarAssigneesSection = document.querySelector(
        '[data-testid="sidebar-assignees-section"]'
    );

    console.log(elSidebarAssigneesSection);

    if (
        !elSidebarAssigneesSection ||
        elSidebarAssigneesSection.dataset.injected === 'true'
    ) {
        return;
    }

    const isAssignee = storage.userInfo.id === extraData.assigneeId;
    const beMarkedFinish = extraData.status === 2;
    const shouldShowMarkFinishButton = extraData.status < 3 && isAssignee && !beMarkedFinish;
    const shouldShowDoneButton = extraData.status < 3 && !isAssignee && beMarkedFinish;
    
    // 檢查是否應該顯示 Claim 按鈕（沒有 assignee 且超過 7 天）
    const isOlderThanSevenDays = window.isIssueOlderThanSevenDays(document);
    const shouldShowClaimButton = !extraData.assigneeId && isOlderThanSevenDays;
    
    console.log(extraData, { shouldShowMarkFinishButton, shouldShowDoneButton, shouldShowClaimButton });
    
    if (shouldShowMarkFinishButton || shouldShowDoneButton || shouldShowClaimButton) {
        const buttonContainer = document.createElement('div');
        buttonContainer.classList.add('homo-button-container');
        buttonContainer.style.marginBottom = '16px';
        buttonContainer.style.borderBottom = '1px solid var(--borderColor-muted)';
        buttonContainer.style.width = 'calc(100% - 20px)';
        buttonContainer.style.marginLeft = '8px';
        buttonContainer.style.marginRight = '12px';
        
        let buttonsHtml = `
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
            </style>
            <h3 style="color: var(--fgColor-muted); font-size: var(--text-body-size-small); left: var(--base-size-8); pointer-events: none;">Homo Bet</h3>
        `;
        
        if (shouldShowMarkFinishButton) {
            buttonsHtml += `
                <button class="btn-mark-finish homo-btn text-sm px-3" style="margin-top: 8px; margin-bottom: 8px;">
                    <div class="p-1" style="position: relative; height: 40px; width: 40px; object-fit: cover; transform: translate(0, -8px)">
                        <img src="https://bet.homo.tw/assets/imgs/verify.png" style="left: 0; width: 100%; height: 100%; position: absolute" />
                    </div>
                    <div class="ml-2">Mark Finish</div>
                </button>
            `;
        }
        
        if (shouldShowDoneButton) {
            buttonsHtml += `
                <button class="btn-done homo-btn text-sm px-3" style="margin-top: 8px; margin-bottom: 8px;">
                    <div class="p-1" style="position: relative; height: 30px; width: 30px; object-fit: cover; transform: translate(0, -8px)">
                        <img src="https://bet.homo.tw/assets/imgs/done.png" style="left: 0; width: 100%; height: 100%; position: absolute" />
                    </div>
                    <div class="ml-2">Done</div>
                </button>
            `;
        }
        
        if (shouldShowClaimButton) {
            buttonsHtml += `
                <button class="btn-claim homo-btn text-sm px-3" style="margin-top: 8px; margin-bottom: 8px;">
                    <div class="p-1" style="position: relative; height: 40px; width: 40px; object-fit: cover; transform: translate(0, -8px)">
                        <img src="https://bet.homo.tw/assets/imgs/hand-up.png" style="left: 0; width: 100%; height: 100%; position: absolute" />
                    </div>
                    <div class="ml-2">Claim</div>
                </button>
            `;
        }
        
        buttonContainer.innerHTML = buttonsHtml;

        if (shouldShowMarkFinishButton) {
            buttonContainer
                .querySelector('.btn-mark-finish')
                .addEventListener('click', (e) => {
                    chrome.runtime.sendMessage(
                        { action: 'mark-finish', externalId: extraData.id },
                        (resp) => {
                            if (resp.status && resp.status === 'OK') {
                                buttonContainer.remove();
                                return;
                            }
                            alert(resp.message);
                        }
                    );
                });
        }

        if (shouldShowDoneButton) {
            buttonContainer
                .querySelector('.btn-done')
                .addEventListener('click', (e) => {
                    chrome.runtime.sendMessage(
                        { action: 'done', externalId: extraData.id },
                        (resp) => {
                            if (resp.status && resp.status === 'OK') {
                                buttonContainer.remove();
                                return;
                            }
                            alert(resp.message);
                        }
                    );
                });
        }

        if (shouldShowClaimButton) {
            buttonContainer
                .querySelector('.btn-claim')
                .addEventListener('click', (e) => {
                    const days = prompt('預計完成的時間 days');
                    if (isNaN(days)) {
                        alert('為填入預計完成時間');
                        return;
                    }
                    chrome.runtime.sendMessage(
                        { action: 'claim', externalId: extraData.id, workDays: days },
                        (resp) => {
                            if (resp.status && resp.status === 'OK') {
                                buttonContainer.remove();
                                return;
                            }
                            alert(resp.message);
                        }
                    );
                });
        }

        elSidebarAssigneesSection.insertBefore(
            buttonContainer,
            elSidebarAssigneesSection.firstElementChild
        );
    }

    elSidebarAssigneesSection.dataset.injected = 'true';
};

window.variablePrefix = 'homo.bet.';

// Issues 列表頁面
if (
    location.origin === 'https://github.com' &&
    location.pathname.startsWith('/homo-tw/itemhub/issues') &&
    !location.pathname.match(/\/homo-tw\/itemhub\/issues\/\d+/)
) {
    (async () => {
        const storage = await chrome.storage.sync.get([
            'token',
            'userInfo',
            'earnCoins',
            'betCoins',
        ]);
        window.injectHead(storage.betCoins);
        const elIssues = document.querySelectorAll('[role="listitem"]');
        window.injectIssuesButton(elIssues);
    })();
}

// Issue 內頁
if (
    location.origin === 'https://github.com' &&
    location.pathname.match(/\/homo-tw\/itemhub\/issues\/\d+/)
) {
    (async () => {
        const storage = await chrome.storage.sync.get([
            'token',
            'userInfo',
            'earnCoins',
            'betCoins',
        ]);
        window.injectHead(storage.betCoins);

        const issueId = location.pathname.split('/').pop();
        chrome.runtime.sendMessage(
            { action: 'get-tasks', externalIds: [issueId] },
            (issues) => {
                if (issues && issues.length > 0) {
                    window.injectButtonsToIssuePage(issueId, issues[0]);
                }
            }
        );
    })();
}
