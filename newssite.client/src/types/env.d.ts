/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  readonly VITE_BINANCE_API_URL?: string
  readonly MODE: string
  readonly DEV: boolean
  readonly PROD: boolean
  // Add other env variables here
}

interface ImportMeta {
  readonly env: ImportMetaEnv
} 