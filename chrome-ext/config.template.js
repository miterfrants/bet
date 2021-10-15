const env = 'dev'; // dev or prod
const apiEndPoint = env === 'dev' ? '{DEV_API_ENDPOINT}' : '{PRODUCTION_API_ENDPOINT}';
export const RESPONSE_STATUS = {
    OK: 'OK',
    FAILED: 'FAILED'
};
export const API = {
    ENDPOINT: apiEndPoint,
    AUTH: apiEndPoint + '/auth/auth-from-chrome-ext',
    COINS_EARN: apiEndPoint + '/coins/earn',
    COINS_BET: apiEndPoint + '/coins/bet'
};
