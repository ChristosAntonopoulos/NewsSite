import React from 'react'
import { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { SunIcon, MoonIcon, QueueListIcon, Bars3Icon, UserCircleIcon } from '@heroicons/react/24/outline'
import { LanguageSwitcher } from '../../../features/language/components/LanguageSwitcher'
import { useLanguage } from '../../../contexts/LanguageContext'
import { useTheme } from '../../../contexts/ThemeContext'
import { useAuth } from '../../../contexts/AuthContext'
import { CryptoTicker } from '../../../features/crypto/pages/CryptoPrices'
import { TrendingTopics } from './TrendingTopics'
import { useState } from 'react'

interface LayoutProps {
  children: ReactNode
}

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { t } = useLanguage()
  const { theme, toggleTheme } = useTheme()
  const { user, logout } = useAuth()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const [userMenuOpen, setUserMenuOpen] = useState(false)

  // Debug theme changes
  console.log('[Layout] Current theme:', theme);

  return (
    <div className={`min-h-screen bg-white text-gray-900 dark:bg-gray-900 dark:text-white transition-colors duration-200 ${theme}`}>
      <CryptoTicker />
      <nav className="bg-gray-800 mt-10">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center justify-between">
            <div className="flex items-center">
              <Link to="/" className="flex-shrink-0">
                <h1 className="text-white text-xl font-bold">{t('common.siteTitle')}</h1>
              </Link>
              <div className="hidden md:block">
                <div className="ml-10 flex items-baseline space-x-4">
                  <Link
                    to="/"
                    className="text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                  >
                    {t('common.newsFeed')}
                  </Link>
                  <Link
                    to="/saved"
                    className="text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                  >
                    {t('common.savedArticles')}
                  </Link>
                 
                </div>
              </div>
            </div>
            <div className="hidden md:flex items-center gap-4">
              <Link
                to="/pipelines"
                className="inline-flex items-center gap-2 text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
              >
                <QueueListIcon className="h-5 w-5" />
                {t('common.pipelines')}
              </Link>
              
              {user ? (
                <div className="relative">
                  <button
                    onClick={() => setUserMenuOpen(!userMenuOpen)}
                    className="flex items-center gap-2 text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                  >
                    <UserCircleIcon className="h-5 w-5" />
                    <span>{t('common.welcome')}, {user.name}</span>
                  </button>
                  
                  {userMenuOpen && (
                    <div className="absolute right-0 mt-2 w-48 rounded-md shadow-lg bg-white dark:bg-gray-700 ring-1 ring-black ring-opacity-5">
                      <div className="py-1">                       
                        <button
                          onClick={logout}
                          className="block w-full text-left px-4 py-2 text-sm text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-600"
                        >
                          {t('common.logout')}
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <>
                  <Link
                    to="/login"
                    className="text-cyan-400 hover:text-cyan-300 px-3 py-2 rounded-md text-sm font-medium"
                  >
                    {t('common.login')}
                  </Link>
                  <Link
                    to="/register"
                    className="bg-cyan-600 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-cyan-700"
                  >
                    {t('common.signUp')}
                  </Link>
                </>
              )}
              
              <div className="relative">
                <button
                  onClick={() => {
                    console.log('[Layout] Theme toggle clicked');
                    toggleTheme();
                  }}
                  className="p-2 rounded-full text-gray-300 hover:bg-gray-700 hover:text-white transition-colors"
                  title={theme === 'light' ? t('common.darkMode') : t('common.lightMode')}
                >
                  {theme === 'light' ? <MoonIcon className="h-6 w-6" /> : <SunIcon className="h-6 w-6" />}
                </button>
              </div>
            </div>
            <div className="md:hidden">
              <button
                onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                className="p-2 rounded-md text-gray-300 hover:bg-gray-700 hover:text-white"
              >
                <Bars3Icon className="h-6 w-6" />
              </button>
            </div>
          </div>
          
          {/* Mobile menu */}
          {mobileMenuOpen && (
            <div className="md:hidden">
              <div className="px-2 pt-2 pb-3 space-y-1">
                <Link
                  to="/"
                  className="block text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-base font-medium"
                >
                  {t('common.newsFeed')}
                </Link>
                <Link
                  to="/saved"
                  className="block text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-base font-medium"
                >
                  {t('common.savedArticles')}
                </Link>
                <Link
                  to="/preferences"
                  className="block text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-base font-medium"
                >
                  {t('common.preferences')}
                </Link>
                <Link
                  to="/pipelines"
                  className="block text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-base font-medium"
                >
                  {t('common.pipelines')}
                </Link>
                
                {user ? (
                  <>
                    <div className="block text-gray-300 px-3 py-2 text-base font-medium">
                      {t('common.welcome')}, {user.name}
                    </div>
                    <Link
                      to="/profile"
                      className="block text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-base font-medium"
                    >
                      {t('common.profile')}
                    </Link>
                    <button
                      onClick={logout}
                      className="block w-full text-left text-gray-300 hover:bg-gray-700 hover:text-white px-3 py-2 rounded-md text-base font-medium"
                    >
                      {t('common.logout')}
                    </button>
                  </>
                ) : (
                  <>
                    <Link
                      to="/login"
                      className="block text-cyan-400 hover:text-cyan-300 px-3 py-2 rounded-md text-base font-medium"
                    >
                      {t('common.login')}
                    </Link>
                    <Link
                      to="/register"
                      className="block bg-cyan-600 text-white px-3 py-2 rounded-md text-base font-medium hover:bg-cyan-700"
                    >
                      {t('common.signUp')}
                    </Link>
                  </>
                )}
                
                <div className="px-3 py-2">
                  <LanguageSwitcher />
                </div>
              </div>
            </div>
          )}
        </div>
      </nav>

      <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8">
        <main className="w-full lg:pr-48">
          {children}
        </main>
        <aside className="hidden lg:block fixed right-8 top-1/2 transform -translate-y-1/2">
          <TrendingTopics />
        </aside>
      </div>

      <footer className="bg-gray-800 text-white mt-auto">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-4 text-center">
          <p>{t('common.footer')}</p>
        </div>
      </footer>
    </div>
  )
}