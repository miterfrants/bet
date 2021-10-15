export const UI = {
    Init: async (storage) => {
        if (storage.token) {
            UI.HideUnauthSection();
        } else {
            UI.HideAuthSection();
        }
    },
    HideUnauthSection: () => {
        document.querySelector('.unauth').style.display = 'none';
        document.querySelector('.auth').style.display = 'block';
    },
    HideAuthSection: () => {
        document.querySelector('.unauth').style.display = 'block';
        document.querySelector('.auth').style.display = 'none';
    }
}