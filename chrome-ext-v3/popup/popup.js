import { RESPONSE_STATUS } from '../config.js';
import { Data } from '../popup/data.js';

import { UI } from '../popup/ui.js';

init();
function init() {
    chrome.storage.sync.get(['token', 'userInfo'], async (storage) => {
        if (storage.token) {
            const respEarnCoins = await Data.GetEarnCoinsAsync(storage.token);
            const respBetCoins = await Data.GetBetCoinsAsync(storage.token);
            if (respEarnCoins.data.errorKey === 'TOKEN_EXPIRED') {
                chrome.storage.sync.set({
                    token: '',
                    userInfo: '',
                });
                init();
                return;
            }
            chrome.storage.sync.set({
                betCoins: respBetCoins.data.qty,
                earnCoins: respEarnCoins.data.qty,
            });
            document.querySelector('.bet-coins').innerHTML =
                respBetCoins.data.qty;
            document.querySelector('.earn-coins').innerHTML =
                respEarnCoins.data.qty;

            document.querySelector('.auth .profile img').src =
                storage.userInfo.profile;
            document.querySelector(
                '.auth .name'
            ).innerHTML = `${storage.userInfo.lastName}${storage.userInfo.firstName}`;

            const respOfGetShareholding = await Data.GetShareholding(
                storage.token
            );
            const respOfGetCoinsPerWeek = await Data.GetCoinsPerWeek(
                storage.token
            );
            const respOfGetThisMonthSickLeaveDays =
                await Data.GetThisMonthSickLeaveDays(storage.token);

            const respOfGetThisMonthMenstruationLeaveDays =
                await Data.GetThisMonthMenstruationLeaveDays(storage.token);

            if (respOfGetThisMonthSickLeaveDays.data.days >= 2) {
                document
                    .querySelector('.sick-leave-good')
                    .classList.add('hidden');
            }

            if (respOfGetThisMonthMenstruationLeaveDays.data.days >= 1) {
                document
                    .querySelector('.menstruation-leave-good')
                    .classList.add('hidden');
            }

            document.querySelector('.shareholding-rate').innerHTML = `${
                Math.round(
                    (respOfGetShareholding.data.mine /
                        respOfGetShareholding.data.all) *
                        10000
                ) / 100
            } %`;
            document.querySelector('.coins-per-week').innerHTML =
                respOfGetCoinsPerWeek.data.coinsPerWeek + 10;

            const respOfUsers = await Data.GetUsers(storage.token);
            document.querySelector('.receiver').innerHTML = respOfUsers.data
                .filter((item) => item.id !== storage.userInfo.id)
                .map(
                    (item) =>
                        `<option value="${item.id}">${item.username}</option>`
                )
                .join('');
        }
        UI.Init(storage);
    });
}

document.querySelector('.auth-google').addEventListener('click', async () => {
    try {
        const resp = await Data.AuthAsync();
        chrome.storage.sync.set({
            token: resp.data.token,
            userInfo: resp.data.userInfo,
        });
        init();
    } catch (error) {}
});

document.querySelectorAll('.store .good button').forEach((item) => {
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
        if (name.indexOf('假') !== -1) {
            confirm.querySelector('.leave-date').classList.remove('hidden');
        } else {
            confirm.querySelector('.leave-date').classList.add('hidden');
        }
        confirm.querySelector('h2').innerHTML = template.replace('{number}', 1);
    });
});

document.querySelector('.transfer-coins').addEventListener('keyup', (e) => {
    if (e.currentTarget.value !== '') {
        document
            .querySelector('.transfer-viewport button')
            .removeAttribute('disabled');
    } else {
        document
            .querySelector('.transfer-viewport button')
            .setAttribute('disabled', '');
    }
});

