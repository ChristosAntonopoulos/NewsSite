import { useState, useEffect } from 'react'
import { cryptoApi, CryptoData } from '../services/cryptoApi'

export function CryptoTicker() {
  const [cryptoData, setCryptoData] = useState<CryptoData[]>([])
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true)
        const data = await cryptoApi.getTopCryptos()
        setCryptoData(data)
      } catch (error) {
        console.error('Error fetching crypto data:', error)
      } finally {
        setIsLoading(false)
      }
    }

    fetchData()
    // Update every 30 seconds
    const interval = setInterval(fetchData, 30000)
    return () => clearInterval(interval)
  }, [])

  if (isLoading && cryptoData.length === 0) {
    return (
      <div className="fixed top-0 left-0 right-0 bg-gray-900 border-b border-gray-800 overflow-hidden h-10 z-50">
        <div className="flex items-center justify-center h-full">
          <span className="text-gray-400">Loading crypto data...</span>
        </div>
      </div>
    )
  }

  return (
    <div className="fixed top-0 left-0 right-0 bg-gray-900 border-b border-gray-800 overflow-hidden h-10 z-50">
      <div className="inline-flex whitespace-nowrap animate-scroll">
        {[...cryptoData, ...cryptoData].map((crypto, index) => (
          <div 
            key={`${crypto.symbol}-${index}`}
            className={`inline-flex items-center h-10 px-4 border-r border-gray-800
              ${crypto.change >= 0 ? 'text-green-400' : 'text-red-400'}`}
          >
            {crypto.symbol} ${crypto.price.toFixed(2)} 
            {crypto.change >= 0 ? '▲' : '▼'}
            {Math.abs(crypto.change).toFixed(2)}%
          </div>
        ))}
      </div>
    </div>
  )
}

export default CryptoTicker;
