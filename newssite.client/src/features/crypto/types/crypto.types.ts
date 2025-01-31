export interface CryptoPrice {
    symbol: string;
    price: number;
    change24h: number;
    volume24h: number;
    lastUpdated: string;
}

export interface CryptoState {
    prices: CryptoPrice[];
    loading: boolean;
    error: string | null;
} 