document
    .querySelector('.transfer-viewport button')
    .addEventListener('click', (e) => {
        const receiverId = document.querySelector('.receiver').value;
        const receiverName = document.querySelector(
            '.receiver option:checked'
        ).innerHTML;
        const qty = Number(document.querySelector('.transfer-coins').value);
        const elTransferViewport = document.querySelector('.transfer-viewport');
        elTransferViewport.classList.add('confirm');
        const elWarning = elTransferViewport.querySelector('h3');
        elWarning.innerHTML = `請確認轉帳 $ ${qty} 給 ${receiverName} `;
        elWarning.dataset.receiverId = receiverId;
        elWarning.dataset.qty = qty;
    });

document.querySelectorAll('.btn-cancel-confirm').forEach((item) => {
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
    elConfirm.querySelector('h2').innerHTML = template.replace(
        '{number}',
        value + 1
    );
    elConfirm.dataset.value = value + 1;
});

document
    .querySelector('.confirm .minus')
    .addEventListener('click', async (e) => {
        const elConfirm = e.currentTarget.parentNode.parentNode;
        const template = elConfirm.dataset.template;
        const value = Number(elConfirm.dataset.value);
        if (value <= 1) {
            return null;
        }
        elConfirm.querySelector('h2').innerHTML = template.replace(
            '{number}',
            value - 1
        );
        elConfirm.dataset.value = value - 1;
    });

document
    .querySelector('.store .confirm .btn-buy')
    .addEventListener('click', (e) => {
        const elConfirm = e.currentTarget.parentNode.parentNode;
        chrome.runtime.sendMessage(
            {
                action: 'buy',
                name: elConfirm.dataset.name,
                value: Number(elConfirm.dataset.value),
                leaveDate: elConfirm.querySelector('.leave-date').value,
            },
            (resp) => {
                if (resp.status === RESPONSE_STATUS.OK) {
                    alert('購買成功');
                    elConfirm.parentNode.classList.remove('confirm');
                    init();
                } else {
                    alert('發生錯誤');
                }
            }
        );
    });

document
    .querySelector('.transfer-viewport .confirm .btn-buy')
    .addEventListener('click', (e) => {
        const elConfirm = e.currentTarget.parentNode.parentNode;
        const elWarning = elConfirm.querySelector('h3');
        chrome.runtime.sendMessage(
            {
                action: 'transfer',
                qty: Number(elWarning.dataset.qty),
                receiverId: Number(elWarning.dataset.receiverId),
            },
            (resp) => {
                if (resp.status === RESPONSE_STATUS.OK) {
                    alert('轉帳成功');
                    elConfirm.parentNode.classList.remove('confirm');
                    init();
                } else {
                    alert('發生錯誤');
                }
            }
        );
    });

// ============ 卡片系統 ============

// 卡片類型對應的 CSS class
const CARD_TYPE_CLASS = {
    'MAGIC': 'magic',
    'TRAP': 'trap',
    'BUFF': 'buff'
};

// 卡片類型的中文名稱
const CARD_TYPE_NAME = {
    'MAGIC': '魔法卡',
    'TRAP': '陷阱卡',
    'BUFF': '增益卡'
};

// 初始化卡片系統
async function initCardSystem() {
    const storage = await chrome.storage.sync.get(['token']);
    if (!storage.token) return;

    // 從 API 取得商店卡片並渲染
    const respCards = await Data.GetCards(storage.token);
    if (respCards.status === RESPONSE_STATUS.OK) {
        renderCardStore(respCards.data.cards);
        // 初始化確認對話框事件監聽
        initCardBuyConfirmEvents();
    }

    // 從 API 取得使用者卡片並渲染
    renderUserCards();
}

