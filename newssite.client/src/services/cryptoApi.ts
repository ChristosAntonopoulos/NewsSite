interface BinanceTickerResponse {
    symbol: string;
    lastPrice: string;
    priceChangePercent: string;
}

export interface CryptoData {
    symbol: string;
    price: number;
    change: number;
}

export class CryptoApi {
    private static readonly BINANCE_API_BASE = 'https://api.binance.com/api/v3';
    private static readonly TOP_VOLUMES_LIMIT = 20;

    async getTopCryptos(): Promise<CryptoData[]> {
        try {
            // Get 24hr ticker for all symbols
            const response = await fetch(`${CryptoApi.BINANCE_API_BASE}/ticker/24hr`);
            const data: BinanceTickerResponse[] = await response.json();

            // Filter for USDT pairs and sort by volume
            const usdtPairs = data
                .filter(ticker => ticker.symbol.endsWith('USDT'))
                .sort((a, b) => 
                    parseFloat(b.lastPrice) * parseFloat(b.priceChangePercent) - 
                    parseFloat(a.lastPrice) * parseFloat(a.priceChangePercent)
                )
                .slice(0, CryptoApi.TOP_VOLUMES_LIMIT);

            // Transform to our CryptoData format
            return usdtPairs.map(ticker => ({
                symbol: ticker.symbol.replace('USDT', ''),
                price: parseFloat(ticker.lastPrice),
                change: parseFloat(ticker.priceChangePercent)
            }));
        } catch (error) {
            console.error('Error fetching crypto data:', error);
            return [];
        }
    }
}

export const cryptoApi = new CryptoApi(); 