// 渲染卡片購物區
function renderCardStore(cards) {
    const cardsListEl = document.querySelector('.cards-list');

    if (!cards || cards.length === 0) {
        cardsListEl.innerHTML = '<p style="color: #999; font-size: 12px;">目前沒有可購買的卡片</p>';
        return;
    }

    cardsListEl.innerHTML = cards.map(card => `
        <div class="card-item"
             data-card-id="${card.id}"
             data-card-name="${card.name}"
             data-card-type="${card.type}"
             data-card-description="${card.description}"
             data-card-cost="${card.cost}">
            <div class="card-name">${card.name}</div>
            <span class="card-type ${CARD_TYPE_CLASS[card.type]}">${CARD_TYPE_NAME[card.type]}</span>
            <div class="card-description">${card.description}</div>
            <div class="card-cost">
                <div class="cost-amount">
                    ${card.cost} X <img class="coin pad-l-5" src="/imgs/coin.png" />
                </div>
                <button data-card-id="${card.id}">購買</button>
            </div>
        </div>
    `).join('');

    // 綁定購買按鈕事件
    document.querySelectorAll('.card-item button').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.stopPropagation();
            const cardItem = e.currentTarget.closest('.card-item');

            // 顯示購買確認對話框
            await showCardBuyConfirm(
                Number(cardItem.dataset.cardId),
                cardItem.dataset.cardName,
                cardItem.dataset.cardType,
                cardItem.dataset.cardDescription,
                Number(cardItem.dataset.cardCost)
            );
        });
    });
}

// 顯示卡片購買確認對話框
async function showCardBuyConfirm(cardId, cardName, cardType, cardDescription, cardCost) {
    // 獲取當前餘額
    const storage = await chrome.storage.sync.get(['earnCoins']);
    const currentBalance = storage.earnCoins || 0;
    const remainingBalance = currentBalance - cardCost;

    // 檢查餘額是否足夠
    if (currentBalance < cardCost) {
        alert('餘額不足，無法購買此卡片');
        return;
    }

    // 更新確認對話框信息
    const cardRunway = document.querySelector('.card-runway');
    const cardConfirm = cardRunway.querySelector('.card-confirm');

    // 填充卡片信息
    cardConfirm.querySelector('.card-confirm-name').textContent = cardName;
    cardConfirm.querySelector('.card-confirm-type').className = `card-confirm-type ${CARD_TYPE_CLASS[cardType]}`;
    cardConfirm.querySelector('.card-confirm-type').textContent = CARD_TYPE_NAME[cardType];
    cardConfirm.querySelector('.card-confirm-description').textContent = cardDescription;

    // 填充成本信息
    cardConfirm.querySelector('.card-confirm-cost').textContent = cardCost;
    cardConfirm.querySelector('.current-balance').textContent = currentBalance;
    cardConfirm.querySelector('.remaining-balance').textContent = remainingBalance;

    // 存儲卡片 ID 以供確認時使用
    cardConfirm.dataset.cardId = cardId;

    // 顯示確認對話框（添加 confirm class 觸發滑動動畫）
    cardRunway.classList.add('confirm');
}

// 初始化卡片購買確認對話框的事件監聽
function initCardBuyConfirmEvents() {
    const cardRunway = document.querySelector('.card-runway');
    const cardConfirm = cardRunway.querySelector('.card-confirm');

    // 取消按鈕
    cardConfirm.querySelector('.btn-cancel-confirm').addEventListener('click', (e) => {
        cardRunway.classList.remove('confirm');
    });

    // 確認購買按鈕
    cardConfirm.querySelector('.btn-card-buy').addEventListener('click', async (e) => {
        const cardId = Number(cardConfirm.dataset.cardId);

        // 調用購買函數
        await buyCardConfirmed(cardId);

        // 關閉確認對話框
        cardRunway.classList.remove('confirm');
    });
}

// 確認購買卡片（API 調用）
async function buyCardConfirmed(cardId) {
    const storage = await chrome.storage.sync.get(['token']);
    if (!storage.token) {
        alert('請先登入');
        return;
    }

    // 調用 API 購買卡片
    const resp = await Data.BuyCard(storage.token, cardId);

    if (resp.status === RESPONSE_STATUS.OK) {
        alert('購買成功！');
        // 重新初始化以更新存款和卡片列表
        init();
        renderUserCards();
    } else {
        alert(resp.data.errorMsg || '購買失敗');
    }
}

// 渲染使用者卡片側邊欄
async function renderUserCards() {
    const storage = await chrome.storage.sync.get(['token']);
    if (!storage.token) return;

    // 從 API 取得使用者所有卡片和已裝備的卡片
    const respMyCards = await Data.GetMyCards(storage.token);
    const respEquippedCards = await Data.GetEquippedCards(storage.token);

    if (respMyCards.status !== RESPONSE_STATUS.OK || respEquippedCards.status !== RESPONSE_STATUS.OK) {
        return;
    }

    const userCards = respMyCards.data.cards || [];
    const equippedCards = respEquippedCards.data.cards || [];

    // 渲染已裝備的卡片
    const equippedCardsListEl = document.querySelector('.equipped-cards-list');
    if (equippedCards.length === 0) {
        equippedCardsListEl.innerHTML = '<p style="color: #999; font-size: 12px;">尚未裝備任何卡片</p>';
    } else {
        equippedCardsListEl.innerHTML = equippedCards.map(card => `
            <div class="user-card-item equipped" data-user-card-id="${card.id}">
                <div class="equipped-badge">已裝備</div>
                <div class="card-name">${card.name}</div>
                <span class="card-type ${CARD_TYPE_CLASS[card.type]}">${CARD_TYPE_NAME[card.type]}</span>
            </div>
        `).join('');
    }

    // 渲染擁有的卡片
    const ownedCardsListEl = document.querySelector('.owned-cards-list');
    if (userCards.length === 0) {
        ownedCardsListEl.innerHTML = '<p style="color: #999; font-size: 12px;">尚未擁有任何卡片</p>';
    } else {
        ownedCardsListEl.innerHTML = userCards.map(card => {
            const isEquipped = card.isEquipped;
            const isMagicCard = card.type === 'MAGIC';

            return `
                <div class="user-card-item ${isEquipped ? 'equipped' : ''}"
                     data-user-card-id="${card.id}"
                     data-card-name="${card.name}"
                     data-card-type="${card.type}">
                    ${isEquipped ? '<div class="equipped-badge">已裝備</div>' : ''}
                    <div class="card-name">${card.name}</div>
                    <span class="card-type ${CARD_TYPE_CLASS[card.type]}">${CARD_TYPE_NAME[card.type]}</span>
                    <div class="card-actions">
                        ${isMagicCard ?
                            `<button class="use-btn" data-user-card-id="${card.id}">使用</button>` :
                            `<button class="equip-btn ${isEquipped ? '' : 'equip'}"
                                    data-user-card-id="${card.id}"
                                    ${isEquipped || equippedCards.length >= 3 ? 'disabled' : ''}>
                                ${isEquipped ? '已裝備' : (equippedCards.length >= 3 ? '已滿' : '裝備')}
                            </button>`
                        }
                    </div>
                </div>
            `;
        }).join('');

        // 綁定裝備按鈕事件（陷阱卡和增益卡）
        document.querySelectorAll('.equip-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                if (e.currentTarget.disabled) return;

                const cardItem = e.currentTarget.closest('.user-card-item');
                const userCardId = Number(cardItem.dataset.userCardId);
                const cardName = cardItem.dataset.cardName;
                const cardType = cardItem.dataset.cardType;

                // 顯示裝備確認對話框
                showEquipConfirm(userCardId, cardName, cardType);
            });
        });

        // 綁定使用按鈕事件（魔法卡）
        document.querySelectorAll('.use-btn').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                const cardItem = e.currentTarget.closest('.user-card-item');
                const userCardId = Number(cardItem.dataset.userCardId);
                const cardName = cardItem.dataset.cardName;

                // 使用魔法卡
                await useMagicCard(userCardId, cardName);
            });
        });
    }
}

// 顯示裝備確認對話框
function showEquipConfirm(userCardId, cardName, cardType) {
    const equipRunway = document.querySelector('.equip-runway');
    const equipConfirm = equipRunway.querySelector('.equip-confirm');

    // 填充卡片信息
    equipConfirm.querySelector('.equip-confirm-name').textContent = cardName;
    equipConfirm.querySelector('.equip-confirm-type').className = `equip-confirm-type ${CARD_TYPE_CLASS[cardType]}`;
    equipConfirm.querySelector('.equip-confirm-type').textContent = CARD_TYPE_NAME[cardType];

    // 存儲卡片 ID 以供確認時使用
    equipConfirm.dataset.userCardId = userCardId;

    // 顯示確認對話框（添加 confirm class 觸發滑動動畫）
    equipRunway.classList.add('confirm');
}

// 初始化裝備確認對話框的事件監聽
function initEquipConfirmEvents() {
    const equipRunway = document.querySelector('.equip-runway');
    const equipConfirm = equipRunway.querySelector('.equip-confirm');

    // 取消按鈕
    equipConfirm.querySelector('.btn-cancel-equip').addEventListener('click', (e) => {
        equipRunway.classList.remove('confirm');
    });

    // 確認裝備按鈕
    equipConfirm.querySelector('.btn-confirm-equip').addEventListener('click', async (e) => {
        const userCardId = Number(equipConfirm.dataset.userCardId);

        // 調用裝備函數
        await equipCardConfirmed(userCardId);

        // 關閉確認對話框
        equipRunway.classList.remove('confirm');
    });
}

// 確認裝備卡片（API 調用）
async function equipCardConfirmed(userCardId) {
    const storage = await chrome.storage.sync.get(['token']);
    if (!storage.token) {
        alert('請先登入');
        return;
    }

    // 調用 API 裝備卡片
    const resp = await Data.EquipCard(storage.token, userCardId);

    if (resp.status === RESPONSE_STATUS.OK) {
        // 重新渲染
        renderUserCards();
    } else {
        alert(resp.data.errorMsg || '裝備失敗');
    }
}

// 使用魔法卡
async function useMagicCard(userCardId, cardName) {
    const storage = await chrome.storage.sync.get(['token']);
    if (!storage.token) {
        alert('請先登入');
        return;
    }

    // 顯示使用確認 alert
    alert(`現在使用 ${cardName}`);

    // 調用 API 使用卡片
    const resp = await Data.UseCard(storage.token, userCardId);

    if (resp.status === RESPONSE_STATUS.OK) {
        // 重新渲染卡片列表
        renderUserCards();
    } else {
        alert(resp.data.errorMsg || '使用失敗');
    }
}

// 裝備卡片
async function equipCard(userCardId) {
    const storage = await chrome.storage.sync.get(['token']);
    if (!storage.token) {
        alert('請先登入');
        return;
    }

    // 調用 API 裝備卡片
    const resp = await Data.EquipCard(storage.token, userCardId);

    if (resp.status === RESPONSE_STATUS.OK) {
        // 重新渲染
        renderUserCards();
    } else {
        alert(resp.data.errorMsg || '裝備失敗');
    }
}

// 卸下卡片
async function unequipCard(userCardId) {
    const storage = await chrome.storage.sync.get(['token']);
    if (!storage.token) {
        alert('請先登入');
        return;
    }

    // 調用 API 卸下卡片
    const resp = await Data.UnequipCard(storage.token, userCardId);

    if (resp.status === RESPONSE_STATUS.OK) {
        // 重新渲染
        renderUserCards();
    } else {
        alert(resp.data.errorMsg || '卸下失敗');
    }
}

// Profile 點擊事件 - 顯示側邊欄
document.querySelector('.profile.clickable').addEventListener('click', () => {
    const mainViewport = document.querySelector('.main-viewport');
    mainViewport.classList.add('show-sidebar');
});

// 關閉側邊欄
document.querySelector('.close-sidebar').addEventListener('click', () => {
    const mainViewport = document.querySelector('.main-viewport');
    mainViewport.classList.remove('show-sidebar');
});

// 初始化卡片系統
initCardSystem();

// 初始化裝備確認事件
initEquipConfirmEvents();
